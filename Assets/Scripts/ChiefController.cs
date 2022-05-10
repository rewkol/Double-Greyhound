using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChiefController : MonoBehaviour
{
    //Public Objects
    public HitboxController hitbox;
    public HurtboxController hurtbox;
    public MaleVikingController enemySpawn;

    //Private variables
    private Transform transform;
    private Animator animator;
    private int cooldown;
    private int stun;
    private int health;
    private int steps;
    private float actionChance;
    private float attackChance;
    private int spawnTimer;
    private bool longDefeat;
    private PlayerController player;
    private UIController ui;

    // Start is called before the first frame update
    void Start()
    {

        ui = GameObject.FindObjectsOfType<UIController>()[0];
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        transform.position = new Vector3(transform.position.x, 0.2616275f, -1.014296f);
        player = GameObject.FindObjectsOfType<PlayerController>()[0];
        //Don't want him acting on spawn
        cooldown = 10;
        steps = 0;
        stun = 0;
        health = 80;
        //Chance that every frame Chief will take an action other than idling
        actionChance = 0.005f;
        //Chance that when taking an action Chief will attack
        attackChance = 0.05f;
        //Timer to prevent spam spawning enemies
        spawnTimer = 0;
        //Determines whether to play full death sequence or shortened death sequence
        longDefeat = true;

        StartCoroutine(EntranceRoutine());
    }

    private IEnumerator EntranceRoutine()
    {
        yield return new WaitForFixedUpdate();
        ui.PanCamera();

        //Movement of Boss onto screen through the cutscene
        for (int i = 0; i < 100; i++)
        {
            if (i == 50)
            {
                ui.DisplayDialogue("ChiefHeadshot", "So you finally made it.|Your precious trophies aren't here.|We would never be so|dishonourable as to steal your|hard-earned treasures.|You offend me!|Now come, prove your strength!|Show me why you deserve those trophies!");
                ui.BossEntrance(health, "CHIEF");
            }
            yield return new WaitForFixedUpdate();
        }
    }

    void FixedUpdate()
    {
        if (!ui.GameActive())
        {
            return;
        }

        //Don't move while captain greyhound is downed
        if (!player.StopChasing())
        {
            float chance = Random.Range(0.0f, 1.0f);

            float dist = (transform.position.x - 3.5f )- player.transform.position.x;

            if (cooldown == 0 && stun == 0 && !player.StopChasing())
            {
                //If player is far in front of Chief just sort of dick around
                if (actionChance >= 1.0f)
                {
                    actionChance = 0.01f;
                    chance = Random.Range(0.0f, 1.0f);
                    //If in attacking range, can attack
                    if (chance < attackChance && dist < 5.0f)
                    {
                        StartCoroutine(AttackRoutine());
                        cooldown = 65;
                        attackChance = 0.25f;
                    }
                    else
                    {
                        attackChance += 0.2f;

                        //Generally trend toward the player unless at max distance
                        if (Random.Range(0.0f, 1.0f) < 0.6f && steps < 7)
                        {
                            animator.SetTrigger("SlowF");
                            cooldown = 56;
                        }
                        else if (steps > 0)
                        {
                            animator.SetTrigger("SlowB");
                            cooldown = 56;
                        }
                    }
                }
                //If player is behind Chief, book it back even off-screen if you need to!
                else if (dist < 0)
                {
                    animator.SetTrigger("FastB");
                    cooldown = 12;
                }
                //If too far ahead should have a routine to have him charge forward and swing!
                else if (steps < 3 && dist > 8.0f)
                {
                    StartCoroutine(ChargeRoutine());
                    cooldown = 113;
                }
                else
                {
                    actionChance += 0.01f;
                }

                if ( health < 40 && steps > 3)
                {
                    spawnTimer++;
                    if (spawnTimer == 300)
                    {
                        spawnTimer = 0;
                        Instantiate(enemySpawn, transform.position + new Vector3(-16.0f , 0.0f, 0.0f), transform.rotation);
                    }
                }
            }
        }

        if (cooldown > 0)
        {
            cooldown--;
        }
        if (stun > 0)
        {
            stun--;
        }
    }

    private void StepForward()
    {
        transform.position += new Vector3(-1.38f, 0.0f, 0.0f);
        steps++;
    }

    private void StepBackward()
    {
        transform.position += new Vector3(1.38f, 0.0f, 0.0f);
        steps--;
    }

    private IEnumerator ChargeRoutine()
    {
        //Charge 4 steps forward
        for (int i = 0; i < 4; i++)
        {
            animator.SetTrigger("FastF");
            for (int j = 0; j < 12; j++)
            {
                yield return new WaitForFixedUpdate();
            }
        }
        //Then attack!
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        animator.SetTrigger("Swing");
        for (int i = 0; i < 65; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        if (steps > 4)
        {
            int struggles = Random.Range(3, 6);
            for ( int j = 0; j < struggles; j++)
            {
                animator.SetTrigger("Struggle");
                cooldown = 25;
                for (int i = 0; i < 25; i++)
                {
                    yield return new WaitForFixedUpdate();
                }
            }
        }
        animator.SetTrigger("Pull");
        cooldown = 60;
        for (int i = 0; i < 60; i++)
        {
            yield return new WaitForFixedUpdate();
        }
    }

    private void LeftLegHitbox()
    {
        //Why are these all *6.0f?
        //Because I'm copy-pasting the numbers right out of the editor, but when I use the Chief as a parent his scale is making these numbers 6x lesser than they should be!
        HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(-0.5026f * 6.0f, -0.41531f * 6.0f, 0.0f), transform.rotation);
        hit.SetX(0.2380721f * 6.0f);
        hit.SetY(0.5612745f * 6.0f);
        hit.SetTtl(40);
        hit.SetDamage(1);
        hit.SetParent(transform);
    }

    private void RightLegHitbox()
    {
        //HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(0.2074f * 6.0f, -0.41531f * 6.0f, 0.0f), transform.rotation);
        HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(-0.4926f * 6.0f, -0.41531f * 6.0f, 0.0f), transform.rotation);
        hit.SetX(0.2192811f * 6.0f);
        hit.SetY(0.5612745f * 6.0f);
        hit.SetTtl(40);
        hit.SetDamage(1);
        hit.SetParent(transform);
    }

    private void BackLegHitbox()
    {
        HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(-0.23933f * 6.0f, -0.44784f * 6.0f, 0.0f), transform.rotation);
        hit.SetX(0.2380724f * 6.0f);
        hit.SetY(0.4823571f * 6.0f);
        hit.SetTtl(40);
        hit.SetDamage(1);
        hit.SetParent(transform);
    }

    private void AxeHitbox()
    {
        HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(-1.08382f * 6.0f, -0.49538f * 6.0f, 0.0f), transform.rotation);
        hit.SetX(0.5875853f * 6.0f);
        hit.SetY(0.3357855f * 6.0f);
        hit.SetTtl(50);
        hit.SetDamage(5);
        hit.SetParent(transform);
    }

    private void MoveHurtbox(int pos)
    {
        /* POSITIONS
         * 1 - Sideways standing / Midstep 
         * 2 - Endstep
         * 3 - Swung
         * 4 -
         * 5 - 
         * 6 - Default
         */
        switch (pos)
        {
            case 1: { hurtbox.MoveDirect(new Vector3(-0.45321f, -0.043f, 1.0f)); hurtbox.UpdateScale(0.2980177f, 1.310197f); break; }
            case 2: { hurtbox.MoveDirect(new Vector3(-0.58851f, -0.043f, 1.0f)); hurtbox.UpdateScale(0.4708951f, 1.310197f); break; }
            case 3: { hurtbox.MoveDirect(new Vector3(-0.51898f, -0.30794f, 1.0f)); hurtbox.UpdateScale(0.670065f, 0.8103533f); break; }
            default: { hurtbox.MoveDirect(new Vector3(-0.34022f, -0.03907f, 1.0f)); hurtbox.UpdateScale(0.4483503f, 1.310197f); break; }
        }

    }

    public void Hurt(DamagePacket packet)
    {
        //In chase mode get stunned and knockback into either attack pattern (long/normal swing, or throw on 1 hp remaining)
        if (stun == 0)
        {
            cooldown = 0;
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
    }
    //Swings immediately out of stun. Meant to dodge this one
    private IEnumerator ShortSwingRoutine()
    {
        for (int i = 0; i < 17; i++)
        {
            if (i == 12)
            {
                HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(-1.30f, 0.60f, 0.0f), transform.rotation);
                hit.SetX(1.1f);
                hit.SetY(1.7f);
                hit.SetTtl(200);
                hit.SetDamage(3);
                hit.SetParent(transform);
            }
            yield return new WaitForFixedUpdate();
        }
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
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].SendMessage("Hurt", new DamagePacket(99999, transform.position.x < enemies[i].transform.position.x));
        }

        animator.SetTrigger("Defeated");
        for (int i = 0; i < 61; i++)
        {
            if (i == 60)
            {
                //TODO: Initiate Death dialogue
                ui.UpdateScore(10000L);
                ui.DisplayDialogue("ChiefHeadshot", "You're good.|I never expected you to best me.|Perhaps you have the strength|you need for the coming battle.");
                while (!ui.GameActive())
                {
                    yield return new WaitForFixedUpdate();
                }
                //Stand back up
                for (int j = 0; j < 15; j++)
                {
                    yield return new WaitForFixedUpdate();
                }
                animator.SetTrigger("Idle");
                for (int j = 0; j < 20; j++)
                {
                    yield return new WaitForFixedUpdate();
                }
                ui.DisplayDialogue("ChiefHeadshot", "And if you don't, I will give you mine!|I deem you worthy to wield my axe|Fjellriver|It has the power to|rend mountains in two|but to you I will grant just enough to|overcome any obstacle you might face.|Now climb aboard, I will see you|to your next challenge.|Good luck my friend!");
                ui.BossExit();
                ui.PrimeTransition("SHS");
            }
            yield return new WaitForFixedUpdate();
        }

        ui.SaveGameState(false, 1);
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
