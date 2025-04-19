using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupremeSaintController : MonoBehaviour
{
    //Public Objects
    public HitboxController hitbox;
    public RingAuraController ring;
    public PunchAuraController punch;

    //Private variables
    private Transform transform;
    private Animator animator;
    private bool facingLeft;
    private int cooldown;
    private int stun;
    private float spacingX;
    private int health;
    private PlayerController player;
    private UIController ui;

    private SFXController sfxController;

    // Start is called before the first frame update
    void Start()
    {
        facingLeft = true;

        ui = GameObject.FindObjectsOfType<UIController>()[0];
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        player = GameObject.FindObjectsOfType<PlayerController>()[0];
        cooldown = 10;
        stun = 0;
        spacingX = 4.0f;
        health = 60;

        sfxController = GameObject.FindObjectOfType<SFXController>();


        StartCoroutine(EntranceRoutine());
    }

    private IEnumerator EntranceRoutine()
    {
        float deltaY = -1.88f - transform.position.y;
        float deltaZ = -1.016f - transform.position.z;
        transform.position = transform.position + new Vector3(0.0f, deltaY, deltaZ);
        yield return new WaitForFixedUpdate();

        ui.PanCamera();

        animator.SetTrigger("Walk");
        //Movement of Boss onto screen through the cutscene
        for (int i = 0; i < 90; i++)
        {
            transform.position = transform.position + new Vector3(-0.025f, 0.0f, 0.0f);
            if (i == 50)
            {
                ui.DisplayDialogue("GodHeadshot", "You have done well to make it this far!|Defeat my champion and I will|deem you worthy of the|final fight!");
                ui.BossEntrance(health, "SUPREME SAINT");
            }
            yield return new WaitForFixedUpdate();
        }
        animator.SetTrigger("Idle");
    }

    void FixedUpdate()
    {
        if (!ui.GameActive() || player.IsDead())
        {
            return;
        }

        //Movement Code
        // Lol there is none. I need a break from complicated fights before the final boss!

        if (cooldown > 0)
        {
            cooldown--;
        }
        // Don't attack while captain greyhound is downed
        if (!player.StopChasing() && cooldown == 0 && stun == 0)
        {
            // If near do Punch
            if (transform.position.x - player.GetPosition().x < spacingX)
            {
                stun = 99;
                animator.SetTrigger("Punch");
                StartCoroutine(PunchRoutine());
            }
            // TODO: If far do Rings
            else
            {
                stun = 99;
                animator.SetTrigger("Raise");
                StartCoroutine(RingRoutine());
            }
        }
    }

    private IEnumerator PunchRoutine()
    {
        sfxController.PlaySFX2D("STM/Charge_Trimmed", 0.8f, 10, 0.0f, false);
        for (int i = 0; i < 136; i++)
        {
            if (health <= 0)
            {
                break;
            }

            // Going to have to go super manual on this animation to make it look chunky enough and not move the sprite out of place
            if (i > 39 && i < 100 && i % 5 == 0)
            {
                transform.position = transform.position + new Vector3(0.015f * (i / 10) * ((i % 10 == 0) ? 1 : -1), 0.0f, 0.0f);
            }
            yield return new WaitForFixedUpdate();
        }

        if (health > 0 && !player.IsDead())
        {
            // begin punch again if player is still too close
            if (transform.position.x - player.GetPosition().x < spacingX * 3 && !player.StopChasing())
            {
                animator.SetTrigger("Punch");
                StartCoroutine(PunchRoutine());
            }
            else
            {
                stun = 0;
                cooldown = Random.Range(35, 55);
            }
        }
    }

    private IEnumerator RingRoutine()
    {
        // Wait for Hand raising animation
        for (int i = 0; i < 34; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        // Keep spawning rings until the player is close enough for a punch, or defeated boss
        while (health > 0 && transform.position.x - player.GetPosition().x >= spacingX && !player.IsDead() && !player.StopChasing())
        {
            RingAuraController instance = Instantiate(ring, transform.position + new Vector3(0.0f, 0.0f, 0.01f), transform.rotation);
            instance.SetDirection(true);
            for (int i = 0; i < ((health > 40) ? 100 : 90); i++)
            {
                if (health <= 0 || transform.position.x - player.GetPosition().x < spacingX || player.IsDead())
                {
                    if (player.IsDead())
                    {
                        animator.SetTrigger("Idle");
                    }
                    break;
                }

                yield return new WaitForFixedUpdate();
            }
        }

        stun = 0;
    }

    public void Punch()
    {
        Instantiate(punch, transform.position + new Vector3(0.0f, 0.0f, 0.01f), transform.rotation);
    }

    public void Hurt(DamagePacket packet)
    {
        sfxController.PlaySFX2D("General/Hit_LowPitch", 1.0f, 15, 0.15f, false);
        cooldown = 0;
        health -= packet.getDamage();
        if (health > 0)
        {
            animator.SetTrigger("Completed");
            StartCoroutine(BlinkRoutine());
        }
        else if (!player.IsDead())
        {
            //Start death sequence
            sfxController.PlaySFX2D("STM/Death_Cherubim_Human_modified", 1.0f, 15, 0.0f, false);
            stun = 99999;
            animator.SetTrigger("Die");
            StartCoroutine(DeathRoutine());
        }
        ui.BossHealthBar(health);
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
        ui.SetPlayerInvincible(true);
        for (int i = 0; i < 51; i++)
        {
            if (i < 50)
            {
                transform.position = transform.position + new Vector3(0.014f, (19 - i) / 200.0f, 0.0f);
            }
            if (i == 50)
            {
                //TODO: Initiate Death dialogue
                animator.SetTrigger("Dim");
                ui.UpdateScore(25000L);
                ui.DisplayDialogue("GodHeadshot", "You have proven yourself child.|You are now ready to|take back your school!|I have faith in your abilities.|Go Strong!");
                ui.PrimeTransition("SJHS");
                ui.BossExit();
            }
            yield return new WaitForFixedUpdate();
        }
        ui.SetPlayerInvincible(false);

        ui.SaveGameState(false, 3);
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
