using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CherubimController : MonoBehaviour
{
    public HitboxController hitbox;
    public FireController fire;

    private Transform transform;
    private Animator animator;
    private PlayerController player;
    private bool inPosX;
    private bool inPosY;
    private bool facingLeft;
    private bool hit;
    private bool turning;
    private int attackTimer;
    private int attacks;
    private int primedTurns;
    private float speed;

    private const float SPACING_X = 7.2f;
    private const float SPACING_Y = 3.7f;


    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        animator = transform.Find("CherubimHead").GetComponent<Animator>();
        player = GameObject.FindObjectsOfType<PlayerController>()[0];
        inPosX = false;
        inPosY = false;
        facingLeft = true;
        hit = false;
        turning = false;
        attackTimer = Random.Range(40, 80);
        attacks = Random.Range(1, 4);
        primedTurns = Random.Range(0, 3);
        speed = 0.04f;

        if (primedTurns > 0)
        {
            StartCoroutine(HeadTurnRoutine());
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Player can't die in this stage so this is really for safety's sake
        if (player.IsDead())
        {
            animator.SetTrigger("Walk");
            transform.localScale = new Vector3(1.0f, transform.localScale.y, transform.localScale.z);
            transform.position += new Vector3(speed, 0.0f, 0.0f);
            attackTimer++;
            if (attackTimer > 1000)
            {
                transform.position += new Vector3(speed, 0.0f, 0.0f);
                //Don't despawn because despawning moves the camera around
            }
            return;
        }

        // Rotate if player passes underneath.
        if (player.GetPosition().x > transform.position.x && attacks > 0)
        {
            facingLeft = false;
            transform.localScale = new Vector3(-1.0f, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            facingLeft = true;
            transform.localScale = new Vector3(1.0f, transform.localScale.y, transform.localScale.z);
        }

        //Movement code
        float moveHorizontal = player.GetPosition().x - transform.position.x;
        float moveVertical = player.GetPosition().y - transform.position.y;

        //Space away from player for attacking
        if (Mathf.Abs(moveVertical) < SPACING_Y)
        {
            moveVertical = 0.0f;
            inPosY = true;
        }
        else
        {
            inPosY = false;
        }
        if (-moveHorizontal < SPACING_X)
        {
            moveHorizontal = 0.0f;
            inPosX = true;
        }
        else
        {
            inPosX = false;
        }

        if (moveHorizontal != 0)
        {
            moveHorizontal = moveHorizontal / Mathf.Abs(moveHorizontal);
        }
        if (moveVertical != 0)
        {
            moveVertical = moveVertical / Mathf.Abs(moveVertical);
        }

        Vector3 movement = new Vector3(moveHorizontal, moveVertical, 0.0f);

        if (attacks > 0)
        {
            transform.position = transform.position + (movement * speed);
        }
        else
        {
            if (attackTimer > 0)
            {
                attackTimer--;
                transform.position = transform.position + (new Vector3(-1.0f, 1.0f, 0.0f) * speed);
            }
            else
            {
                Destroy(gameObject);
            }
        }


        //Attack code
        if (inPosX && inPosY && !player.StopChasing())
        {
            if (attackTimer > 0 && !turning)
            {
                attackTimer--;
            }

            if (attackTimer == 0)
            {
                StartCoroutine(AttackRoutine());
                attackTimer = Random.Range(40, 80) + 30;
            }

        }
    }

    // Hurt routine attacks immediately equals 0 and run away!
    public void Hurt(DamagePacket packet)
    {
        attacks = 0;
        attackTimer = 250;
        StartCoroutine(StunRoutine());
        StartCoroutine(BlinkRoutine());
        GameObject.FindObjectsOfType<UIController>()[0].UpdateScore(1500L);
    }

    private IEnumerator HeadTurnRoutine()
    {
        if (primedTurns < 1)
        {
            primedTurns = 1;
        }
        // Loop until all the turns have been completed
        turning = true;
        while (primedTurns > 0)
        {
            animator.SetTrigger("Turn");
            for (int i = 0; i < 30; i++)
            {
                yield return new WaitForFixedUpdate();
            }
            primedTurns--;
        }
        turning = false;
    }

    private IEnumerator AttackRoutine()
    {
        animator.SetTrigger("Open");
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        Instantiate(fire, transform.position + new Vector3(0.01f * (facingLeft ? -1 : 1), 0.67f, 0.0f), transform.rotation);
        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        animator.SetTrigger("Close");
        if (Random.Range(0.0f, 1.0f) < 0.5f)
        {
            StartCoroutine(HeadTurnRoutine());
        }
        attacks--;
        if (attacks <= 0)
        {
            gameObject.tag = "Untagged";
            attackTimer = 250;
        }
    }


    private IEnumerator StunRoutine()
    {
        for (int i = 0; i < 5; i++)
        {
            transform.position += new Vector3(-0.05f * (facingLeft ? -1 : 1), 0.0f, 0.0f);
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator BlinkRoutine()
    {
        transform.Find("CherubimHead").GetComponent<SpriteRenderer>().color = new Color(255.0f, 0.0f, 0.0f, 1.0f);
        transform.Find("CherubimWings").GetComponent<SpriteRenderer>().color = new Color(255.0f, 0.0f, 0.0f, 1.0f);
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        transform.Find("CherubimHead").GetComponent<SpriteRenderer>().color = new Color(255.0f, 255.0f, 255.0f, 1.0f);
        transform.Find("CherubimWings").GetComponent<SpriteRenderer>().color = new Color(255.0f, 255.0f, 255.0f, 1.0f);
        gameObject.tag = "Untagged";
    }


    public void SpawnRandom()
    {
        transform.position = new Vector3(transform.position.x, player.GetPosition().y + Random.Range(2.0f, 12.0f), -1.0f);
    }
}
