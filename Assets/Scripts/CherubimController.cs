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

    private float limitXLeft;
    private float limitXRight;

    private const float SPACING_X = 7.2f;
    private const float SPACING_Y = 3.7f;

    private SFXController sfxController;
    private bool playSounds;
    private int currentHead;


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
        playSounds = true;
        attackTimer = Random.Range(40, 80);
        attacks = Random.Range(1, 4);
        primedTurns = Random.Range(0, 3);
        currentHead = 0;
        speed = 0.06f;

        limitXLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x - 2.0f;
        limitXRight = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x + 2.0f;

        sfxController = GameObject.FindObjectOfType<SFXController>();

        if (primedTurns > 0)
        {
            StartCoroutine(HeadTurnRoutine());
        }
        StartCoroutine(FlyingAnimationRoutine());
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
                if (transform.position.y > 7.0f)
                {
                    this.playSounds = false;
                }
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
        if (attacks > 0 && transform.position.x > limitXLeft - 2.0f && transform.position.x < limitXRight + 2.0f)
        {
            sfxController.PlaySFX2D("General/Hit_LowPitch", 1.0f, 15, 0.15f, false);
            attacks = 0;
            attackTimer = 250;
            StartCoroutine(StunRoutine());
            StartCoroutine(BlinkRoutine());
            GameObject.FindObjectsOfType<UIController>()[0].UpdateScore(1500L);

            if (currentHead == 1)
            {
                sfxController.PlaySFX2D("STM/Death_Cherubim_Bull", 0.6f, 15, 0.0f, false);
            }
            else if (currentHead == 2)
            {
                sfxController.PlaySFX2D("STM/Death_Cherubim_Eagle", 0.6f, 15, 0.0f, false);
            }
            else if (currentHead == 3)
            {
                sfxController.PlaySFX2D("STM/Death_Cherubim_Lion", 0.6f, 15, 0.0f, false);
            }
            else
            {
                sfxController.PlaySFX2D("STM/Death_Cherubim_Human_modified", 0.6f, 15, 0.0f, false);
            }
        }
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
            currentHead = (currentHead + 1) % 4;
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
        sfxController.PlaySFX2D("STM/Fireball_Spawn", 0.7f, 15, 0.15f, false);
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

    private IEnumerator FlyingAnimationRoutine()
    {
        while(true)
        {
            for(int i = 0; i < 25; i++)
            {
                if (i == 4)
                {
                    transform.position += new Vector3(0.0f, 0.05f, 0.0f);
                }
                if (i == 8)
                {
                    transform.position += new Vector3(0.0f, 0.05f, 0.0f);
                }
                if (i == 12)
                {
                    transform.position += new Vector3(0.0f, -0.025f, 0.0f);
                }
                if (i == 16)
                {
                    transform.position += new Vector3(0.0f, -0.025f, 0.0f);
                    if (playSounds)
                    {
                        sfxController.PlaySFX2D("STM/Flap_Boosted", 1.0f, 15, 0.1f, false);
                    }
                }
                if (i == 20)
                {
                    transform.position += new Vector3(0.0f, -0.025f, 0.0f);
                }
                if (i == 24)
                {
                    transform.position += new Vector3(0.0f, -0.025f, 0.0f);
                }
                yield return new WaitForFixedUpdate();
            }
        }
    }
}
