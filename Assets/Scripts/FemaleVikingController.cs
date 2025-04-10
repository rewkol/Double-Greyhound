using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FemaleVikingController : MonoBehaviour
{
    //Public Objects
    public HitboxController hitbox;
    public EnemyAxeController axe;

    //Public variables
    public float speed;
    public float limitYTop;
    public float limitYBottom;
    public float throwChance;

    //Private variables
    private Transform transform;
    private Animator animator;
    private bool facingLeft;
    private int cooldown;
    private int stun;
    private float spacingX;
    private float spacingY;
    private bool inPosX;
    private bool inPosY;
    private int health;
    private PlayerController player;
    private bool nextAttackSwing;

    private float limitXLeft;
    private float limitXRight;

    //Constants
    private float SPACE_SWING_X = 2.0f;
    private float SPACE_THROW_X = 3.7f;

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
        stun = 0;
        spacingX = 2.0f;
        spacingY = 1.0f;
        health = 4;
        inPosX = false;
        inPosY = false;

        nextAttackSwing = true;
        if (Random.Range(0.0f, 1.0f) < throwChance)
        {
            nextAttackSwing = false;
            spacingX = SPACE_THROW_X;
        }
        else
        {
            nextAttackSwing = true;
            spacingX = SPACE_SWING_X;
        }

        limitXLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x - 2.0f;
        limitXRight = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x + 2.0f;

        sfxController = GameObject.FindObjectOfType<SFXController>();
    }

    void FixedUpdate()
    {
        if (player.IsDead() && health > 0)
        {
            animator.SetTrigger("Walk");
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("FemaleVikingWalk"))
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
        if (Mathf.Abs(moveHorizontal) < spacingX)
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
        }
        if (stun > 0)
        {
            stun--;
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
        if (inPosX && inPosY && cooldown == 0 && stun == 0 && !player.StopChasing())
        {
            if (nextAttackSwing)
            {
                Swing();
            }
            else
            {
                Throw();
            }

            if (Random.Range(0.0f, 1.0f) < throwChance)
            {
                nextAttackSwing = false;
                spacingX = SPACE_THROW_X;
            }
            else
            {
                nextAttackSwing = true;
                spacingX = SPACE_SWING_X;
            }

        }
    }

    void Swing()
    {
        animator.SetTrigger("Swing");
        cooldown = 70;
        StartCoroutine(SwingRoutine());
    }

    private IEnumerator SwingRoutine()
    {
        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        if (stun == 0)
        {
            HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(1.30f * (facingLeft ? -1 : 1), 0.60f, 0.0f), transform.rotation);
            hit.SetX(1.1f);
            hit.SetY(1.7f);
            hit.SetTtl(200);
            hit.SetDamage(2);
            hit.SetParent(transform);
            sfxController.PlaySFX2D("HVHS/Axe_Woosh_Small", 0.7f, 10, 0.2f, false);
        }
    }

    void Throw()
    {
        animator.SetTrigger("Throw");
        cooldown = 105;
    }

    public void ThrowAxe()
    {
        EnemyAxeController thrownAxe = Instantiate(axe, transform.position + new Vector3(0.95f * (facingLeft ? -1 : 1), 0.7f, 0.0f), transform.rotation);
        thrownAxe.SetDirection(facingLeft);
        sfxController.PlaySFX2D("HVHS/Axe_Woosh_Small", 0.7f, 10, 0.2f, false);
    }

    public void Hurt(DamagePacket packet)
    {
        if (stun == 0 && transform.position.x > limitXLeft && transform.position.x < limitXRight)
        {
            sfxController.PlaySFX2D("General/Hit_LowPitch", 1.0f, 15, 0.15f, false);
            facingLeft = packet.getDirection();
            transform.localScale = new Vector3((facingLeft ? -1.0f : 1.0f) * 6.0f, transform.localScale.y, transform.localScale.z);
            stun = 20;
            //Don't stick around too long
            cooldown = 0;
            health -= packet.getDamage();
            if (health <= 0)
            {
                stun = 9999;
                animator.SetTrigger("Knockback"); 
                GameObject.FindObjectsOfType<UIController>()[0].UpdateScore(250L);
                StartCoroutine(DeathRoutine());
                sfxController.PlaySFX2D("HVHS/Death_VikingF", 1.0f, 20, 0.1f, false);
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
        for (int i = 0; i < 5; i++)
        {
            transform.position = transform.position + new Vector3((facingLeft ? 1 : -1) * 0.09f, 0, 0.0f);
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
                transform.position = transform.position + new Vector3((facingLeft ? 1 : -1) * 0.028f, (19 - i) / 200.0f, 0.0f);
                //Don't need to bounds check anymore because she's about to poof anyway
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
        transform.position = new Vector3(transform.position.x, startY, ((startY - limitYTop) * 0.01f) - 1.0f);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
