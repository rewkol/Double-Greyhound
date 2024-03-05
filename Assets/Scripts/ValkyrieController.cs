using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValkyrieController : MonoBehaviour
{
    //Public Objects
    public HitboxController hitbox;
    public EnemyAxeController axe;

    //Public variables
    public float speed;
    public float limitYTop;
    public float limitYBottom;
    public float limitXRight;
    public float limitXLeft;

    //Private variables
    private Transform transform;
    private Animator animator;
    private bool facingLeft;
    private int cooldown;
    private int safety;
    private int stun;
    private float spacingX;
    private int health;
    private bool chaseMode;
    private bool cutscene;

    private UIController ui;
    private PlayerController player;

    private Transform forceLeft;

    // Start is called before the first frame update
    void Start()
    {
        //She only faces left
        facingLeft = true;
        chaseMode = true;

        ui = GameObject.FindObjectsOfType<UIController>()[0];
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        transform.position = new Vector3(transform.position.x, limitYTop, -1.0f);
        player = GameObject.FindObjectsOfType<PlayerController>()[0];
        cooldown = 0;
        safety = 0;
        stun = 0;
        spacingX = 2.0f;
        health = 6;
        cutscene = true;

        forceLeft = GameObject.FindGameObjectsWithTag("Puppet")[0].GetComponent<Transform>();

        StartCoroutine(EntranceRoutine());
    }

    private IEnumerator EntranceRoutine()
    {
        transform.position = transform.position + new Vector3(0.0f, player.GetPosition().y - transform.position.y, (player.GetPosition().y - transform.position.y) * 0.01f);
        yield return new WaitForFixedUpdate();

        ui.PanCamera();

        //Movement of Boss onto screen through the cutscene
        for (int i = 0; i < 100; i++)
        {
            transform.position = transform.position + new Vector3(-0.025f, 0.0f, 0.0f);
            if (i == 50)
            {
                ui.DisplayDialogue("ValkyrieHeadshot", "You will go no further Captain Greyhound!|My shield will stop your advance|and my axe will cut you in two!");
                ui.BossEntrance(health, "VALKYRIE");
            }
            yield return new WaitForFixedUpdate();
        }
        animator.SetTrigger("Idle");
        cutscene = false;
    }

    void FixedUpdate()
    {
        if (player.IsDead())
        {
            animator.SetTrigger("Idle");
            cooldown++;
            if (cooldown > 100 && cooldown < 1000)
            {
                transform.localScale = new Vector3(6.0f, transform.localScale.y, transform.localScale.z);
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("ValkyrieWalk"))
                {
                    animator.SetTrigger("Walk");
                }
                transform.position += new Vector3(0.05f, 0.0f, 0.0f);
            }
            return;
        }

        if (!ui.GameActive())
        {
            return;
        }

        if (player.GetPosition().x >= limitXRight && transform.position.x <= limitXRight)
        {
            transform.localScale = new Vector3(6.0f, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(-6.0f, transform.localScale.y, transform.localScale.z);
        }

        //Movement Code
        float moveHorizontal = -speed;
        if (transform.position.x + moveHorizontal < limitXLeft)
        {
            moveHorizontal = -(transform.position.x - limitXLeft);
        }

        if ((transform.position.x + moveHorizontal) - player.GetPosition().x <= spacingX)
        {
            moveHorizontal = -(transform.position.x - spacingX - player.GetPosition().x);
            if (moveHorizontal > speed)
            {
                moveHorizontal = speed;
            }
        }

        float moveVertical = player.GetPosition().y - transform.position.y;

        if (moveVertical != 0.0f)
        {
            moveVertical = (moveVertical / Mathf.Abs(moveVertical)) * speed;
        }

        if (moveVertical > 0 && transform.position.y + moveVertical > player.GetPosition().y)
        {
            moveVertical = player.GetPosition().y - transform.position.y;
        }
        else if (moveVertical < 0 && transform.position.y + moveVertical < player.GetPosition().y)
        {
            moveVertical = player.GetPosition().y - transform.position.y;
        }

        //Bounds checks
        if (transform.position.y + moveVertical > limitYTop)
        {
            moveVertical = limitYTop - transform.position.y;
        }
        else if (transform.position.y + moveVertical < limitYBottom)
        {
            moveVertical = limitYBottom - transform.position.y;
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
        if (safety > 0)
        {
            safety--;
        }

        //Don't move while captain greyhound is downed
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

        transform.position = transform.position + movement;

        //Fast Attack logic if player tries to go past edge of screen
        if (safety == 0 && transform.position.x > limitXRight && player.GetPosition().x > limitXRight && cooldown == 0 && stun == 0 && !player.StopChasing())
        {
            cooldown = 10;
            animator.SetTrigger("Insta");
            HitboxController hit = Instantiate(hitbox, transform.position - new Vector3(1.3f, 0.00f, 0.0f), transform.rotation);
            hit.SetX(4.0f);
            hit.SetY(10.0f);
            hit.SetTtl(200);
            hit.SetDamage(3);
            if (player.GetPosition().x >= transform.position.x)
            {
                hit.SetParent(forceLeft);
            }
            else
            {
                hit.SetParent(transform);
            }
        }
        // Instant reaction to give Valkyire the chance to block player from sneaking past her
        else if (transform.position.x - player.GetPosition().x < 0.3f && !chaseMode)
        {
            animator.SetTrigger("Completed");
            cooldown = 0;
            stun = 0;
            chaseMode = true;
        }
        // Nuclear option!!!!!!
        else if (player.GetPosition().x >= transform.position.x && cooldown == 0 && !player.StopChasing())
        {
            StartCoroutine(NuclearRoutine());
            chaseMode = true;
        }
    }

    private void FastSwing()
    {
        animator.SetTrigger("FastSwing");
        cooldown = 50;
    }

    public void Hurt(DamagePacket packet)
    {
        if (health <= 0 || cutscene)
        {
            return;
        }
        
        //In chase mode get stunned and knockback into either attack pattern (long/normal swing, or throw on 1 hp remaining)
        if (chaseMode)
        {
            chaseMode = false;
            stun = 40;
            animator.SetTrigger("Stun");
            StartCoroutine(StunRoutine());
        }
        // If in attack animation, interrupt attack and return to idle/chase mode or intiate death if hp remaining goes to 0
        else
        {
            chaseMode = true;
            cooldown = 0;
            //Prevent instant carma if too close to edge
            safety = 90;
            health -= packet.getDamage();
            if (health > 0)
            {
                animator.SetTrigger("Completed");
                StartCoroutine(BlinkRoutine());
            }
            else
            {
                //Start death sequence
                stun = 99999;
                animator.SetTrigger("Die");
                StartCoroutine(DeathRoutine());
            }
            ui.BossHealthBar(health);
        }

        // Valkyrie doesn't really use the damage packet info because she's special...
    }

    //Knock Valkyrie back and choose an attack pattern
    private IEnumerator StunRoutine()
    {
        for(int i = 0; i < 5; i++)
        {
            float knockbackSpeed = 0.25f;
            transform.position = transform.position + new Vector3(knockbackSpeed, 0.0f, 0.0f);
            yield return new WaitForFixedUpdate();
        }
        if (health > 1.0f)
        {
            // Make it impossible to sneak by if too close to edge of screen so player can't sneak by
            if (limitXRight - transform.position.x < 1.0f || Random.Range(0.0f, 1.0f) < 0.4f)
            {
                animator.SetTrigger("ShortSwing");
                StartCoroutine(ShortSwingRoutine());
                cooldown = 26;
            }
            else
            {
                animator.SetTrigger("LongSwing");
                StartCoroutine(LongSwingRoutine());
                cooldown = 60;
            }
        }
        else
        {
            animator.SetTrigger("Throw");
            StartCoroutine(ThrowRoutine());
            cooldown = 50;
        }
    }

    //Pauses before swinging. Meant to attack at this one
    private IEnumerator LongSwingRoutine()
    {
        for(int i = 0; i < 48; i++)
        {   
            yield return new WaitForFixedUpdate();
        }
        chaseMode = true;
    }

    //Swings immediately out of stun. Meant to dodge this one
    private IEnumerator ShortSwingRoutine()
    {
        for (int i = 0; i < 17; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        chaseMode = true;
    }

    private IEnumerator ThrowRoutine()
    {
        for (int i = 0; i < 38; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        chaseMode = true;
    }

    private IEnumerator BlinkRoutine()
    {
        GetComponent<SpriteRenderer>().color = new Color(255.0f, 0.0f, 0.0f, 1.0f);
        for (int i = 0; i < 7; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        GetComponent<SpriteRenderer>().color = new Color(255.0f, 255.0f, 255.0f, 1.0f);
    }

    private IEnumerator DeathRoutine()
    {
        cutscene = true;
        ui.SetPlayerInvincible(true);
        for (int i = 0; i < 51; i++)
        {
            if (i < 50)
            {
                transform.position = transform.position + new Vector3(0.028f, (19 - i) / 200.0f, 0.0f);
            }
            if (i == 50)
            {
				ui.UpdateScore(2500L);
                ui.DisplayDialogue("ValkyrieHeadshot", "You may have bested me...|But my brothers and sisters will keep you|from reaching the Chief!");
                ui.BossExit();
                while(!ui.GameActive())
                {
                    yield return new WaitForFixedUpdate();
                }
            }
            yield return new WaitForFixedUpdate();
        }
        ui.SetPlayerInvincible(false);
        Destroy(gameObject);
    }

    public void SwingAxe(int chase)
    {
        if (chase == 0 || !chaseMode)
        {
            HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(-1.30f, 0.60f, 0.0f), transform.rotation);
            hit.SetX(1.1f);
            hit.SetY(1.7f);
            hit.SetTtl(200);
            hit.SetDamage(3);
            if (player.GetPosition().x >= transform.position.x)
            {
                hit.SetParent(forceLeft);
            }
            else
            {
                hit.SetParent(transform);
            }
        }
    }

    public void ThrowAxe()
    {
        EnemyAxeController thrownAxe = Instantiate(axe, transform.position + new Vector3(-0.95f, 0.7f, 0.0f), transform.rotation);
        thrownAxe.SetDirection(true);
    }

    private IEnumerator NuclearRoutine()
    {
        // The so-called nuclear option because it breaks some precepts of the boss battle
        // Should not have been required but players are crafty
        cooldown = 26;
        animator.ResetTrigger("Idle");
        for (int i = 0; i < 8;  i++)
        {
            if (i == 5)
            {
                animator.SetTrigger("Insta");
            }
            yield return new WaitForFixedUpdate();
        }

        // Hitbox spans whole screen and is so far right that it forces player to get knocked back into the screen
        HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(1.3f, 0.00f, 0.0f), transform.rotation);
        hit.SetX(2.1f);
        hit.SetY(10.0f);
        hit.SetTtl(200);
        hit.SetDamage(3);
        if (player.GetPosition().x >= transform.position.x)
        {
            hit.SetParent(forceLeft);
        }
        else
        {
            hit.SetParent(transform);
        }

        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForFixedUpdate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnRandom()
    {
        //Only to avoid error logs
    }
}
