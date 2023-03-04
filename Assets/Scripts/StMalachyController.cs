using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StMalachyController : MonoBehaviour
{
    // All of the graphics layers needed for the ascension scene he triggers
    public GameObject schoolLayer;
    public GameObject uptownLayer;
    public GameObject skylineLayer;
    public GameObject heavenLayer;
    public GameObject transitionLayer;
    public LightningController lightning;

    private const float Y_OFFSET = 3.2f;

    private PlayerController player;
    private Animator animator;
    private UIController ui;
    private CameraController camera;
    private bool triggeredFight;
    private bool triggeredAscension;
    private bool triggeredBlessing;
    private bool triggeredBattle;
    private bool ascensionOver;
    private int health;
    private Vector3 target;
    private int counter;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindObjectsOfType<PlayerController>()[0];
        animator = GetComponent<Animator>();
        ui = GameObject.FindObjectsOfType<UIController>()[0];
        camera = GameObject.FindObjectsOfType<CameraController>()[0];

        // His state machine is wild and would probably be served better by a true state machine
        triggeredFight = false;
        triggeredAscension = false;
        triggeredBlessing = false;
        triggeredBattle = false;
        ascensionOver = false;
        // He is strongest in the game. Secret Super Boss
        health = 1000;
        target = player.GetPosition();
        counter = Random.Range(40, 70);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 playerPos = player.GetPosition();
        // Wait until player gets close, then spring the entrance routine that other bosses do on Spawn
        if (!triggeredBattle && !triggeredFight && !triggeredBlessing && !triggeredAscension && playerPos.x >= 35.30007f)
        {
            player.ForcePosition(new Vector3(35.30007f, playerPos.y, playerPos.z));
            camera.ForcePosition(35.30007f);
            StartCoroutine(EntranceRoutine());
        }

        if (!triggeredBattle && triggeredFight && !triggeredBlessing && !triggeredAscension && ui.GameActive())
        {
            triggeredBlessing = true;
            StartCoroutine(BlessingRoutine());
        }

        if (!triggeredBattle && triggeredFight && triggeredBlessing && triggeredAscension && ui.GameActive())
        {
            // Set blessing false
            triggeredBlessing = false;
            camera.ForcePosition(38.9f);
            StartCoroutine(AscensionRoutine());
        }

        // Battle code
        if (triggeredBattle && ui.GameActive())
        {
            // Count down if not animating
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("MalachyIdle"))
            {
                counter--;
            }

            // When hits 0 choose an animation
            if (counter == 0)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    animator.SetTrigger("BlessShort");
                }
                else
                {
                    animator.SetTrigger("BlessLong");
                }
                counter = Random.Range(20, 50);
            }
        }
    }


    private IEnumerator EntranceRoutine()
    {
        yield return new WaitForFixedUpdate();
        // Need that pause so that Camera is in fixed position every time
        gameObject.tag = "Boss";
        ui.PanCamera();
        //Need to trigger here so that ui mode is set first
        triggeredFight = true;

        //Movement of Boss onto screen through the cutscene
        for (int i = 0; i < 100; i++)
        {
            if (i == 50)
            {
                ui.DisplayDialogue("StMalachyHeadshot", "Welcome child!|I have been expecting your arrival.|You may have proven your strength|but not yet the strength of your spirit!|I have a trial for you to overcome|Give me a few seconds to prepare|and have faith that I can help you.");
                ui.BossEntrance(health, "ST.MALACHY");
            }
            // Lol this isn't really needed because he isn't moving, but it's a real better safe than sorry moment to leave it as it is, you know?
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator BlessingRoutine()
    {
        for (int i = 0; i < 320; i++)
        {
            if (i == 50)
            {
                animator.SetTrigger("BlessLong");
            }
            yield return new WaitForFixedUpdate();
        }

        // If health is not at max then he was hit and battle already triggered to start
        if (health == 1000)
        {
            ui.DisplayDialogue("StMalachyHeadshot", "Thank you for waiting.|Everything is prepared for your trial.|Steel your mind for what is to come|and I ask once more to trust me.");
            ui.BossExit();
            triggeredAscension = true;
        }
    }

    private IEnumerator AscensionRoutine()
    {
        ui.StartManualCutscene();
        for (int i = 0; i < 30; i++)
        {
            // Wait for lightning
            yield return new WaitForFixedUpdate();
        }

        Instantiate(lightning, player.GetPosition() + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.1f, 0.1f) + Y_OFFSET, 0.0f), transform.rotation);
        player.KnockbackAnimation();

        bool hellEvent = Random.Range(0.0f, 1.0f) < 0.05f;

        for (int i = 0; i < 100; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        // Hell Diversion
        if (hellEvent)
        { 
            for (int i = 0; i < 1100; i++)
            {
                // Accidentally fall toward Hell
                if (i < 25)
                {
                    schoolLayer.transform.position += new Vector3(0.0f, ((i + 1) / 25.0f) * 0.08f, 0.0f);
                }
                else if (25 <= i && i < 125)
                {
                    schoolLayer.transform.position += new Vector3(0.0f, 0.08f, 0.0f);
                }
                else if (125 <= i && i < 150)
                {
                    schoolLayer.transform.position += new Vector3(0.0f, ((149 - i) / 25.0f) * 0.08f, 0.0f);
                }
                // Check Hell out for a second
                else if (150 <= i && i < 219)
                {
                    ;// TODO: Maybe play sound effect here later
                }
                // Dialogue event
                else if (i == 220)
                {
                    ui.EndManualCutscene();
                    ui.DisplayDialogue("StMalachyHeadshot", "You really don't want to go this way...");
                    while (!ui.GameActive())
                    {
                        yield return new WaitForFixedUpdate();
                    }
                    ui.StartManualCutscene();
                }
                // Ascend out of school slowly
                else if (220 <= i && i < 245)
                {
                    schoolLayer.transform.position += new Vector3(0.0f, ((i - 219) / 25.0f) * -0.08f, 0.0f);
                }
                else if (245 <= i && i < 555)
                {
                    schoolLayer.transform.position += new Vector3(0.0f, -0.08f, 0.0f);
                    uptownLayer.transform.position += new Vector3(0.0f, -0.06f, 0.0f);
                    skylineLayer.transform.position += new Vector3(0.0f, -0.033f, 0.0f);
                }
                // Slow down to look over city
                else if (555 <= i && i < 580)
                {
                    float modifier = ((579 - i) / 25.0f);
                    schoolLayer.transform.position += new Vector3(0.0f, modifier * -0.08f, 0.0f);
                    uptownLayer.transform.position += new Vector3(0.0f, modifier * -0.06f, 0.0f);
                    skylineLayer.transform.position += new Vector3(0.0f, modifier * -0.033f, 0.0f);
                }
                else if (580 <= i && i < 750)
                {
                    ;// TODO: Could put sound effects here too
                }
                // Speed back up toward Heaven
                else if (750 <= i && i < 900)
                {
                    float modifier = ((i - 749) / 50.0f);
                    schoolLayer.transform.position += new Vector3(0.0f, modifier * -0.12f, 0.0f);
                    uptownLayer.transform.position += new Vector3(0.0f, modifier * -0.09f, 0.0f);
                    skylineLayer.transform.position += new Vector3(0.0f, modifier * -0.07f, 0.0f);
                }
                // Transition cloud covers screen / move Heaven into place
                else if (i >= 900)
                {
                    float modifier = 1.0f;
                    if (i > 950)
                    {
                        modifier = (1100 - i) / 150.0f;
                    }
                    transitionLayer.transform.position += new Vector3(0.0f, modifier * -0.28f, 0.0f);
                    if (i == 965)
                    {
                        heavenLayer.transform.position += new Vector3(0.0f, -66.3f, 0.0f);
                    }
                }
                yield return new WaitForFixedUpdate();
            }
        }
        //Normal Ascension
        else
        {
            for (int i = 0; i < 820; i++)
            {
                // Ascend out of school slowly
                if (i < 25)
                {
                    schoolLayer.transform.position += new Vector3(0.0f, (i / 25.0f) * -0.06f, 0.0f);
                }
                else if (25 <= i && i < 275)
                {
                    float catchUp = i < 125 ? 2.8f : 1;
                    schoolLayer.transform.position += new Vector3(0.0f, -0.06f, 0.0f);
                    uptownLayer.transform.position += new Vector3(0.0f, -0.0455f * catchUp, 0.0f);
                    skylineLayer.transform.position += new Vector3(0.0f, -0.027f * catchUp, 0.0f);
                }
                // Slow down to look over city
                else if (275 <= i && i < 300)
                {
                    float modifier = ((300 - i) / 25.0f);
                    schoolLayer.transform.position += new Vector3(0.0f, modifier * -0.06f, 0.0f);
                    uptownLayer.transform.position += new Vector3(0.0f, modifier * -0.0455f, 0.0f);
                    skylineLayer.transform.position += new Vector3(0.0f, modifier * -0.027f, 0.0f);
                }
                else if (300 <= i && i < 470)
                {
                    ;// TODO: Could put sound effects here too
                }
                // Speed back up toward Heaven
                else if (470 <= i && i < 620)
                {
                    float modifier = ((i - 469) / 50.0f);
                    schoolLayer.transform.position += new Vector3(0.0f, modifier * -0.12f, 0.0f);
                    uptownLayer.transform.position += new Vector3(0.0f, modifier * -0.09f, 0.0f);
                    skylineLayer.transform.position += new Vector3(0.0f, modifier * -0.065f, 0.0f);
                }
                // Transition cloud covers screen / move Heaven into place
                else if (i >= 620)
                {
                    float modifier = 1.0f;
                    if (i > 670)
                    {
                        modifier = (820- i) / 150.0f;
                    }
                    transitionLayer.transform.position += new Vector3(0.0f, modifier * -0.28f, 0.0f);
                    if (i == 685)
                    {
                        heavenLayer.transform.position += new Vector3(0.0f, -66.3f, 0.0f);
                    }
                }
                yield return new WaitForFixedUpdate();
            }
        }


        for (int i = 0; i < 40; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        ui.EndManualCutscene();
        ui.DisplayDialogue("StMalachyHeadshot", "A powerful spirit awaits you!|Let it test your will|and you will have proven youself.|The Heavenly Host will aid you.|Good luck child!");
        ui.BossExit();
        while (!ui.GameActive())
        {
            yield return new WaitForFixedUpdate();
        }
        gameObject.tag = "Untagged";
        player.KnockbackAnimationRecovery();
        ascensionOver = true;
    }

    public void Hurt(DamagePacket packet)
    {
        if (triggeredBlessing && !triggeredAscension)
        {
            triggeredBattle = true;
            triggeredBlessing = false;
            counter = Random.Range(40, 70);
            ui.DisplayDialogue("StMalachyHeadshot", "I thought I had made myself clear!|I did not wish to fight you|but it seems words do not sway you.|Perhaps this will help you understand!");
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("MalachyIdle"))
            {
                animator.SetTrigger("Idle");
            }
        }
        if (triggeredBattle)
        {
            health -= packet.getDamage();
            if (health > 0)
            {
                StartCoroutine(BlinkRoutine());
            }
            else
            {
                animator.SetTrigger("Idle");
                StartCoroutine(DeathRoutine());
            }
            ui.BossHealthBar(health);
        }
        if (ascensionOver && camera.GetPosition().x < 55.0f)
        {
            StartCoroutine(BlinkRoutine());
            if (health > 999)
            {
                ui.DisplayDialogue("StMalachyHeadshot", "Please do not hit me.");
            }
            else
            {
                ui.DisplayDialogue("StMalachyHeadshot", "Stop hitting me.");
            }
            // Do after so checks for full health first
            health -= packet.getDamage();
        }
    }

    private IEnumerator DeathRoutine()
    {
        ui.DisplayDialogue("StMalachyHeadshot", "Okay, you have proven your point.|Congratulations for being so strong|but this trial you still must face");
        ui.UpdateScore(999999999L);
        ui.BossExit();
        while (!ui.GameActive())
        {
            yield return new WaitForFixedUpdate();
        }
        triggeredBattle = false;
        triggeredAscension = true;
        triggeredBlessing = false;
        camera.ForcePosition(38.9f);
        StartCoroutine(AscensionRoutine());
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

    /**
     * set target as player position with some random offsets
     */
    public void PrepareTarget()
    {
        target = player.GetPosition() + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.1f, 0.1f) + Y_OFFSET, 0.0f);
    }

    /**
     * Strike target now, hopefully a few seconds later
     */
    public void StrikeTarget()
    {
        if (triggeredBattle && health > 0)
        {
            Instantiate(lightning, target, transform.rotation);
        }
    }
}
