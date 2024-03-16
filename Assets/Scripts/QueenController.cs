using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenController : MonoBehaviour
{
    //Public variables
    public HitboxController hitbox;
    public HurtboxController hurtbox;
    public FlyingBarbController barb;
    public DressCrownController crown;
    public DressCrownController dress;
    public float limitYTop;
    public float limitYBottom;

    //Private variables
    private Transform transform;
    private Animator animator;
    private PlayerController player;
    private UIController ui;
    private float limitXLeft;
    private float limitXMid;
    private float limitXRight;
    private GameObject drone1;
    private GameObject drone2;
    private int health;
    private int stun;
    private int cooldown;
    private bool flying;
    private bool pointing;
    private bool facingLeft;
    private float targetX;
    private float targetY;
    private bool jumping;
    private bool kicking;
    private int punchCounter;
    private int jumpDirection;
    private int pointTimer;

    private static float PLAYER_HEIGHT_ADJUSTMENT = 0.05f; 

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        ui = GameObject.FindObjectsOfType<UIController>()[0];
        player = GameObject.FindObjectsOfType<PlayerController>()[0];
        drone1 = transform.Find("Drone1").gameObject;
        drone2 = transform.Find("Drone2").gameObject;
        health = 120;
        stun = 0;
        cooldown = 50;
        flying = true;
        pointing = false;
        facingLeft = true;
        targetX = transform.position.x;
        targetY = transform.position.y;
        jumping = false;
        kicking = false;
        punchCounter = 0;
        jumpDirection = 1;
        pointTimer = 0;


        float midY = (limitYTop + limitYBottom) / 2;
        float midZ = -1.0f + (0.01f * (midY - limitYTop));
        transform.position = new Vector3(transform.position.x, transform.position.y, midZ);

        limitXLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        limitXMid = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        limitXRight = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;

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
                ui.DisplayDialogue("QueenHeadshot", "We would give you the \'Royal Welcome\'|but you have hurt many of our daughters.|We cannot tolerate such senselessness.|And to think if you had asked nicely|we would have gladly helped you|find your precious trophies...");
                ui.BossEntrance(health, "QUEEN SEABEE");
            }
            yield return new WaitForFixedUpdate();
        }

        // Reset limits
        limitXLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        limitXMid = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        limitXRight = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        ChooseTarget();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!ui.GameActive())
        {
            return;
        }

        if (!pointing)
        {
            HandleDirection();
        }

        // Flying code
        if (flying)
        {
            // Keeping speed localized here. Speed being carried vs alone should be different and variable depending on action
            float speed = 0.04f;

            float moveHorizontal = targetX - transform.position.x;
            float moveVertical = targetY - transform.position.y;

            if (moveHorizontal != 0)
            {
                moveHorizontal = moveHorizontal / Mathf.Abs(moveHorizontal);
            }
            if (moveVertical != 0)
            {
                moveVertical = moveVertical / Mathf.Abs(moveVertical);
            }

            //Reduce if in range of target
            if (Mathf.Abs(targetX - transform.position.x) < speed)
            {
                moveHorizontal = (targetX - transform.position.x) / speed;
            }
            if (Mathf.Abs(targetY - transform.position.y) < speed)
            {
                moveVertical = (targetY - transform.position.y) / speed;
            }

            Vector3 movement = new Vector3(moveHorizontal, moveVertical, 0.0f);


            if (movement == new Vector3(0.0f, 0.0f, 0.0f) && cooldown == 0)
            {
                ChooseTarget();
                StartCoroutine(FlyingPointRoutine());
            }


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

            transform.position = transform.position + (movement * speed);
        }
        // MMA ground code
        else
        {
            float playerDist = Mathf.Abs(player.GetPosition().x - transform.position.x);
            // Grounded
            if (!jumping && !kicking)
            {
                // If just punching too much leave
                if (cooldown == 0 && punchCounter > Random.Range(4, 10))
                {
                    animator.SetTrigger("Jump");
                    punchCounter = 0;
                    cooldown = 75;
                }
                // If really close can jab twice
                if (cooldown == 0 && playerDist < 1.5f)
                {
                    if (Random.Range(0.0f, 1.0f) < 0.67f)
                    {
                        cooldown = 25;
                        animator.SetTrigger("Jab");
                        punchCounter++;
                    }
                    else
                    {
                        cooldown = 55;
                        animator.SetTrigger("JabTwice");
                        punchCounter++;
                    }
                }
                else if (cooldown == 0 && playerDist < 2.3f)
                {
                    cooldown = 29;
                    animator.SetTrigger("Jab");
                    punchCounter++;
                }
                else if (cooldown == 0 && playerDist >= 2.3f)
                {
                    animator.SetTrigger("Jump");
                    punchCounter = 0;
                    cooldown = 75;
                }
            }
            // Jumping/Hovering
            else if (jumping)
            {
                float speed = 0.05f;

                float moveHorizontal = jumpDirection;
                float moveVertical = targetY - transform.position.y;

                if (moveVertical != 0)
                {
                    moveVertical = moveVertical / Mathf.Abs(moveVertical);
                }
                if (Mathf.Abs(targetY - transform.position.y) < speed)
                {
                    moveVertical = (targetY - transform.position.y) / speed;
                }
                if (pointing)
                {
                    moveVertical = 0.0f;
                }

                Vector3 movement = new Vector3(moveHorizontal, moveVertical, 0.0f);

                transform.position = transform.position + (movement * speed);

                float strikeZone = transform.position.x + (jumpDirection * (Mathf.Abs(0.15f * ((transform.position.y - player.GetPosition().y + PLAYER_HEIGHT_ADJUSTMENT) / 0.32f))));
                // If player in strike zone, land
                if (cooldown == 0 && !pointing && player.GetPosition().x > strikeZone - 0.15f && player.GetPosition().x < strikeZone + 0.15f)
                {
                    StartCoroutine(LandRoutine());
                }
                // Can only start pointing in a jump if not nearing the edge of the screen
                else if (!pointing && pointTimer == 0 
                    && ((jumpDirection == -1 && transform.position.x > limitXLeft + 5.5f) 
                    || (jumpDirection == 1 && transform.position.x < limitXRight - 5.5f)))
                {
                    StartCoroutine(JumpingPointRoutine());
                    // Lock out the pointTimer so it doesn't infinitely set off
                    pointTimer = 99999;
                }
                // If somehow left screen, turn around immediately
                else if (!pointing && transform.position.x < (limitXLeft - 1.0f))
                {
                    jumpDirection = 1;
                }
                else if (!pointing && transform.position.x > (limitXRight + 1.0f))
                {
                    jumpDirection = -1;
                }
                // If strike zone is close to edge of screen, land (small chance to turn around in mid-air, increases as health decreases)
                else if (!pointing && (strikeZone < (limitXLeft + 1.0f) || strikeZone > (limitXRight - 1.0f)))
                {
                    if (Random.Range((120 - health) / 180.0f, 1.0f) > 0.75f)
                    {
                        jumpDirection = jumpDirection * -1;
                    }
                    else
                    {
                        StartCoroutine(LandRoutine());
                    }
                }

                //Timer only decreases when in position
                if (targetY == transform.position.y && pointTimer > 0)
                {
                    pointTimer--;
                }
            }
            else if (kicking)
            {
                float speed = 0.15f;

                float moveHorizontal = jumpDirection;

                Vector3 movement = new Vector3(moveHorizontal, 0.0f, 0.0f);

                transform.position = transform.position + (movement * speed);
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
    }

    private void HandleDirection()
    {
        if (!jumping && !kicking)
        {
            if (player.GetPosition().x < transform.position.x)
            {
                facingLeft = true;
                transform.localScale = new Vector3(6.0f, transform.localScale.y, transform.localScale.z);
            }
            else if (player.GetPosition().x >= transform.position.x)
            {
                facingLeft = false;
                transform.localScale = new Vector3(-6.0f, transform.localScale.y, transform.localScale.z);
            }
            if (flying)
            {
                drone1.SendMessage("SwitchDirection", facingLeft);
                drone2.SendMessage("SwitchDirection", facingLeft);
            }
        }
        else
        {
            if (jumpDirection > 0)
            {
                facingLeft = false;
                transform.localScale = new Vector3(-6.0f, transform.localScale.y, transform.localScale.z);
            }
            else
            {
                facingLeft = true;
                transform.localScale = new Vector3(6.0f, transform.localScale.y, transform.localScale.z);
            }
        }
    }

    private void ChooseTarget()
    {
        targetX = limitXLeft + 1.9f + (transform.position.x < limitXMid ? 15.5f : 0.0f);
        // Chance to target near the centre
        if (Mathf.Abs(transform.position.x - limitXMid) > 3.5f && Random.Range(0.0f, 1.0f) < 0.3333f)
        {
            targetX = targetX + (Random.Range(4.0f, 9.5f) * (transform.position.x < limitXMid ? -1 : 1));
        }
        // Else add some variability to the X pos
        else
        {
            targetX += Random.Range(-0.7f, 0.7f);
        }
        targetY = limitYTop + 4.5f;//;(Random.Range(3.0f, 4.9f));
    }

    public void Hurt(DamagePacket packet)
    {
        if (stun == 0)
        {
            health -= packet.getDamage();
            if (health > 0)
            {
                stun = 7;
                StartCoroutine(BlinkRoutine());
            }
            else
            {
                cooldown = 99999;
                stun = 99999;
                facingLeft = packet.getDirection();
                StartCoroutine(DeathRoutine());
            }

            // Drones
            if (flying)
            {
                drone1.SendMessage("Hurt");
                drone2.SendMessage("Hurt");
            }
            if (health < 81 && flying)
            {
                pointing = false;
                HandleDirection();
                drone1.SendMessage("Die");
                drone2.SendMessage("Die");
                StartCoroutine(DressFallRoutine());
            }
            ui.BossHealthBar(health);
        }
    }

    private IEnumerator FlyingPointRoutine()
    {
        int wait = Random.Range(55, 135);
        for (int i = 0; i < wait; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        //Don't need to check this because it can't trigger any animations after done flying
        pointing = true;
        animator.SetTrigger("PointPre");
        for (int i = 0; i < 37; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        if (ui.GameActive())
        {
            if (Random.Range(0.0f, 1.0f) < 0.5f)
            {
                animator.SetTrigger("PointForward");
            }
            else
            {
                animator.SetTrigger("PointBottom");
            }
        }
        for (int i = 0; i < 113; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        pointing = false;
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

    private IEnumerator DressFallRoutine()
    {
        flying = false;
        pointing = false;
        stun = 99999;
        ui.StartManualCutscene();
        animator.SetTrigger("Descend");
        for (int i = 0; i < 45; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        float midY = (limitYTop + limitYBottom) / 2;
        float midZ = -1.0f + (0.01f * (midY - limitYTop));
        while (transform.position.y > midY)
        {
            transform.position += new Vector3(0.0f, -0.05f, 0.0f);
            yield return new WaitForFixedUpdate();
        }
        animator.SetTrigger("Land");
        for (int i = 0; i < 150; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        animator.SetTrigger("RemoveDress");
        for (int i = 0; i < 100; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        // Sometimes this gets stuck causing the points to end prematurely in second phase
        animator.ResetTrigger("PointPre");
        ui.EndManualCutscene();
        stun = 0;
        cooldown = 25;
    }

    public void SpawnDressBG()
    {
        DressCrownController dressBG = Instantiate(dress, transform.position + new Vector3(-0.08f * (facingLeft ? 1 : -1), 0.0f, 0.02f), transform.rotation);
        dressBG.SetFacingLeft(facingLeft);
    }

    public void SpawnCrownBG()
    {
        DressCrownController crownBG = Instantiate(crown, transform.position + new Vector3(-0.2f * (facingLeft ? 1 : -1), 0.8f, 0.03f), transform.rotation);
        crownBG.SetFacingLeft(facingLeft);
    }

    public void ShootSideways()
    {
        StartCoroutine(ShootSidewaysRoutine());
    }

    public void ShootDown()
    {
        StartCoroutine(ShootDownRoutine());
    }

    private IEnumerator ShootSidewaysRoutine()
    {
        cooldown = 30 + Random.Range((health - 80), 40);
        bool shootLeft = facingLeft;
        float startPosX = limitXMid + (facingLeft ? 13.0f : -13.0f);
        float startPosY = limitYBottom + 0.35f + Random.Range(-0.35f, 0.25f);
        for (int i = 0; i < 150; i++)
        {
            // If triggers into dress falling or death, quit shooting
            if (!ui.GameActive())
            {
                break;
            }
            if (i % 30 == 0)
            {
                FlyingBarbController proj = Instantiate(barb, new Vector3(startPosX, startPosY + ((4 - (i / 30)) * 1.7f), transform.position.z), transform.rotation);
                proj.SetDirection(shootLeft ? -2 : 0);
            }

            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator ShootDownRoutine()
    {
        cooldown = 30 + Random.Range((health - 80), 40);
        bool shootLeft = facingLeft;
        float startPosX = limitXMid + (facingLeft ? -9.0f : 9.0f) + Random.Range(-0.25f, 0.25f);
        float startPosY = limitYTop + 12.0f;
        for (int i = 0; i < 150; i++)
        {
            // If triggers into dress falling or death, quit shooting
            if (!ui.GameActive())
            {
                break;
            }
            if (i % 15 == 0)
            {
                FlyingBarbController proj = Instantiate(barb, new Vector3(startPosX + ((9 - (i / 15)) * (2.05f * (shootLeft ? 1 : -1))), startPosY, transform.position.z), transform.rotation);
                proj.SetDirection(-1);
            }

            yield return new WaitForFixedUpdate();
        }
    }

    public void MoveHurtbox(int pos)
    {
        /*  Default - Dress
         *  1 - MMA Idle
         *  2 - MMA Hover
         *  3 - MMA Land
         * 
         */
        switch (pos)
        {
            case 1: { hurtbox.MoveDirect(new Vector3(-0.03f, -0.038f, 0.0f)); hurtbox.UpdateScale(0.1613008f, 0.4652178f); break; }
            case 2: { hurtbox.MoveDirect(new Vector3(-0.06388f, -0.04817f, 0.0f)); hurtbox.UpdateScale(0.2155077f, 0.4087553f); break; }
            case 3: { hurtbox.MoveDirect(new Vector3(-0.03461f, -0.14035f, 0.0f)); hurtbox.UpdateScale(0.1758289f, 0.2605018f); break; }
            default: { hurtbox.MoveDirect(new Vector3(0.00577f, -0.04045f, 0.0f)); hurtbox.UpdateScale(0.1613008f, 0.4652178f); break; }
        }
    }

    public void JabHitbox(int hitNum)
    {
        if (hitNum == 2)
        {
            HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(-0.15607f * 6.0f * (facingLeft ? 1 : -1), 0.03563f * 6.0f, 0.0f), transform.rotation);
            hit.SetX(0.2149694f * 6.0f);
            hit.SetY(0.09110817f * 6.0f);
            hit.SetTtl(50);
            hit.SetDamage(2);
            hit.SetParent(transform);
        }
        else
        {
            HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(-0.15573f * 6.0f * (facingLeft ? 1 : -1), 0.03643f * 6.0f, 0.0f), transform.rotation);
            hit.SetX(0.1931129f * 6.0f);
            hit.SetY(0.07835779f * 6.0f);
            hit.SetTtl(50);
            hit.SetDamage(1);
            hit.SetParent(transform);
        }
    }

    public void KickHitbox()
    {
        HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(-0.06312f * 6.0f * (facingLeft ? 1 : -1), -0.19968f * 6.0f, 0.0f), transform.rotation);
        hit.SetX(0.07321245f * 6.0f);
        hit.SetY(0.1377147f * 6.0f);
        hit.SetTtl(50);
        hit.SetDamage(1);
        hit.SetParent(transform);
    }

    public void LandHitbox()
    {
        HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(-0.05042f * 6.0f * (facingLeft ? 1 : -1), -0.21037f * 6.0f, 0.0f), transform.rotation);
        hit.SetX(0.3068324f * 6.0f);
        hit.SetY(0.1204649f * 6.0f);
        hit.SetTtl(100);
        hit.SetDamage(3);
        hit.SetParent(transform);
    }

    public void Jump()
    {
        StartCoroutine(JumpingRoutine());
        jumpDirection = facingLeft ? -1 : 1;
        if (transform.position.x < limitXLeft + 3.5f)
        {
            jumpDirection = 1;
        }
        else if (transform.position.x > limitXRight - 3.5f)
        {
            jumpDirection = -1;
        }
    }

    private IEnumerator JumpingRoutine()
    {
        pointTimer = 99999;
        jumping = true;
        int jumpFrames = Random.Range(26, 36) + (transform.position.y < limitYTop - 1.5f ? 6 : 0);
        bool cancel = false;
        for (int i = 0; i < jumpFrames; i++)
        {
            //If player is within kicking distance, go for it instead of going into the jump (only if high enough into the jump)
            if (i + 10 > jumpFrames)
            {
                float strikeZone = transform.position.x + (jumpDirection * (Mathf.Abs(0.15f * ((transform.position.y - player.GetPosition().y) / 0.32f))));
                if (player.GetPosition().x > strikeZone - 0.15f && player.GetPosition().x < strikeZone + 0.15f)
                {
                    cancel = true;
                    break;
                }
            }
            if (cancel)
            {
                break;
            }
            // Jump reduces as she ascends further
            float heightDelta = (((float) (jumpFrames - i)) / jumpFrames) * 0.29f;
            // X component of jump should already be handled in normal movement code
            if (health > 0)
            {
                transform.position += new Vector3(0.0f, heightDelta, 0.0f);
            }
            yield return new WaitForFixedUpdate();
        }
        // Hover if completed, kick if not
        if (!cancel)
        {
            animator.SetTrigger("Hover");
            // For some reason calling this in the animation isn't working properly. The transition is scuffed
            MoveHurtbox(2);
            ResetPointTimer();
            targetY = transform.position.y;
        }
        else
        {
            StartCoroutine(LandRoutine());
        }
    }

    private IEnumerator LandRoutine()
    {
        jumping = false;
        kicking = true;
        animator.SetTrigger("Kick");
        // Need to give player a warning and a chance (if was hovering)
        for (int i = 0; i < 8; i++)
        {
            transform.position += new Vector3(0.15f * (facingLeft ? 1 : -1), 0.0f, 0.0f);
            yield return new WaitForFixedUpdate();
        }
        int jumpFrames = ((int) ((transform.position.y - (player.GetPosition().y + PLAYER_HEIGHT_ADJUSTMENT)) / 0.32f)) + 1;
        float zDelta = (transform.position.z - player.GetPosition().z) / jumpFrames;
        for (int i = 0; i < jumpFrames; i++)
        {
            if (health > 0)
            {
                transform.position += new Vector3(0.0f, -0.32f, -zDelta);
            }
            yield return new WaitForFixedUpdate();
        }
        animator.SetTrigger("Land");
        cooldown = 80;
        kicking = false;
    }

    private IEnumerator JumpingPointRoutine()
    {
        pointing = true;
        bool pointDirection = Random.Range(0.0f, 1.0f) < 0.5f;
        if (pointDirection)
        {
            animator.SetTrigger("PointForward");
        }
        else
        {
            animator.SetTrigger("PointBottom");
        }
        for (int i = 0; i < 100; i++)
        {
            if (ui.GameActive())
            {
                transform.position = transform.position + new Vector3(0.0f, -0.065f, 0.0f);
            }
            if (i == 65)
            {
                if (pointDirection)
                {
                    ShootSideways();
                }
                else
                {
                    ShootDown();
                }
            }
            yield return new WaitForFixedUpdate();
        }
        pointing = false;
        // Reusing for cancelling point
        animator.SetTrigger("PointPre");
        ResetPointTimer();
    }

    private void ResetPointTimer()
    {
        int maxTime = 80 + ((health / 120) * 120);
        pointTimer = Random.Range(70, maxTime);
    }

    private IEnumerator DeathRoutine()
    {
        animator.SetTrigger("Die");
        ui.UpdateScore(15000L);
        ui.StartManualCutscene();
        // If in the air, land near player y position
        if (jumping || kicking)
        {
            if (transform.position.y > player.GetPosition().y)
            {
                int i = 0;
                while (transform.position.y > player.GetPosition().y)
                {
                    transform.position = transform.position + new Vector3(0.03f * (facingLeft ? 1 : -1), ((i - 30) / 40.0f) * -0.05f, 0.0f);
                    if (transform.position.x < limitXLeft || transform.position.x > limitXRight)
                    {
                        transform.position = transform.position + new Vector3(limitXLeft - transform.position.x, 0.0f, 0.0f);
                    }
                    i++;
                    yield return new WaitForFixedUpdate();
                }
            }
            else
            {
                for (int i = 0; i < 65; i++)
                {
                    transform.position = transform.position + new Vector3(0.03f * (facingLeft ? 1 : -1), ((i - 30) / 40.0f) * -0.05f, 0.0f);
                    if (transform.position.x < limitXLeft || transform.position.x > limitXRight)
                    {
                        transform.position = transform.position + new Vector3(limitXLeft - transform.position.x, 0.0f, 0.0f);
                    }
                    i++;
                    yield return new WaitForFixedUpdate();
                }
            }
        }
        // If on ground, just fall 
        else
        {
            for (int i = 0; i < 65; i++)
            {
                transform.position = transform.position + new Vector3(0.03f * (facingLeft ? 1 : -1), ((i - 30) / 40.0f) * -0.05f, 0.0f);
                if (transform.position.x < limitXLeft || transform.position.x > limitXRight)
                {
                    transform.position = transform.position + new Vector3(limitXLeft - transform.position.x, 0.0f, 0.0f);
                }
                i++;
                yield return new WaitForFixedUpdate();
            }
        }
        animator.SetTrigger("Land"); 
        for (int j = 0; j < 60; j++)
        {
            yield return new WaitForFixedUpdate();
        }
        animator.SetTrigger("Sit");
        ui.DisplayDialogue("QueenHeadshot", "We can see how you have managed|to come so far all by yourself.|But you should not go through|this trial alone - we shall help you.|As a mother and as Queen,|our gossip network spreads far.|We know where your trophies are hidden.|We shall send you with haste back Uptown.|Your rivals await!");
        ui.BossExit();
        ui.PrimeTransition("STM");

        ui.SaveGameState(false, 2);
    }

    public void SpawnRandom()
    {
        // supresses errors
    }
}
