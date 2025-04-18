using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperSaintController : MonoBehaviour
{
    //Public Objects
    public HitboxController hitbox;
    public HaloController halo;

    //Public variables
    public float speed;
    public float limitYTop;
    public float limitYBottom;

    //Private variables
    private Transform transform;
    private Animator animator;
    private bool facingLeft;
    private int cooldown;
    private int attackCycle;
    private int stun;
    private float spacingX;
    private float spacingY;
    private bool inPosX;
    private bool inPosY;
    private int health;
    private PlayerController player;
    private float limitXLeft;
    private float limitXRight;
    private bool onScreen;

    private SFXController sfxController;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        transform.position = new Vector3(transform.position.x, limitYTop, -1.0f);
        player = GameObject.FindObjectsOfType<PlayerController>()[0];
        facingLeft = false;
        cooldown = 0;
        // Extra cooldown on top of cooldown
        attackCycle = 0;
        stun = 0;
        spacingX = 12.0f;
        spacingY = 0.4f;
        health = 4;
        inPosX = false;
        inPosY = false;
        // Tracks that saint has at least been on screen once
        onScreen = false;

        //Get camera position to limit x movement
        limitXLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        limitXRight = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;

        sfxController = GameObject.FindObjectOfType<SFXController>();
    }

    void FixedUpdate()
    {
        if (player.IsDead())
        {
            animator.SetTrigger("Walk");
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("SaintWalk"))
            {
                transform.localScale = new Vector3(6.0f, transform.localScale.y, transform.localScale.z);
                cooldown++;
                if (cooldown < 1000)
                {
                    transform.position += new Vector3(speed, 0.0f, 0.0f);
                    //Don't despawn because despawning moves the camera around
                }
            }
            return;
        }
        //Movement code
        float moveHorizontal = player.GetPosition().x - transform.position.x;
        float moveVertical = player.GetPosition().y - transform.position.y;

        //Can't flip around while cooling down
        if (cooldown <= 1 && stun <= 1)
        {
            if (moveHorizontal > 0)
            {
                facingLeft = false;
                transform.localScale = new Vector3(6.0f, transform.localScale.y, transform.localScale.z);
            }
            else if (moveHorizontal < 0)
            {
                facingLeft = true;
                transform.localScale = new Vector3(-6.0f, transform.localScale.y, transform.localScale.z);
            }
        }

        onScreen = limitXLeft < transform.position.x && limitXRight > transform.position.x;

        //Space away from player for attacking
        if (Mathf.Abs(moveVertical) < spacingY)
        {
            moveVertical = 0.0f;
            inPosY = true;
        }
        else
        {
            inPosY = false;
        }
        // If too close start to walk away
        if (Mathf.Abs(moveHorizontal) <  (2 * spacingX) / 5.0f)
        {
            moveHorizontal = moveHorizontal * -1;
            inPosX = false;
        }
        else if (Mathf.Abs(moveHorizontal) < spacingX && onScreen)
        {
            moveHorizontal = 0.0f;
            inPosX = true;
        }
        else if (Mathf.Abs(moveHorizontal) < spacingX / 2.0f && !onScreen)
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

        //Bounds checks
        if (transform.position.y + (moveVertical * speed) > limitYTop)
        {
            moveVertical = (limitYTop - transform.position.y) / speed;
        }
        else if (transform.position.y + (moveVertical * speed) < limitYBottom)
        {
            moveVertical = (limitYBottom - transform.position.y) / speed;
        }

        float moveZ = 0.01f * moveVertical;

        Vector3 movement = new Vector3(moveHorizontal, moveVertical, moveZ);

        //Can't move while attacking!
        if (cooldown > 0)
        {
            cooldown--;
            movement = new Vector3(0.0f, 0.0f, 0.0f);
            if (attackCycle > 0)
            {
                attackCycle--;
            }
        }
        if (stun > 0)
        {
            stun--;
            movement = new Vector3(0.0f, 0.0f, 0.0f);
        }

        // Stop movement during their knockback animation
        if (player.StopChasing())
        {
            movement = new Vector3(0.0f, 0.0f, 0.0f);
        }

        if (movement != new Vector3(0.0f, 0.0f, 0.0f))
        {
            animator.SetTrigger("Walk");
        }
        else
        {
            animator.SetTrigger("Idle");
        }

        transform.position = transform.position + (movement * speed);

        //Attack code
        if (inPosX && inPosY && cooldown == 0 && stun == 0 && attackCycle == 0 && !player.StopChasing() && onScreen)
        {
            animator.SetTrigger("Throw");
            cooldown = 110;
            attackCycle = Random.Range(20, 50);
        }
    }

    public void ThrowHalo()
    {
        HaloController thrownHalo = Instantiate(halo, transform.position + new Vector3(0.8f * (facingLeft ? -1 : 1), 0.7f, 0.0f), transform.rotation);
        thrownHalo.SetDirection(facingLeft);
        thrownHalo.SetAsEnemy(true);
    }

    public void Hurt(DamagePacket packet)
    {
        if (stun == 0 && transform.position.x > limitXLeft - 2.0f && transform.position.x < limitXRight + 2.0f)
        {
            sfxController.PlaySFX2D("General/Hit_LowPitch", 1.0f, 15, 0.15f, false);
            facingLeft = packet.getDirection();
            transform.localScale = new Vector3((facingLeft ? -1.0f : 1.0f) * 6.0f, transform.localScale.y, transform.localScale.z);
            stun = 20;
            //Don't stick around too long
            cooldown = 0;
            attackCycle = 0;
            health -= packet.getDamage();
            if (health <= 0 && !player.IsDead())
            {
                stun = 9999;
                animator.SetTrigger("Dead");
                GameObject.FindObjectsOfType<UIController>()[0].UpdateScore(900L);
                StartCoroutine(DeathRoutine());
                sfxController.PlaySFX2D("STM/Death_Saint", 1.0f, 20, 0.1f, false);
            }
            else
            {
                animator.SetTrigger("Stun");
                StartCoroutine(BlinkRoutine());
                StartCoroutine(StunRoutine());
            }
        }
    }

    private IEnumerator BlinkRoutine()
    {
        GetComponent<SpriteRenderer>().color = new Color(255.0f, 0.0f, 0.0f, 1.0f);
        for (int i = 0; i < 7; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        for (int i = 0; i < 13; i++)
        {
            float ratio = i / 13.0f;
            GetComponent<SpriteRenderer>().color = new Color(1.0f, ratio, ratio, 1.0f);
            yield return new WaitForFixedUpdate();
        }
        GetComponent<SpriteRenderer>().color = new Color(255.0f, 255.0f, 255.0f, 1.0f);
    }

    private IEnumerator StunRoutine()
    {
        for (int i = 0; i < 7; i++)
        {
            transform.position = transform.position + new Vector3((facingLeft ? 1 : -1) * 0.055f, 0, 0.0f);
            yield return new WaitForFixedUpdate();
        }
    }


    private IEnumerator DeathRoutine()
    {
        gameObject.tag = "Untagged";
        for (int i = 0; i < 201; i++)
        {
            if (i < 50)
            {
                transform.position = transform.position + new Vector3((facingLeft ? 1 : -1) * 0.028f, (17 - i) / 220.0f, 0.0f);
            }
            if (i == 200)
            {
                Destroy(gameObject);
            }
            yield return new WaitForFixedUpdate();
        }
    }


    public void SpawnRandom()
    {
        float startY = Random.Range(limitYBottom, limitYTop);
        transform.position = new Vector3(transform.position.x, startY, ((startY - limitYTop + 0.01f) * 0.01f) - 1.0f);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
