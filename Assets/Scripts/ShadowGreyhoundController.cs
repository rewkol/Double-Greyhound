using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This class is funny because stuff is getting more complex now so I'm actually using 
 * comments and stuff like I should
 */
public class ShadowGreyhoundController : MonoBehaviour
{

    //Public Objects
    public HitboxController hitbox;
    public HurtboxController hurtbox;
    public FjellriverController fjellriver;
    public HaloController halo;
    public MitchellController mitchell;

    public float speed;
    public float limitYTop;
    public float limitYBottom;
    public Material lerpMaterial;

    private UIController ui;
    private PuppetMasterController puppetMaster;
    private GameObject ownHurtbox;

    private Transform transform;
    private Animator animator;
    private SpriteRenderer renderer;

    private int state;
    private PlayerController player;

    private float limitXLeft;
    private float limitXRight;
    private bool facingLeft;

    private int cooldown;
    private bool punchPushed;
    private int specialCooldown;
    private bool specialPushed;
    private bool specialChangePushed;
    private int stun;
    private bool inPos;
    private bool doAnimation;
    private int health;
    private int special;
    private List<HitboxController> activeHitboxes;
    private FjellriverController axeInstance;
    private bool axeInstanced;

    private bool doorScared;
    private bool battleBegun;

    private float avoidXLeft;
    private float avoidXRight;
    private float avoidYBottom;
    private float avoidYTop;

    private int staleness;
    private bool punchHappy;

    private GameObject musicController;

    // Start is called before the first frame update
    void Start()
    {
        ui = GameObject.FindObjectsOfType<UIController>()[0];
        musicController = GameObject.Find("MusicController");
        puppetMaster = GameObject.FindObjectsOfType<PuppetMasterController>()[0];
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        renderer = GetComponent<SpriteRenderer>();
        ownHurtbox = transform.Find("Hurtbox").gameObject;
        ownHurtbox.SetActive(false);

        // Player copy variables
        player = GameObject.FindObjectsOfType<PlayerController>()[0];
        transform.position = new Vector3(transform.position.x, limitYTop, -1.0001f);
        limitXLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        limitXRight = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        facingLeft = false;
        special = 3;
        cooldown = 0;
        punchPushed = false;
        specialCooldown = 0;
        specialPushed = false;
        specialChangePushed = false;
        stun = 0;
        inPos = false;
        health = 70;
        doAnimation = false;


        // Yeah I know by this point I could really be using ENUMs but who cares
        // States:  0 - Follow
        //          1 - Cutscene Control
        //          2 - Fight
        //          3 - Fight phase 2???? Maybe new object by this point
        state = 0;

        doorScared = false;

        // For defining an area for shadow greyhound to avoid standing in
        avoidXLeft = -99.9f;
        avoidXRight = -99.9f;
        avoidYBottom = -99.9f;
        avoidYTop = -99.9f;

        staleness = 0;
        punchHappy = false;
    }


    void FixedUpdate()
    {
        if (!ui.GameActive())
        {
            if (player.IsDead() && !animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerVictory"))
            {
                state = 999;
                animator.SetTrigger("Victory");
            }

            return;
        }

        if (state == 0)
        {
            CopyPlayerMovement();

            // Switch after crossing the door
            if (transform.position.x >= 50.0f && health > 0)
            {
                state = 1;
                DefaultStance();
                transform.position += new Vector3(-1.0f, 0.0f, 0.0f);
            }
        }

        // State 1 is malleable to do whatever we want with. No internal logic outside certain conditions and cutscenes
        if (state == 1 && !doorScared && player.transform.position.x >= 57.0f)
        {
            doorScared = true;
            StartCoroutine(DoorScareRoutine());
        }

        if (state == 1 && !battleBegun && player.transform.position.x >= 74.3f)
        {
            battleBegun = true;
            DefaultStance();
            StartCoroutine(EntranceRoutine());
        }

        if (state >= 2)
        {
            BattleLogic();
        }

    }

    /**
     * Reset his stance so animations and other logic don't get funky
     */
    private void DefaultStance()
    {
        animator.SetTrigger("Idle");
        facingLeft = false;
        transform.localScale = new Vector3(6.0f, transform.localScale.y, transform.localScale.z);
    }

    private IEnumerator DoorScareRoutine()
    {
        for (int i = 0; i < 40; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        if (battleBegun)
        {
            yield break;
        }
        facingLeft = true;
        transform.localScale = new Vector3(-6.0f, transform.localScale.y, transform.localScale.z);
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        if (battleBegun)
        {
            yield break;
        }
        animator.SetTrigger("Walk");
        for (int i = 0; i < 50; i++)
        {
            if (!battleBegun)
            {
                transform.position += new Vector3(-0.1f, 0.0f, 0.0f);
                yield return new WaitForFixedUpdate();
            }
        }
        if (!battleBegun)
        {
            DefaultStance();
        }
    }

    private IEnumerator EntranceRoutine()
    {
        gameObject.tag = "Boss";
        transform.position = player.GetPosition() + new Vector3(-19.5f, 0.0f, 0.0f);
        if (transform.position.y > limitYTop)
        {
            transform.position += new Vector3(0.0f, limitYTop - transform.position.y, 0.0f);
        }
        yield return new WaitForFixedUpdate();
        
        if (musicController != null)
        {
            musicController.SendMessage("StopCurrentSong");
        }


        ui.DisplayDialogue("GodHeadshot", "I thought I'd try something different|for our final meeting!");
        while(!ui.GameActive())
        {
            yield return new WaitForFixedUpdate();
        }

        ui.PanCameraReverse();

        //Movement of Boss onto screen through the cutscene
        animator.SetTrigger("Walk");
        for (int i = 0; i < 180; i++)
        {
            if (i == 10)
            {
                player.FaceLeft(true);
            }

            transform.position = transform.position + new Vector3(0.025f, 0.0f, 0.0f);
            if (i == 100)
            {
                ui.DisplayDialogue("ShadowHeadshot", "Or I guess this is the first|time you have met me in this form...|I am the one whole stole the trophies!|I am the Greyhound who came before you|and I am the Greyhound who will|end your journey!");
                ui.BossEntrance(health, "SHADOW GREYHOUND");
                if (musicController != null)
                {
                    musicController.SendMessage("StartNextSong");
                }
            }
            yield return new WaitForFixedUpdate();
        }
        animator.SetTrigger("Idle");

        state = 2;
        ownHurtbox.SetActive(true);
        StartCoroutine(SpecialSelectionRoutine());
        cooldown = 1;
    }

    // THE BELOW CODE IS FOR THE ACTUAL BATTLE

    /**
     * Follow Captain Greyhound and use the best special against his current special
     */
    private void BattleLogic()
    {
        // Reset staleness if the player has been hurt (hitstun will reduce the time scale)
        if (Time.timeScale < 1.0f)
        {
            staleness = 0;
        }

        // Need up to date camera bounds
        limitXLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        limitXRight = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;


        // Can use these to figure out position in relation to player for all paths below
        Vector3 playerPos = player.GetPosition();
        float moveHorizontal = playerPos.x - transform.position.x;
        float moveVertical = playerPos.y - transform.position.y;
        float spacingX = 5.0f;
        float varianceX = 0.25f;
        float spacingY = 1.0f;
        float varianceY = 0.15f;

        // Always face the player (unless in the middle of an action)
        if (cooldown == 0)
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
        
        // Depending on special use prioritize different spacing
        if (special == 1)
        {
            spacingX = 7.5f;
            spacingY = 1.0f;
            varianceX = 1.0f;
            varianceY = 0.15f;
        }
        else if (special == 2)
        {
            spacingX = 2.6f;
            spacingY = 0.0f;
            varianceX = 0.15f;
            varianceY = 0.15f;
        }
        else if (special == 3)
        {
            // Want to be at far edge of screen in line vertically with player and beyond axe range away from the player
            float haloDist = facingLeft ? (limitXRight - playerPos.x - 0.5f) : (playerPos.x - limitXLeft - 0.5f);
            
            // Calculate spacingX so that it will be close to boundary but not less than 7.5f from player
            // By using max, if nearest side is too close to player will flip around
            spacingX = Mathf.Max(haloDist, 5.0f);
            spacingY = 0.0f;
            varianceX = 0.5f;
            varianceY = 0.15f;
        }

        // Really don't care about Y axis in these calculations
        float playerDistance = Mathf.Abs(transform.position.x - playerPos.x);

        // If jump selected and close to player and they start to throw halo
        // Or if halo is instanced and a set distance away
        HaloController playerHalo = player.GetHaloInstance();
        if (avoidYBottom < transform.position.y && special == 2 && specialCooldown == 0 && 
            ((playerDistance < 7.5f && player.IsHaloWindup()) ||
            (playerHalo != null && transform.position.y - playerHalo.GetPosition().y < 1.0f && 
                ((playerHalo.GetDirection() && playerHalo.GetPosition().x - transform.position.x > 7.5f) || 
                (!playerHalo.GetDirection() && transform.position.x - playerHalo.GetPosition().x > 7.5f)))))
        {
            // Trigger jump now!
            Specials();
        }
        // Else if Halo instanced and close to bottom or top of stage, turn that side to avoid zone and walk below/above the halo
        else if (avoidXLeft < 0.0f && playerHalo != null && 
                ((playerHalo.GetDirection() && playerHalo.GetPosition().x - transform.position.x > -2.0f) ||
                (!playerHalo.GetDirection() && transform.position.x - playerHalo.GetPosition().x > -2.0f)))
        {
            // Take 0.8f off because that is where it is spawned in relation to player height
            float haloZoneY = playerHalo.GetPosition().y - 0.8f;
            float haloBuffer = 1.3f;

            if (haloZoneY < limitYBottom + haloBuffer)
            {
                avoidXLeft = limitXLeft;
                avoidXRight = limitXRight;
                avoidYBottom = limitYBottom - haloBuffer;
                avoidYTop = limitYBottom + haloBuffer;
            }
            else if (haloZoneY > limitYTop - haloBuffer)
            {
                avoidXLeft = limitXLeft;
                avoidXRight = limitXRight;
                avoidYBottom = limitYTop - haloBuffer;
                avoidYTop = limitYTop + haloBuffer;
            }

            // Can set with low TTL because Halo will still exist so just refresh it each time

            if (avoidXLeft > 0.0f)
            {
                StartCoroutine(ClearAvoidZoneRoutine(10));
            }
        }

        // When player has thrown axe and within axe range, run toward player
        if (avoidXLeft < 0.0f && !player.IsAxeInstanced() && player.GetAxeInstance() != null)
        {
            bool axeFacingLeft = player.GetAxeInstance().GetDirection();
            float axeZoneX = playerPos.x + ((axeFacingLeft ? -1 : 1) * 7.5f);
            // Switch it up so he doesn't act predictably
            if (Random.Range(0, 10) < 3)
            {
                axeFacingLeft = !axeFacingLeft;
            }
            avoidXLeft = axeZoneX - (axeFacingLeft ? 2.2f : 0.75f);
            avoidXRight = axeZoneX + (axeFacingLeft ? 0.75f : 2.2f);
            avoidYBottom = limitYBottom;
            avoidYTop = limitYTop;
            StartCoroutine(ClearAvoidZoneRoutine(60));
        }



        // Move to spacing
        if (cooldown == 0 && !player.StopChasing())
        {
            // Find closest target space around player position
            Vector3[] targets = new Vector3[4];
            float[] distances = new float[4];

            targets[0] = new Vector3(playerPos.x - spacingX, playerPos.y - spacingY, playerPos.z);
            targets[1] = new Vector3(playerPos.x + spacingX, playerPos.y - spacingY, playerPos.z);
            targets[2] = new Vector3(playerPos.x - spacingX, playerPos.y + spacingY, playerPos.z);
            targets[3] = new Vector3(playerPos.x + spacingX, playerPos.y + spacingY, playerPos.z);
            int minDistTarget = -1;

            for (int i = 0; i < 4; i++)
            {
                // If out of bounds ignore
                if (targets[i].x < limitXLeft || targets[i].x > limitXRight || targets[i].y < limitYBottom || targets[i].y > limitYTop)
                {
                    continue;
                }

                // If in avoid range ignore
                if (targets[i].x >= avoidXLeft && targets[i].x <= avoidXRight && targets[i].y >= avoidYBottom && targets[i].y <= avoidYTop)
                {
                    continue;
                }


                float distance = Vector3.Distance(targets[i], transform.position);
                distances[i] = distance;
                if (minDistTarget < 0)
                {
                    minDistTarget = i;
                }
                else if (distance < distances[minDistTarget])
                {
                    minDistTarget = i;
                }
            }

            // If no valid target run to middle
            Vector3 target = new Vector3((limitXLeft + limitXRight) / 2, (limitYBottom + limitYTop) / 2, 0.0f);
            // If need to avoid something default target needs to move
            if (avoidXLeft > 0.0f)
            {
                // Go after player if entire width of axis is avoided
                float targetX = playerPos.x;
                if (avoidXLeft != limitXLeft || avoidXRight != limitXRight)
                {
                    float xMidpoint = (avoidXLeft + avoidXRight) / 2;
                    float xFromRight = limitXRight - xMidpoint;
                    targetX = limitXLeft + xFromRight;
                }

                float targetY = playerPos.y;
                if (avoidYBottom != limitYBottom || avoidYTop != limitYTop)
                {
                    float yMidpoint = (avoidYBottom + avoidYTop) / 2;
                    float yFromTop = limitYTop - yMidpoint;
                    targetY = limitYBottom + yFromTop;
                }

                // Found midpoint of avoid sector and trying to go to opposite area of it in camera bounds
                target = new Vector3(targetX, targetY, 0.0f);
            }
            // Otherwise go toward target
            if (minDistTarget >= 0)
            {
                target = targets[minDistTarget];
            }

            // Sometimes we need to switch it up using harder logic
            if (punchHappy)
            {
                target = new Vector3(playerPos.x, target.y, 0.0f);
            }


            // Rewrite moveHorizontal and moveVertical using the target instead of player
            moveHorizontal = target.x - transform.position.x;
            moveVertical = target.y - transform.position.y;

            if (Mathf.Abs(moveHorizontal) <= varianceX)
            {
                moveHorizontal = 0.0f;
            }
            if (Mathf.Abs(moveVertical) <= varianceY)
            {
                moveVertical = 0.0f;
            }

            // In attack position if in target zone
            inPos = moveHorizontal == moveVertical && moveVertical == 0.0f;


            if (moveHorizontal != 0)
            {
                moveHorizontal = moveHorizontal / Mathf.Abs(moveHorizontal);
            }
            if (moveVertical != 0)
            {
                moveVertical = moveVertical / Mathf.Abs(moveVertical);
            }

            //Bounds checks
            if (transform.position.x + (moveHorizontal * speed) > limitXRight)
            {
                moveHorizontal = (limitXRight - transform.position.x) / speed;
            }
            else if (transform.position.x + (moveHorizontal * speed) < limitXLeft)
            {
                moveHorizontal = (limitXLeft - transform.position.x) / speed;
            }
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

            //If currently moving and not in walking state, move to walking state!
            if (movement != new Vector3(0.0f, 0.0f, 0.0f) && animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerIdle"))
            {
                animator.SetTrigger("Walk");
            }
            //If movement stopped, but not because of stun/knockback/etc, then go to idle pose
            else if (movement == new Vector3(0.0f, 0.0f, 0.0f) && animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerWalk"))
            {
                animator.SetTrigger("Idle");
            }


            transform.position = transform.position + (movement * speed);
        }

        if (player.StopChasing())
        {
            staleness = 0;
        }

        if (player.StopChasing() && !animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerIdle"))
        {
            animator.SetTrigger("Idle");
        }

        // If in position to attack do so
        if (inPos && cooldown == 0 && specialCooldown == 0)
        {
            staleness++;
            if (staleness > Random.Range(7, 13))
            {
                punchHappy = true;
                StartCoroutine(PunchHappyRoutine());
            }
            // Can literally use the player code to trigger :)
            Specials();
        }

        // Punch player if they get too close
        if (cooldown == 0 && Mathf.Abs(playerPos.x - transform.position.x) < 2.45f && !player.StopChasing())
        {
            cooldown = 25;
            animator.SetTrigger("Punch");
        }


        if (cooldown > 0)
        {
            cooldown--;
        }
        if (specialCooldown > 0)
        {
            specialCooldown--;
        }


        if (state == 2 && health < 35) // > 36
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerIdle"))
            {
                animator.SetTrigger("Idle");
            }
            state = 1;
            StartCoroutine(MidwayRoutine());
        }
    }

    /**
     * Periodically check Captain Greyound and choose the special that beats his
     */
    private IEnumerator SpecialSelectionRoutine()
    {
        while (state >= 2)
        {
            // Make it random so player can't game the timing
            int period = Random.Range(10, 40);
            for (int i = 0; i < period; i++)
            {
                if (i == 0)
                {
                    // I could just use 0-2 but mimicking Player for consistency
                    if (special != player.GetSpecial() - 1 && (player.GetSpecial() == 1 && special != 3))
                    {
                        staleness -= Random.Range(3,7);
                        if (staleness < 0)
                        {
                            staleness = 0;
                        }
                    }

                    special = player.GetSpecial() - 1;
                    if (special == 0)
                    {
                        special = 3;
                    }
                }
                yield return new WaitForFixedUpdate();
            }
        }
    }

    private IEnumerator ClearAvoidZoneRoutine(int ttl)
    {
        for(int i = 0; i < ttl; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        avoidXLeft = -99.9f;
        avoidXRight = -99.9f;
        avoidYBottom = -99.9f;
        avoidYTop = -99.9f;
    }

    private IEnumerator BlinkRoutine()
    {
        GetComponent<SpriteRenderer>().color = new Color(0.75f, 0.0f, 0.0f, 1.0f);
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        GetComponent<SpriteRenderer>().color = new Color(0.3922f, 0.3922f, 0.3922f, 1.0f);
    }

    public void Hurt(DamagePacket packet)
    {
        if (stun == 0 && ui.GameActive() && state >= 2)
        {
            int damage = packet.getDamage();

            health -= damage;
            if (health <= 0)
            {
                ui.UpdateScore(15000L);
                StartCoroutine(EndRoutine());
            }
            else
            {
                StartCoroutine(BlinkRoutine());
                ui.BossHealthBar(health);

                if (punchHappy && cooldown == 0)
                {
                    staleness = Random.Range(0, 3);
                }
            }
        }
    }

    private IEnumerator MidwayRoutine()
    {
        ui.StartManualCutscene();
        for (int i = 0; i < 15; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        ui.EndManualCutscene(); 
        ui.DisplayDialogue("ShadowHeadshot", "You certainly can put up a fight|but you are woefully unprepared|to fight me all alone!");
        // Wait for player to read
        while (!ui.GameActive())
        {
            yield return new WaitForFixedUpdate();
        }
        // Bring in the clones
        this.puppetMaster.EnemyEntrance();
        ui.StartManualCutscene();
        for (int i = 0; i < 75; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        ui.EndManualCutscene();
        ui.DisplayDialogue("ShadowHeadshot", "For you see, I came prepared");
        // Wait for player to read
        while (!ui.GameActive())
        {
            yield return new WaitForFixedUpdate();
        }
        // Plus some extra for impact
        ui.StartManualCutscene();
        for (int i = 0; i < 30; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        ui.EndManualCutscene();

        this.puppetMaster.AllyEntrance();

        // Wait for PuppetMaster to do his thing first before coming back down
        while (!ui.GameActive())
        {
            yield return new WaitForFixedUpdate();
        }
        state = 3;
        StartCoroutine(SpecialSelectionRoutine());
    }

    private IEnumerator EndRoutine()
    {
        animator.SetTrigger("Idle");
        state = 0;
        ui.StartManualCutscene();
        for (int i = 0; i < 15; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        if (musicController != null)
        {
            musicController.SendMessage("StopCurrentSong");
        }
        ui.EndManualCutscene();
        ui.DisplayDialogue("ShadowHeadshot", "Enough!|I am tired of this charade!");
        ui.BossExit();
        while (!ui.GameActive())
        {
            yield return new WaitForFixedUpdate();
        }
        ui.StartManualCutscene();
        this.puppetMaster.CueDestruction();

        for (int i = 0; i < 300; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        ui.EndManualCutscene();


        ui.DisplayDialogue("ShadowHeadshot", "Congratulations on getting this far.|Time to meet the real me.");
        while (!ui.GameActive())
        {
            yield return new WaitForFixedUpdate();
        }


        for (int i = 0; i < 15; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        renderer.material = lerpMaterial;
        float amount = 0.1f;
        for (int i = 0; i < 10; i++)
        {
            renderer.material.SetFloat("_LerpAmount", amount);
            amount += 0.1f;
            transform.position += new Vector3(0.0f, 0.02f, 0.0f);
            yield return new WaitForFixedUpdate();
        }

        Instantiate(mitchell, new Vector3(transform.position.x, 12.0f, -1.017f), transform.rotation);

        Destroy(gameObject);
    }

    // THE BELOW CODE IS ALL JUST SO SHADOW GREYHOUND CAN SHADOW THE PLAYER IN THE LOBBY

    private void CopyPlayerMovement()
    {
        this.special = player.GetSpecial();
        CalculateMovement();
        PerformActions();
    }

    private void CalculateMovement()
    {
        limitXLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        limitXRight = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;

        //Movement code
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        //Can't flip around while cooling down
        if (cooldown == 0 && stun == 0)
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


        if (transform.position.x + (moveHorizontal * speed) > limitXRight)
        {
            moveHorizontal = (limitXRight - transform.position.x) / speed;
        }
        else if (transform.position.x + (moveHorizontal * speed) < limitXLeft)
        {
            moveHorizontal = (limitXLeft - transform.position.x) / speed;
        }
        if (transform.position.y + (moveVertical * speed) > limitYTop)
        {
            moveVertical = (limitYTop - transform.position.y) / speed;
        }
        else if (transform.position.y + (moveVertical * speed) < limitYBottom)
        {
            moveVertical = (limitYBottom - transform.position.y) / speed;
        }

        //Z decreases (toward camera) as the player moves down/increases while moving up
        float moveZ = 0.01f * moveVertical;


        Vector3 movement = new Vector3(moveHorizontal, moveVertical, moveZ);

        //Can't move while attacking!
        if (cooldown > 0)
        {
            cooldown--;
            movement = new Vector3(0.0f, 0.0f, 0.0f);
        }
        if (specialCooldown > 0)
        {
            specialCooldown--;
        }

        //If currently moving and not in walking state, move to walking state!
        if (movement != new Vector3(0.0f, 0.0f, 0.0f) && animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerIdle"))
        {
            animator.SetTrigger("Walk");
        }
        //If movement stopped, but not because of stun/knockback/etc, then go to idle pose
        else if (movement == new Vector3(0.0f, 0.0f, 0.0f) && animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerWalk"))
        {
            animator.SetTrigger("Idle");
        }

        transform.position = transform.position + (movement * speed);
    }

    private void PerformActions()
    {
        // Press button to switch between specials
        if (Input.GetAxis("Jump") > 0 && specialChangePushed == false)
        {
            special = (special + 1) % 4;
            if (special == 0 && 4 > 0)
            {
                special = 1;
            }
            specialChangePushed = true;
            ui.UpdateSpecial(special);
        }
        if (Input.GetAxis("Jump") == 0)
        {
            specialChangePushed = false;
        }

        //Attack code
        if (cooldown == 0 && stun == 0 && !punchPushed && Input.GetAxis("Fire1") > 0)
        {
            cooldown = 14;
            animator.SetTrigger("Punch");
            punchPushed = true;
        }
        else if (cooldown == 0 && specialCooldown == 0 && stun == 0 && !specialPushed && Input.GetAxis("Fire2") > 0)
        {
            Specials();
            specialPushed = true;
        }

        if (Input.GetAxis("Fire1") == 0)
        {
            punchPushed = false;
        }
        if (Input.GetAxis("Fire2") == 0)
        {
            specialPushed = false;
        }
    }

    private void Specials()
    {
        if (special == 1)
        {
            animator.SetTrigger("Axe");
            cooldown = 55;
            // Block this out so that the player can't get hurt by shadow's axe in intro
            if (state >= 2)
            {
                FjellriverController axe = Instantiate(fjellriver, transform.position + new Vector3(-0.426f * 6.0f * (facingLeft ? -1.0f : 1.0f), 0.15f * 6.0f, 0.0001f), transform.rotation);
                axe.SetDirection(facingLeft);
                axeInstanced = true;
                axeInstance = axe;
            }
            specialCooldown = 135;
        }
        else if (special == 2)
        {
            animator.SetTrigger("Jump");
            cooldown = 64;
            specialCooldown = 64;
            StartCoroutine(JumpRoutine());
        }
        else if (special == 3)
        {
            animator.SetTrigger("Halo");
            cooldown = 22;
            specialCooldown = 76;
        }

        if (state >= 2 && health > 35)
        {
            specialCooldown += Random.Range(5, 10);
        }
        else if (state >= 2 && health <= 35)
        {
            specialCooldown += Random.Range(0, 2);
        }
    }

    public void CreateHitbox(Vector3 vector, float x, float y, int ttl, int damage)
    {
        if (state < 2)
        {
            return;
        }

        HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(vector.x * (facingLeft ? -1 : 1), vector.y, 0.0f), transform.rotation);
        hit.SetX(x);
        hit.SetY(y);
        hit.SetTtl(ttl);
        hit.SetDamage(damage);
        hit.SetParent(transform);
    }

    private IEnumerator JumpRoutine()
    {
        //Crouched
        for (int i = 0; i < 9; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        //Jumping
        for (int i = 0; i < 24; i++)
        {
            transform.position += new Vector3(0.07f * (facingLeft ? -1 : 1), 0.33f * (((float)(24 - i)) / 24), 0.0f);
            // Correct out of boundsness
            if (transform.position.x < limitXLeft)
            {
                transform.position += new Vector3(limitXLeft - transform.position.x, 0.0f, 0.0f);
            }
            else if (transform.position.x > limitXRight)
            {
                transform.position += new Vector3(limitXRight - transform.position.x, 0.0f, 0.0f);
            }
            yield return new WaitForFixedUpdate();
        }
        // Pause
        for (int i = 0; i < 6; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        //Kick
        CreateHitbox(new Vector3(0.0f * 6, -0.15742f * 6, 0.0f), 0.08799372f * 6, 0.2870359f * 6, 140, 1);
        for (int i = 0; i < 7; i++)
        {
            // This is calculated to be the exact height of the jump so we don't need to worry about losing/gaining altitude with each jump
            transform.position += new Vector3(0.18f * (facingLeft ? -1 : 1), -0.589285714285f, 0.0f);
            // Correct out of boundsness
            if (transform.position.x < limitXLeft)
            {
                transform.position += new Vector3(limitXLeft - transform.position.x, 0.0f, 0.0f);
            }
            else if (transform.position.x > limitXRight)
            {
                transform.position += new Vector3(limitXRight - transform.position.x, 0.0f, 0.0f);
            }
            yield return new WaitForFixedUpdate();
        }
        //Landing
        CreateHitbox(new Vector3(0.011f * 6, -0.241f * 6, 0.0f), 0.203804f * 6, 0.135036f * 6, 20, 2);
        for (int i = 0; i < 9; i++)
        {
            if (i == 1)
            {
                CreateHitbox(new Vector3(0.01281f * 6, -0.2591f * 6, 0.0f), 0.4173282f * 6, 0.09884501f * 6, 20, 1);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public void ThrowHalo()
    {
        if (state < 2)
        {
            return;
        }

        HaloController thrownHalo = Instantiate(halo, transform.position + new Vector3(1.0f * (facingLeft ? -1 : 1), 0.8f, 0.0f), transform.rotation);
        thrownHalo.SetDirection(facingLeft);
        thrownHalo.SetAsEnemy(true);
    }

    private void Punch()
    {
        CreateHitbox(new Vector3(1.1f, 0.63f, 0.0f), 1.2f, 0.8f, 180, 1);
        punchHappy = false;
        staleness = 0; 
    }

    // These two are only here to keep the Animation Controller from spewing errors
    private void SetStunnable()
    {
        // Do nothing
    }

    public void SetUnstunnable()
    {
        // Do nothing
    }

    private void ChangeHurtbox(int hurtboxPosition)
    {
        switch (hurtboxPosition)
        {
            case 1: { hurtbox.UpdateScale(0.2319017f, 0.5263758f); hurtbox.MoveDirect(new Vector3(0.00898f, -0.08404f, 0.0f)); break; }
            case 2: { hurtbox.UpdateScale(0.2435175f, 0.6657706f); hurtbox.MoveDirect(new Vector3(-0.00263f, -0.00272f, 0.0f)); break; }
            case 3: { hurtbox.UpdateScale(0.2144756f, 0.4828102f); hurtbox.MoveDirect(new Vector3(0.01189f, 0.08876f, 0.0f)); break; }
            case 4: { hurtbox.UpdateScale(0.1854354f, 0.6251161f); hurtbox.MoveDirect(new Vector3(-0.01715f, 0.01761f, 0.0f)); break; }
            case 5: { hurtbox.UpdateScale(0.3277372f, 0.3143733f); hurtbox.MoveDirect(new Vector3(0.01044f, -0.14066f, 0.0f)); break; }
            case 6: { hurtbox.UpdateScale(0.0f, 0.0f); break; }
            default: { hurtbox.UpdateScale(0.2348053f, 0.6628667f); hurtbox.MoveDirect(new Vector3(-0.00699f, -0.01579f, 0.0f)); break; }
        }
    }

    private IEnumerator PunchHappyRoutine()
    {
        int length = 0;
        if (this.special == 1)
        {
            length = 40;
        }
        else if (this.special == 2)
        {
            length = 20;
        }
        else if (this.special == 3)
        {
            length = 75;
        }

        for (int i = 0; i < length; i++)
        {
            if (staleness <= 0 || !punchHappy)
            {
                break;
            }

            if (cooldown > 0)
            {
                i--;
            }

            yield return new WaitForFixedUpdate();
        }
        punchHappy = false;
        staleness = 0;
    }
}
