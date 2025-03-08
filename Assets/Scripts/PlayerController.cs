using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //Public Objects
    public HitboxController hitbox;
    public HurtboxController hurtbox;
    public FjellriverController fjellriver;
    public HaloController halo;

    //Public Variables
    public float speed;
    public float limitYTop;
    public float limitYBottom;

    //Private variables
    // - - - - - - - - - - - 

    //Other objects
    private UIController ui;

    //Components
    private Transform transform;
    private Animator animator;

    //Movement variables
    private float limitXLeft;
    private float limitXRight;
    private bool facingLeft;
    private bool downed;
    private bool jumping;
    private float jumpHeight;

    //Timers/Flags
    private int cooldown;
    private bool punchPushed;
    private int specialCooldown;
    private bool specialPushed;
    private bool specialChangePushed;
    private int stun;
    private bool doStun;
    private bool doAnimation;

    //Character information
    private int health;
    private int special;
    private int specialUnlocked;
    private float comboCounter;
    private List<HitboxController> activeHitboxes;
    private FjellriverController axeInstance;
    private bool axeInstanced;
    private HaloController haloInstance;
    private bool haloWindup;

    private SFXController sfxController;

    // Start is called before the first frame update
    void Start()
    {
        //Get components
        ui = GameObject.FindObjectsOfType<UIController>()[0];
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        transform.position = new Vector3(transform.position.x, limitYTop, -1.0f);
        sfxController = GameObject.FindObjectOfType<SFXController>();

        //Initialize all the flags, timers, etc.
        facingLeft = false;
        downed = false;
        jumping = false;
        jumpHeight = 0.0f;

        special = 3;
        cooldown = 0;
        punchPushed = false;
        comboCounter = 0.0f;
        //Special cooldown is so that specials with projectiles (axes/halos) can't be spammed. Only allowing one on screen by the player at a time
        specialCooldown = 0;
        specialPushed = false;
        specialChangePushed = false;
        specialUnlocked = 3;
        stun = 0;
        doStun = true;
        health = 20;
        activeHitboxes = new List<HitboxController>();
        axeInstance = null;
        axeInstanced = false;
        doAnimation = false;
        haloInstance = null;
        haloWindup = false;

        //Get camera position to limit x movement
        limitXLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        limitXRight = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        // For safety
        ChangeHurtbox(0);

        StartCoroutine(LoadStateRoutine());
    }

    private IEnumerator LoadStateRoutine()
    {
        yield return new WaitForFixedUpdate();
        // Load data from previous level
        ui.LoadGameState();
    }

    void FixedUpdate()
    {
        if (!ui.GameActive())
        {
            animator.SetTrigger("Idle");
            return;
        }

        if (!doAnimation)
        {
            CalculateMovement();

            PerformActions();
        }
    }

    // Update is called once per frame
    void Update()
    {
        limitXLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        limitXRight = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
    }

    private void CalculateMovement()
    {
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
            if (specialCooldown == 85 && axeInstanced)
            {
                axeInstanced = false;
            }
            if (specialCooldown == 54 && haloWindup)
            {
                haloWindup = false;
            }
            // When cooldown is done we can forget about any instanced items
            if (specialCooldown == 0)
            {
                axeInstance = null;
                haloInstance = null;
            }
        }
        if (stun > 0)
        {
            stun--;
            if (doStun)
            {
                movement = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }
        if (comboCounter > 0.0f)
        {
            comboCounter -= 0.017f;
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
        //If currently moving and in downedS state, move to Get Up state!
        if ((movement != new Vector3(0.0f, 0.0f, 0.0f)) && animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerDowned"))
        {
            animator.SetTrigger("Stand");
            cooldown = 14;
            movement = new Vector3(0.0f, 0.0f, 0.0f);
            StartCoroutine(StandingRoutine());
            // TODO: Does this still happen?      Sometimes this gets retriggered while still down. Stop that!
            animator.ResetTrigger("Knockback");
        }

        transform.position = transform.position + (movement * speed);
    }

    private void PerformActions()
    {
        // Press button to switch between specials
        if (Input.GetAxis("Jump") > 0 && specialChangePushed == false)
        {
            special = (special + 1) % (specialUnlocked + 1);
            if (special == 0 && specialUnlocked > 0)
            {
                special = 1;
            }
            specialChangePushed = true;
            ui.UpdateSpecial(special);
            if (specialUnlocked > 1)
            {
                sfxController.PlaySFX2D("General/Text_Beep", 0.3f, 5, 0.0f, true);
            }
        }
        if (Input.GetAxis("Jump") == 0)
        {
            specialChangePushed = false;
        }

        //Attack code
        if (cooldown == 0 && stun == 0 && !downed && !punchPushed && Input.GetAxis("Fire1") > 0)
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

    private void Punch()
    {
        CreateHitbox(new Vector3(1.1f, 0.63f, 0.0f), 1.2f, 0.8f, 180, 1);
        sfxController.PlaySFX2D("General/Punch", 1.0f, 20, 0.15f, false);
    }

    public void CreateHitbox(Vector3 vector, float x, float y, int ttl, int damage)
    {
        HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(vector.x * (facingLeft ? -1 : 1), vector.y, 0.0f), transform.rotation);
        hit.SetX(x);
        hit.SetY(y);
        hit.SetTtl(ttl);
        hit.SetDamage(damage);
        hit.SetParent(transform);

        //Keep track of all active hitboxes so that they can be purged in case Player is hit
        activeHitboxes.Add(hit);

        //This coroutine will remove every hitbox 0.3 seconds after it should be ttl'd
        float time = (ttl * 0.001f) + 0.3f;
        StartCoroutine(CleanHitbox(activeHitboxes, hit, time));
    }

    //Wait 10 seconds, then clean hitbox
    private IEnumerator CleanHitbox(List<HitboxController> list, HitboxController hit, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        list.Remove(hit);
    }

    private void Specials()
    {
        if (special == 1)
        {
            animator.SetTrigger("Axe");
            cooldown = 55;
            FjellriverController axe = Instantiate(fjellriver, transform.position + new Vector3(-0.426f * 6.0f * (facingLeft ? -1.0f : 1.0f), 0.15f * 6.0f, 0.0001f), transform.rotation);
            axe.SetDirection(facingLeft);
            axeInstanced = true;
            axeInstance = axe;
            specialCooldown = 135;
            ui.TriggerSpecialCover(135);
        }
        else if (special == 2)
        {
            animator.SetTrigger("Jump");
            cooldown = 64;
            StartCoroutine(JumpRoutine());
            ui.TriggerSpecialCover(64);
        }
        else if (special == 3)
        {
            animator.SetTrigger("Halo");
            cooldown = 22;
            specialCooldown = 76;
            haloWindup = true;
            ui.TriggerSpecialCover(76);
        }
    }

    public void Hurt(DamagePacket packet)
    {
        if (stun == 0 && ui.GameActive() && !downed)
        {
            int damage = packet.getDamage();
            if (damage >= 0)
            {
                sfxController.PlaySFX2D("General/Hit_HighPitch", 1.0f, 10, 0.15f, false);
            }
            else
            {
                sfxController.PlaySFX2D("General/Heal_modified", 0.5f, 10, 0.05f, false);
            }
            if (doStun)
            {
                //Clear cooldown so user can act immediately out of stun in case they used a move with huge cooldown before being stunned
                cooldown = 0;
                if (damage > 0)
                {
                    facingLeft = packet.getDirection();
                    transform.localScale = new Vector3((facingLeft ? -1.0f : 1.0f) * 6.0f, transform.localScale.y, transform.localScale.z);
                }
                //Above only applies if stun is being applied. Don't want to break stunless mvoes
            }

            //Destroy all active hitboxes
            foreach (HitboxController hitbox in activeHitboxes)
            {
                if (hitbox != null)
                {
                    hitbox.Kill();
                }
            }
            activeHitboxes.Clear();

            //Get rid of Fjellriver Controller
            if (axeInstanced)
            {
                axeInstance.Destroy();
                axeInstanced = false;
            }

            // Clear Halo flag
            if (haloWindup)
            {
                haloWindup = false;
            }

            health -= damage;
            if (health <= 0)
            {
                animator.SetTrigger("Knockback");
                StartCoroutine(KnockbackRoutine(false));
                ui.SaveHighScore();
                health = 0;
                ui.PlayerHealthBar(health);
                sfxController.PlaySFX2D("General/Death", 1.0f, 1, 0.0f, false);
            }
            else if (health > 20)
            {
                StartCoroutine(HealthBlinkRoutine());
                stun = 10;
                health = 20;
            }
            else
            {
                // Allow 0 damage to cause stun too so that you can scare players
                if (damage >= 0)
                {
                    StartCoroutine(BlinkRoutine());
                    comboCounter += damage;
                    if (comboCounter >= 3.0f && doStun)
                    {
                        sfxController.PlaySFX2D("General/Knockback", 1.0f, 20, 0.0f, false);
                        animator.SetTrigger("Knockback");
                        StartCoroutine(KnockbackRoutine(false));
                        cooldown = 60;
                        comboCounter = 0.0f;
                    }
                    else if (doStun)
                    {
                        animator.SetTrigger("Stun");
                        stun = 15;
                        StartCoroutine(StunRoutine());
                    }
                    else;
                    {
                        stun = 15;
                    }
                }
                else
                {
                    StartCoroutine(HealthBlinkRoutine());
                    stun = 10;
                }
                ui.PlayerHealthBar(health);
            }
        }
    }

    private IEnumerator StunRoutine()
    {
        for (int i = 0; i < 5; i++)
        {
            transform.position += new Vector3(-0.05f * (facingLeft ? -1 : 1), 0.0f, 0.0f);
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
    }

    private IEnumerator BlinkRoutine()
    {
        GetComponent<SpriteRenderer>().color = new Color(255.0f, 0.0f, 0.0f, 1.0f);
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        GetComponent<SpriteRenderer>().color = new Color(255.0f, 255.0f, 255.0f, 1.0f);
    }

    private IEnumerator HealthBlinkRoutine()
    {
        GetComponent<SpriteRenderer>().color = new Color(0.5f, 1.0f, 0.5f, 1.0f);
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        GetComponent<SpriteRenderer>().color = new Color(255.0f, 255.0f, 255.0f, 1.0f);
    }

    private IEnumerator KnockbackRoutine(bool manualControl)
    {
        if (!jumping || jumpHeight < 0.000001f)
        {
            downed = true;
            for (int i = 0; i < 49; i++)
            {
                transform.position += new Vector3(0.03f * (facingLeft ? 1 : -1), 0.01f * ((48 - i) - 24), 0.0f);
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
            // Break before landing so other callers can create different length fall animations
            if (manualControl)
            {
                yield break;
            }

            animator.SetTrigger("Downed");
            if (transform.position.y - 0.85f >= limitYBottom)
            {
                transform.position += new Vector3(0.0f, -0.85f, -0.0085f);
            }
            sfxController.PlaySFX2D("General/Fall", 1.0f, 10, 0.0f, false);
            // Yes the cooldown goes beyond this so they can sit and contemplate their failures
        }
        // This should only trigger if player is dead while jumping
        else
        {
            int i = 0;
            while (jumpHeight > 0.0f)
            {
                transform.position += new Vector3(0.03f * (facingLeft ? 1 : -1), 0.01f * ((48 - i) - 24), 0.0f);
                jumpHeight += 0.01f * ((48 - i) - 24);
                // Correct out of boundsness
                if (transform.position.x < limitXLeft)
                {
                    transform.position += new Vector3(limitXLeft - transform.position.x, 0.0f, 0.0f);
                }
                else if (transform.position.x > limitXRight)
                {
                    transform.position += new Vector3(limitXRight - transform.position.x, 0.0f, 0.0f);
                }
                i++;

                yield return new WaitForFixedUpdate();
            }

            animator.SetTrigger("Downed");
            if (transform.position.y - 0.85f >= limitYBottom)
            {
                transform.position += new Vector3(0.0f, -0.85f, -0.0085f);
            }
        }
    }

    private IEnumerator StandingRoutine()
    {
        for (int i = 0; i < 14; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        if (transform.position.y + 0.85f <= limitYTop)
        {
            transform.position += new Vector3(0.0f, 0.85f, 0.0085f);
        }
        downed = false;
    }

    private IEnumerator JumpRoutine()
    {
        jumping = true;
        //Crouched
        for (int i = 0; i < 9; i++)
        {
            if (health <= 0)
            {
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }
        //Jumping
        sfxController.PlaySFX2D("General/Jump_modified", 1.0f, 20, 0.0f, false);
        for (int i = 0; i < 24; i++)
        {
            if (health <= 0)
            {
                yield break;
            }

            transform.position += new Vector3(0.07f * (facingLeft ? -1 : 1), 0.33f * (((float)(24 - i)) / 24), 0.0f);
            jumpHeight += 0.33f * (((float)(24 - i)) / 24);
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
        if (health <= 0)
        {
            yield break;
        }

        //Kick
        sfxController.PlaySFX2D("General/Kick_modified3", 1.0f, 20, 0.0f, false);

        // This needs a buff, and buffing this actually nerfs the player in the Shadow Greyhound fight
        stun = 99999;

        CreateHitbox(new Vector3(0.0f * 6, -0.15742f * 6, 0.0f), 0.08799372f * 6, 0.2870359f * 6, 140, 1);
        for (int i = 0; i < 7; i++)
        {
            if (health <= 0)
            {
                yield break;
            }

            // This is calculated to be the exact height of the jump so we don't need to worry about losing/gaining altitude with each jump
            transform.position += new Vector3(0.18f * (facingLeft ? -1 : 1), -0.589285714285f, 0.0f);
            jumpHeight += -0.589285714285f;
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
        if (health <= 0)
        {
            yield break;
        }
        stun = 0;

        //Landing
        CreateHitbox(new Vector3(0.011f * 6, -0.241f * 6, 0.0f), 0.203804f * 6, 0.135036f * 6, 20, 2);
        for (int i = 0; i < 9; i++)
        {
            if (health <= 0)
            {
                yield break;
            }

            if (i == 1)
            {
                CreateHitbox(new Vector3(0.01281f * 6, -0.2591f * 6, 0.0f), 0.4173282f * 6, 0.09884501f * 6, 20, 1);
                jumping = false;
                jumpHeight = 0.0f;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public void ThrowHalo()
    {
        HaloController thrownHalo = Instantiate(halo, transform.position + new Vector3(1.0f * (facingLeft ? -1 : 1), 0.8f, 0.0f), transform.rotation);
        thrownHalo.SetDirection(facingLeft);
        thrownHalo.SetAsEnemy(false);
        haloInstance = thrownHalo;
    }

    public void ChangeHurtbox(int hurtboxPosition)
    {
        /* Positions
         * Default (0): idle
         * 1: crouch / standing up
         * 2: jumping
         * 3: mid-air tuck
         * 4: mid-air kick
         * 5: land
         * 6: dead
         */
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

    public bool StopChasing()
    {
        return downed;
    }

    public bool IsDead()
    {
        return health <= 0;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    /**
     * Only to be used for the most extreme situations!!!
     */
    public void ForcePosition(Vector3 pos)
    {
        transform.position = pos;
    }

    public int GetHealth()
    {
        return health;
    }

    public void SetHealth(int health)
    {
        this.health = health;
    }

    public int GetSpecial()
    {
        return this.special;
    }

    public void SetSpecial(int special)
    {
        this.special = special;
    }

    public int GetSpecialUnlocked()
    {
        return this.specialUnlocked;
    }

    public void SetSpecialUnlocked(int unlocked)
    {
        this.specialUnlocked = unlocked;
    }

    /**
     * For making the player invincible after defeating a boss so a projectile doesn't wreck their day.
     * Can just reuse stun because they ain't moving afterward anyway
     */
    public void MakeInvincible()
    {
        stun = 9999999;
    }

    /**
     * Nothing gold can stay
     */
    public void MakeVincible()
    {
        stun = 0;
    }

    public void Celebrate()
    {
        animator.SetTrigger("Victory");
    }

    /**
     *  For making special attacks non-cancellable
     */
    public void SetStunnable()
    {
        this.doStun = true;
    }

    public void SetUnstunnable()
    {
        this.doStun = false;
    }

    public void FaceLeft(bool faceLeft)
    {
        this.facingLeft = faceLeft;
        transform.localScale = new Vector3(6.0f * (faceLeft ? -1 : 1), transform.localScale.y, transform.localScale.z);
    }

    public bool IsFacingLeft()
    {
        return this.facingLeft;
    }

    /**
     * For animating the STM cutscenes
     */
    public void KnockbackAnimation(bool zeroHealth)
    {
        float midpoint = (limitXLeft + limitXRight) / 2;
        if (transform.position.x < midpoint - 1.0f)
        {
            facingLeft = true;
            transform.localScale = new Vector3(-6.0f, transform.localScale.y, transform.localScale.z);
        }
        else if (transform.position.x > midpoint + 1.0f)
        {
            facingLeft = false;
            transform.localScale = new Vector3(6.0f, transform.localScale.y, transform.localScale.z);
        }
        animator.SetTrigger("Knockback");
        StartCoroutine(KnockbackRoutine(true));
        if (zeroHealth)
        {
            ui.PlayerHealthBar(0);
        }
    }

    public void LandingAnimation()
    {
        animator.SetTrigger("Downed");
        sfxController.PlaySFX2D("General/Fall", 1.0f, 10, 0.0f, true);
        if (transform.position.y - 0.85f >= limitYBottom)
        {
            transform.position += new Vector3(0.0f, -0.85f, -0.0085f);
        }
    }

    public void KnockbackAnimationRecovery()
    {
        ui.PlayerHealthBar(health);
    }

    public bool IsAxeInstanced()
    {
        return this.axeInstanced;
    }

    public FjellriverController GetAxeInstance()
    {
        return this.axeInstance;
    }

    public bool IsHaloWindup()
    {
        return this.haloWindup;
    }

    public HaloController GetHaloInstance()
    {
        return this.haloInstance;
    }
}
