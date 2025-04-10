using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public ParticleSystem fireBreath;
    public HitboxController hitbox;

    private UIController ui;

    private Animator animator;
    private Transform transform;
    private Transform shoulders;
    private Transform chest;
    private Transform belly;
    private BossHandController leftHand;
    private BossHandController rightHand;
    private Transform leftHandT;
    private Transform rightHandT;
    private Transform leftElbow;
    private Transform rightElbow;

    private GameObject[] bodyParts;

    private PlayerController player;

    private int state;
    private int stun;
    private int health;
    private int timer;
    private bool turned;
    private bool justTurned;
    private float midpoint;
    private int handSlams;
    private float turnChanceModifier;

    private GameObject musicController;
    private SFXController sfxController;

    // Start is called before the first frame update
    void Start()
    {
        ui = GameObject.FindObjectsOfType<UIController>()[0];
        musicController = GameObject.Find("MusicController");
        sfxController = GameObject.FindObjectOfType<SFXController>();
        animator = GetComponent<Animator>();
        transform = GetComponent<Transform>();

        player = GameObject.FindObjectsOfType<PlayerController>()[0];

        shoulders = transform.Find("BossShoulders");
        chest = transform.Find("BossChest");
        belly = transform.Find("BossBelly");
        leftHandT = transform.Find("BossHand - Left");
        rightHandT = transform.Find("BossHand - Right");
        leftElbow = transform.Find("BossArmElbowLeft");
        rightElbow = transform.Find("BossArmElbowRight");
        leftHand = leftHandT.gameObject.GetComponent<BossHandController>();
        rightHand = rightHandT.gameObject.GetComponent<BossHandController>();

        bodyParts = GameObject.FindGameObjectsWithTag("Boss");

        state = 0;
        stun = 0;
        health = 750;
        timer = 100;
        turned = false;
        justTurned = false;
        handSlams = 0;
        turnChanceModifier = 0.0f;


        midpoint = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;


        StartCoroutine(EntranceRoutine());
    }

    private IEnumerator EntranceRoutine()
    {
        stun = 9999;
        bool debugFlag = false;
        transform.Find("Hitbox1").gameObject.SetActive(false);
        transform.Find("Hitbox2").gameObject.SetActive(false);
        for (int i = 0; i < 878; i++)
        {
            // Leave time for hands to rise up before the head is visible

            // Raise head up and lower other parts into hunched positions
            if (i > 75)
            {
                transform.position += new Vector3(0.0f, 0.011f, 0.0f);
                shoulders.position += new Vector3(0.0f, -0.001123f, 0.0f);
                chest.position += new Vector3(0.0f, -0.00322f, 0.0f);
                belly.position += new Vector3(0.0f, -0.005965f, 0.0f);
            }
            if (i == 75)
            {
                ui.BossEntrance(health, "THE FINAL BOSS");
                stun = 0;
            }
            if (i == 175)
            {
                transform.Find("Hitbox1").gameObject.SetActive(true);
            }
            if (i == 325)
            {
                transform.Find("Hitbox2").gameObject.SetActive(true);
            }
            yield return new WaitForFixedUpdate();
        }

        animator.SetTrigger("Fire");
        sfxController.PlaySFX2D("SJHS/Flame_Breath_Inhale_Boosted", 1.0f, 15, 0.05f, false);

        for (int i = 0; i < 450; i++)
        {
            if (i == 50)
            {
                sfxController.PlaySFX2D("SJHS/Roar", 1.0f, 15, 0.05f, false);
                sfxController.PlaySFX2D("SJHS/Flame_Breath", 0.6f, 15, 0.05f, false);
            }
            yield return new WaitForFixedUpdate();
        }

        state = 1;
        StartCoroutine(BreatheRoutine());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (state == 0 || !ui.GameActive())
        {
            return;
        }

        int bias = 1;
        if (player.GetPosition().x < midpoint - 4.0f)
        {
            animator.SetTrigger("Left");
            bias = 0;
        }
        else if (player.GetPosition().x > midpoint + 4.0f)
        {
            animator.SetTrigger("Right");
            bias = 2;
        }
        else
        {
            animator.SetTrigger("Idle");
        }

        timer--;
        if (timer <= 0)
        {
            int correction = (int) (((750 - health) / 750.0f) * 5);
            timer = Random.Range(25 - correction, 50 - (correction * 4));
            // Trigger action
            float chance = Random.Range(0.0f, 1.0f);
            bool attacked = false;

            if (turned)
            {
                // Turn chance decreases over time so more claps the less health he has
                float turnChance = 0.4f - (((750 - health) / 750.0f) * 0.15f);
                if (chance < turnChance && !justTurned)
                {
                    if (leftHand.CanTurn() && rightHand.CanTurn())
                    {
                        rightHand.Turn();
                        leftHand.Turn();
                        turned = false;
                        justTurned = true;
                    }
                }
                else if (leftHand.CanTurn() && rightHand.CanTurn())
                {
                    rightHand.PrimeAttack();
                    leftHand.PrimeAttack();
                    // If both hands weren't ready to attack must try again next frame continuously until both can act as one
                    if (!rightHand.AttackPrimed() || !leftHand.AttackPrimed())
                    {
                        timer = 0;
                        rightHand.RescindAttack();
                        leftHand.RescindAttack();
                    }
                    else
                    {
                        justTurned = false;
                    }
                }
            }
            // In centre
            else if (bias == 1 && !turned)
            {
                // 50/50 shot he will fire breath out of clap, 40% otherwise
                if (chance < 0.4f || (justTurned && chance > 0.5f))
                {
                    if (leftHand.CanTurn() && rightHand.CanTurn())
                    {
                        rightHand.FireAttack();
                        leftHand.FireAttack();
                        StartCoroutine(CrouchRoutine());
                        justTurned = false;
                        handSlams = 0;
                    }
                }
                else
                {
                    if (!justTurned && leftHand.CanTurn() && rightHand.CanTurn())
                    {
                        rightHand.Turn();
                        leftHand.Turn();
                        turned = true;
                        justTurned = true;
                        handSlams = 0;
                    }
                }
            }
            else
            {
                if (chance < 0.2f)
                {
                    if (leftHand.CanTurn() && rightHand.CanTurn())
                    {
                        rightHand.FireAttack();
                        leftHand.FireAttack();
                        StartCoroutine(CrouchRoutine());
                        attacked = true;
                        justTurned = false;
                        handSlams = 0;
                        turnChanceModifier += 0.05f;
                    }
                }
                // Needed to add the turnChanceModifier because he was just hitting this supposed 30% chance like 5 times in a row which should be < 0.1% but it kept happening
                else if (chance > (0.7f + turnChanceModifier) && !justTurned)
                {
                    if (leftHand.CanTurn() && rightHand.CanTurn())
                    {
                        rightHand.Turn();
                        leftHand.Turn();
                        turned = true;
                        attacked = true;
                        justTurned = true;
                        handSlams = 0;
                        turnChanceModifier += 0.05f;
                    }
                }

                // Only hand slam if the others didn't go through
                if (!attacked)
                {
                    float handed = Random.Range(0.0f, 1.0f);
                    // In opposite terms, the chance to not attack increases as number of continuous hand slams increases
                    // Basically to make it so when cooldown is so quick that there is a higher chance to break out of endless pattern where
                    // one hand is slamming and the other primes before it recovers so both are not available to turn/fire breath
                    float otherHandChance = 0.5f + ((handSlams/3) * 0.1f);
                    if (handed > 0.8f && leftHand.CanTurn() && rightHand.CanTurn())
                    {
                        rightHand.PrimeAttack();
                        leftHand.PrimeAttack();
                    }
                    else if (bias == 2 || (handed < 0.2f && bias == 0))
                    {
                        rightHand.PrimeAttack();
                        // If attmepted attack failed, try the other with random chance
                        if (!rightHand.AttackPrimed() && handed > otherHandChance)
                        {
                            leftHand.PrimeAttack();
                        }
                        else if (rightHand.AttackPrimed())
                        {
                            handSlams++;
                        }
                    }
                    else if (bias == 0 || (handed < 0.2f && bias == 2))
                    {
                        leftHand.PrimeAttack();
                        if (!leftHand.AttackPrimed() && handed > otherHandChance)
                        {
                            rightHand.PrimeAttack();
                        }
                        else if (leftHand.AttackPrimed())
                        {
                            handSlams++;
                        }
                    }

                    if (rightHand.AttackPrimed() || leftHand.AttackPrimed())
                    {
                        justTurned = false;
                        turnChanceModifier = 0.0f;
                    }
                }
            }



        }

        stun--;
        if (stun <= 0)
        {
            stun = 0;
        }
    }

    private IEnumerator CrouchRoutine()
    {
        int prevState = state;
        state = 0;
        for (int i = 0; i < 50; i++)
        {
            if (health <= 0)
            {
                yield break;
            }
            transform.position += new Vector3(0.0f, -0.066f, 0.0f);
            shoulders.position += new Vector3(0.0f, 0.015f, 0.0f);
            chest.position += new Vector3(0.0f, 0.027f, 0.0f);
            belly.position += new Vector3(0.0f, 0.042f, 0.0f);

            // Correct arm positions
            leftHandT.position += new Vector3(0.0f, 0.066f, 0.0f);
            leftElbow.position += new Vector3(0.0f, 0.066f, 0.0f);
            rightHandT.position += new Vector3(0.0f, 0.066f, 0.0f);
            rightElbow.position += new Vector3(0.0f, 0.066f, 0.0f);
            yield return new WaitForFixedUpdate();
        }
        if (health <= 0)
        {
            yield break;
        }
        animator.SetTrigger("Fire");
        sfxController.PlaySFX2D("SJHS/Flame_Breath_Inhale_Boosted", 1.0f, 15, 0.05f, false);
        for (int i = 0; i < 500; i++)
        {
            if (health <= 0)
            {
                yield break;
            }
            if (i == 50)
            {
                sfxController.PlaySFX2D("SJHS/Roar", 1.0f, 15, 0.05f, false);
                sfxController.PlaySFX2D("SJHS/Flame_Breath", 0.6f, 15, 0.05f, false);
            }
            yield return new WaitForFixedUpdate();
        }

        for (int i = 0; i < 150; i++)
        {
            if (health <= 0)
            {
                yield break;
            }
            transform.position += new Vector3(0.0f, 0.022f, 0.0f);
            shoulders.position += new Vector3(0.0f, -0.005f, 0.0f);
            chest.position += new Vector3(0.0f, -0.009f, 0.0f);
            belly.position += new Vector3(0.0f, -0.014f, 0.0f);

            // Correct arm positions
            leftHandT.position += new Vector3(0.0f, -0.022f, 0.0f);
            leftElbow.position += new Vector3(0.0f, -0.022f, 0.0f);
            rightHandT.position += new Vector3(0.0f, -0.022f, 0.0f);
            rightElbow.position += new Vector3(0.0f, -0.022f, 0.0f);
            yield return new WaitForFixedUpdate();
        }
        state = prevState;
    }

    private IEnumerator BreatheRoutine()
    {
        int cycle = 0;
        while (health > 0)
        {
            while (state > 0)
            {
                int direction = 1;
                if (cycle < 40)
                {
                    direction = -1;
                }
                else if (cycle >= 80 && cycle < 120)
                {
                    direction = 1;
                }
                else
                {
                    direction = 0;
                }

                shoulders.position += new Vector3(0.0f, 0.002f * direction, 0.0f);
                if (cycle % 2 == 0)
                {
                    chest.position += new Vector3(0.0f, 0.002f * direction, 0.0f);
                }
                if (cycle % 4 == 0)
                {
                    belly.position += new Vector3(0.0f, 0.002f * direction, 0.0f);
                }
                cycle++;
                if (cycle > 160)
                {
                    cycle = 0;
                }
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public void BreatheFire()
    {
        StartCoroutine(BurnRoutine());
    }

    private IEnumerator BurnRoutine()
    {
        fireBreath.Play();
        int cycle = -20;
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("BossHeadFire"))
        {
            if (cycle < 50)
            {
                cycle++;
            }

            int j = cycle;
            if (j < 0)
            {
                j = 0;
            }
            float modifier = j / 50.0f;

            HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(-0.00985f, -3.41914f, 0.0f), transform.rotation);
            hit.SetX(1.3639708f * 6 * modifier);
            hit.SetY(0.6603646f * 6 * modifier);
            hit.SetTtl(50);
            hit.SetDamage(3);
            hit.SetParent(transform);
            yield return new WaitForFixedUpdate();
        }
        if (health <= 0)
        {
            fireBreath.Stop();
            yield break;
        }
        for (int i = 0; i < 75; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        fireBreath.Simulate(0.0f, true, true, true);
        yield break;
    }


    public void Hurt(DamagePacket packet)
    {
        if (ui.GameActive() && stun == 0)
        {
            sfxController.PlaySFX2D("General/Hit_LowPitch", 1.0f, 15, 0.15f, false);
            int damage = packet.getDamage();
            int prevHealth = health;
            health -= damage;
            if (health <= 0 && !player.IsDead())
            {
                //Start death sequence
                ui.UpdateScore(100000L);
                stun = 99999999;
                StartCoroutine(DeathRoutine());
            }
            else
            {
                StartCoroutine(BlinkRoutine());
                ui.BossHealthBar(health);

                // Reduce cooldown on hands as health decreases
                if (prevHealth > 600 && health <= 600)
                {
                    leftHand.ReduceCooldown();
                    rightHand.ReduceCooldown();
                }
                else if (prevHealth > 450 && health <= 450)
                {
                    leftHand.ReduceCooldown();
                    rightHand.ReduceCooldown();
                }
                else if (prevHealth > 300 && health <= 300)
                {
                    leftHand.ReduceCooldown();
                    rightHand.ReduceCooldown();
                }
                else if (prevHealth > 150 && health <= 150)
                {
                    leftHand.ReduceCooldown();
                    rightHand.ReduceCooldown();
                }
                else if (prevHealth > 100 && health <= 100)
                {
                    leftHand.ReduceCooldown();
                    rightHand.ReduceCooldown();
                }
                else if (prevHealth > 50 && health <= 50)
                {
                    leftHand.ReduceCooldown();
                    rightHand.ReduceCooldown();
                }
                else if (prevHealth > 25 && health <= 25)
                {
                    leftHand.ReduceCooldown();
                    rightHand.ReduceCooldown();
                }
            }
        }
    }

    private IEnumerator BlinkRoutine()
    {
        foreach (GameObject part in bodyParts)
        {
            part.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }


        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForFixedUpdate();
        }


        foreach (GameObject part in bodyParts)
        {
            part.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
    }

    // Wow this is it huh
    private IEnumerator DeathRoutine()
    {
        if (musicController != null)
        {
            musicController.SendMessage("StopCurrentSong");
        }

        // Kill the sounds in case a long sound effect is playing over the death
        sfxController.CancelAllSounds();
        sfxController.PlaySFX2D("SJHS/Death_Monster_modified", 1.0f, 1, 0.0f, true);

        state = 0;
        animator.SetTrigger("Pain");
        ui.BossExit();
        leftHand.Die();
        rightHand.Die();
        for (int i = 0; i < 1000; i++)
        {
            // Wait for hands to get into a good position. And let player savour victory!
            if (i > 50)
            {
                float randomShake = Random.Range(-0.01f, 0.01f); 
                transform.position += new Vector3(randomShake, -0.015f, 0.0f);
            }
            yield return new WaitForFixedUpdate();
        }

        ui.PrimeTransition("Credits");
        ui.SaveGameState(true, 3);
    }
}
