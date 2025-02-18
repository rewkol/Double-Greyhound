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

    private GameObject musicController;

    // Start is called before the first frame update
    void Start()
    {
        ui = GameObject.FindObjectsOfType<UIController>()[0];
        musicController = GameObject.Find("MusicController");
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


        midpoint = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;


        StartCoroutine(EntranceRoutine());
    }

    private IEnumerator EntranceRoutine()
    {
        stun = 9999;
        bool debugFlag = false;
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
            yield return new WaitForFixedUpdate();
        }

        animator.SetTrigger("Fire");

        for (int i = 0; i < 450; i++)
        {
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
                if (chance < 0.25f && !justTurned)
                {
                    if (leftHand.CanTurn() && rightHand.CanTurn())
                    {
                        rightHand.Turn();
                        leftHand.Turn();
                        turned = false;
                        justTurned = true;
                    }
                }
                else
                {
                    rightHand.PrimeAttack();
                    leftHand.PrimeAttack();
                    justTurned = false;
                }
            }
            // In centre
            else if (bias == 1 && !turned)
            {
                if (chance < 0.4f || justTurned)
                {
                    if (leftHand.CanTurn() && rightHand.CanTurn())
                    {
                        rightHand.FireAttack();
                        leftHand.FireAttack();
                        StartCoroutine(CrouchRoutine());
                        justTurned = false;
                    }
                }
                else
                {
                    if (leftHand.CanTurn() && rightHand.CanTurn())
                    {
                        rightHand.Turn();
                        leftHand.Turn();
                        turned = true;
                        justTurned = true;
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
                    }
                }
                else if (chance > 0.7f && !justTurned)
                {
                    if (leftHand.CanTurn() && rightHand.CanTurn())
                    {
                        rightHand.Turn();
                        leftHand.Turn();
                        turned = true;
                        attacked = true;
                        justTurned = true;
                    }
                }

                // Only hand slam if the others didn't go through
                if (!attacked)
                {
                    float handed = Random.Range(0.0f, 1.0f);
                    if (handed > 0.8f && leftHand.CanTurn() && rightHand.CanTurn())
                    {
                        rightHand.PrimeAttack();
                        leftHand.PrimeAttack();
                    }
                    else if (bias == 2 || (handed < 0.2f && bias == 0))
                    {
                        rightHand.PrimeAttack();
                        // If attmepted attack failed, try the other with random chance
                        if (!rightHand.AttackPrimed() && handed > 0.5f)
                        {
                            leftHand.PrimeAttack();
                        }
                    }
                    else if (bias == 0 || (handed < 0.2f && bias == 2))
                    {
                        leftHand.PrimeAttack();
                        if (!leftHand.AttackPrimed() && handed > 0.5f)
                        {
                            rightHand.PrimeAttack();
                        }
                    }
                    justTurned = false;
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
        for (int i = 0; i < 500; i++)
        {
            if (health <= 0)
            {
                yield break;
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
            int damage = packet.getDamage();
            health -= damage;
            if (health <= 0)
            {
                ui.UpdateScore(100000L);
                stun = 99999999;
                StartCoroutine(DeathRoutine());
            }
            else
            {
                StartCoroutine(BlinkRoutine());
                ui.BossHealthBar(health);
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
