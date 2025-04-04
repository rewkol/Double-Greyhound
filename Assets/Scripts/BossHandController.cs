using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHandController : MonoBehaviour
{
    public HitboxController hitbox;
    public HurtboxController hurtbox;
    public bool rightHand;

    private Transform hand;
    private Transform elbow;
    private Transform lowerArmT;
    private Transform hurtbox1;
    private Transform hurtbox2;
    private Transform hurtbox3;
    private Transform hurtboxHand;
    private Transform hurtboxForearm;
    private Transform hurtboxElbow;
    private Transform transform;
    private Animator animator;

    private PlayerController player;
    private BossArmController upperArm;
    private BossArmController lowerArm;

    private bool state;
    private bool turned;
    private float acceleration;
    private float speed;
    private bool attackReady;
    private bool primeAttack;
    private bool idle;
    private bool dead;
    private int stun;
    private int cooldown;
    private float midpoint;
    private float edgepoint;
    private int cooldownModifier;

    private const float MAX_SPEED = 0.2f;
    private const float SPACING = 0.7f;
    private const float ELBOW_RANGE = 0.33f;

    private SFXController sfxController;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        hand = transform.Find("BossHand");
        elbow = transform.parent.Find("BossArmElbow" + (rightHand ? "Right" : "Left"));
        upperArm = transform.parent.Find("BossArm" + (rightHand ? "R" : "L") + "1").gameObject.GetComponent<BossArmController>();
        lowerArmT = transform.parent.Find("BossArm" + (rightHand ? "R" : "L") + "3");
        lowerArm = lowerArmT.gameObject.GetComponent<BossArmController>();
        hurtbox1 = transform.Find("Hurtbox1");
        hurtbox2 = transform.Find("Hurtbox2");
        hurtbox3 = transform.Find("Hurtbox3");
        animator = hand.gameObject.GetComponent<Animator>();

        player = GameObject.FindObjectsOfType<PlayerController>()[0];

        state = false;
        turned = false;
        attackReady = false;
        primeAttack = false;
        cooldown = 0;
        cooldownModifier = 15;
        idle = true;
        stun = 0;
        dead = false;

        acceleration = 0.03f;
        speed = 0.0f;


        midpoint = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        edgepoint = Camera.main.ViewportToWorldPoint(new Vector3(0.0f + (rightHand ? 1.0f : 0.0f), 0.0f, transform.position.z - Camera.main.transform.position.z)).x + (rightHand ? -1.5f : 1.5f);

        sfxController = GameObject.FindObjectOfType<SFXController>();

        StartCoroutine(EntranceRoutine());
    }

    void FixedUpdate()
    {
        if (!state)
        {
            InterpolateArmParts();
            // Need this to make sure you can attack during opening cutscene
            if (stun > 0)
            {
                stun--;
            }
            return;
        }

        if (turned)
        {
            float localAccel = acceleration;
            if (player.GetPosition().y < transform.position.y)
            {
                localAccel = -acceleration;
            }

            attackReady = Mathf.Abs(player.GetPosition().y - transform.position.y) < SPACING;
            if (attackReady)
            {
                if (Mathf.Abs(speed) > acceleration)
                {
                    localAccel = speed > 0.0f ? -acceleration : acceleration;
                }
                else
                {
                    localAccel = 0.0f;
                    speed = 0.0f;
                }
            }

            speed += localAccel;

            if (speed > MAX_SPEED)
            {
                speed = MAX_SPEED;
            }
            else if (speed < -MAX_SPEED)
            {
                speed = -MAX_SPEED;
            }

            transform.position += new Vector3(0.0f, speed, 0.0f);
            elbow.position += new Vector3(0.0f, speed * ELBOW_RANGE, 0.0f);

            // Adjust x position to be near edge of screen, but move closer to player if player near centre
            if ((rightHand && transform.position.x < edgepoint) || (!rightHand && transform.position.x > edgepoint))
            {
                float direction = rightHand ? 0.1f : -0.1f;
                transform.position += new Vector3(direction, 0.0f, 0.0f);
                elbow.position += new Vector3(direction * ELBOW_RANGE, 0.0f, 0.0f);
            }
            idle = false;
        }
        else
        {
            Vector3 target = player.GetPosition();
            if ((rightHand && player.GetPosition().x < midpoint + 1.0f) || (!rightHand && player.GetPosition().x > midpoint - 1.0f))
            {
                target = new Vector3(midpoint + (rightHand ? 6.0f : -6.0f), target.y, 0.0f);
                idle = true;
            }
            else
            {
                idle = false;
            }

            float localAccel = acceleration;
            if (target.x < transform.position.x)
            {
                localAccel = -acceleration;
            }

            attackReady = Mathf.Abs(target.x - transform.position.x) < SPACING;
            if (attackReady || ((rightHand && target.x < midpoint + 1.0f) || (!rightHand && target.x > midpoint - 1.0f)))
            {
                if (Mathf.Abs(speed) > acceleration)
                {
                    localAccel = speed > 0.0f ? -acceleration : acceleration;
                }
                else
                {
                    localAccel = 0.0f;
                    speed = 0.0f;
                }
            }

            speed += localAccel;

            if (speed > MAX_SPEED)
            {
                speed = MAX_SPEED;
            }
            else if (speed < -MAX_SPEED)
            {
                speed = -MAX_SPEED;
            }

            transform.position += new Vector3(speed, 0.0f, 0.0f);
            elbow.position += new Vector3(speed * ELBOW_RANGE, 0.0f, 0.0f);

            // Adjust height to be above player
            if (((!rightHand && target.x < midpoint + 1.0f) || (rightHand && target.x > midpoint - 1.0f)))
            {
                float vertDiff = transform.position.y - target.y;
                if (vertDiff < 5.0f)
                {
                    transform.position += new Vector3(0.0f, 0.1f, 0.0f);
                    elbow.position += new Vector3(0.0f, 0.1f * ELBOW_RANGE, 0.0f);
                    if (vertDiff < 4.0f)
                    {
                        idle = true;
                    }
                }
                else if (vertDiff > 7.0f)
                {
                    transform.position += new Vector3(0.0f, -0.1f, 0.0f);
                    elbow.position += new Vector3(0.0f, -0.1f * ELBOW_RANGE, 0.0f);
                }
            }
        }

        if (primeAttack && attackReady)
        {
            Attack();
        }

        if (stun > 0)
        {
            stun--;
        }
        if (cooldown > 0)
        {
            cooldown--;
        }
        InterpolateArmParts();
    }

    private void InterpolateArmParts()
    {
        upperArm.Interpolate();
        lowerArm.Interpolate();
    }

    public void SetState(bool state)
    {
        this.state = state;
    }

    public void PrimeAttack()
    {
        // For the slams it is fine to prime attack and wait to be ready
        if (state && cooldown == 0)
        {
            this.primeAttack = true;
        }
        // For claps both hands need to act as one so can only go if both hands are ready
        if (this.primeAttack && turned && !attackReady)
        {
            this.primeAttack = false;
        }
    }

    public void RescindAttack()
    {
        this.primeAttack = false;
    }

    public bool AttackPrimed()
    {
        return this.primeAttack;
    }

    public void FireAttack()
    {
        if (state && cooldown == 0)
        {
            StartCoroutine(FireRoutine());
        }
    }

    public void Attack()
    {
        if (turned)
        {
            StartCoroutine(ClapRoutine());
        }
        else
        {
            StartCoroutine(SlamRoutine());
        }
    }

    private IEnumerator EntranceRoutine()
    {
        // Reset hand position so that vertical is positioned correctly
        GripCutscene();
        yield return new WaitForFixedUpdate();
        Vertical();

        if (rightHand)
        {
            // Delay
            for (int i = 0; i < 75; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            for (int i = 0; i < 75; i++)
            {
                transform.position += new Vector3(0.0f, 0.009f, 0.0f);
                yield return new WaitForFixedUpdate();
            }
            Grip();
            transform.position += new Vector3(0.0f, 1.233f, 0.0f);
            for (int i = 0; i < 250; i++)
            {
                transform.position += new Vector3(0.0f, -0.011f, 0.0f);
                yield return new WaitForFixedUpdate();
            }
            Vertical();
            transform.position += new Vector3(0.0f, -1.233f, 0.0f);
            for (int i = 0; i < 150; i++)
            {
                transform.position += new Vector3(0.0f, 0.01f, 0.0f);
                yield return new WaitForFixedUpdate();
            }
            HorizontalHitbox();
            SlamCutscene();
            HorizontalHitbox();
            for (int i = 0; i < 200; i++)
            {
                transform.position += new Vector3(0.0f, -0.011f, 0.0f);
                yield return new WaitForFixedUpdate();
                if (i > 73)
                {
                    elbow.position += new Vector3(0.0f, -0.011f, 0.0f);
                }
            }
            Dangle();
        }
        else
        {
            for (int i = 0; i < 75; i++)
            {
                transform.position += new Vector3(0.0f, 0.02f, 0.0f);
                yield return new WaitForFixedUpdate();
            }
            Grip();
            transform.position += new Vector3(0.0f, 1.233f, 0.0f);
            for (int i = 0; i < 100; i++)
            {
                transform.position += new Vector3(0.0f, -0.011f, 0.0f);
                yield return new WaitForFixedUpdate();
            }
            Vertical();
            transform.position += new Vector3(0.0f, -1.233f, 0.0f);
            for (int i = 0; i < 225; i++)
            {
                transform.position += new Vector3(0.0f, 0.002f, 0.0f);
                yield return new WaitForFixedUpdate();
            }
            HorizontalHitbox();
            SlamCutscene();
            HorizontalHitbox();
            for (int i = 0; i < 350; i++)
            {
                transform.position += new Vector3(0.0f, -0.011f, 0.0f);
                if (i > 223)
                {
                    elbow.position += new Vector3(0.0f, -0.011f, 0.0f);
                }
                yield return new WaitForFixedUpdate();
            }
            Dangle();
        }

        for (int i = 0; i < 500; i++)
        {
            if (transform.localPosition.y < -0.596f)
            {
                transform.position += new Vector3(0.0f, 0.015f, 0.0f);
            }
            if (elbow.localPosition.y > -0.64f)
            {
                elbow.position += new Vector3(0.0f, -0.015f, 0.0f);
            }
            yield return new WaitForFixedUpdate();
        }
        state = true;
    }

    private IEnumerator ClapRoutine()
    {
        if (!turned || !attackReady)
        {
            yield break;
        }
        state = false;
        primeAttack = false;
        yield return new WaitForFixedUpdate();
        for (int i = 0; i < 30; i++)
        {
            // These are annoying but are needed so hands don't become alive again during death cutscene
            if (dead)
            {
                yield break;
            }
            transform.position += new Vector3(0.05f * (rightHand ? 1 : -1), 0.0f, 0.0f);
            elbow.position += new Vector3(0.05f * (rightHand ? 1 : -1) * ELBOW_RANGE, 0.0f, 0.0f);
            yield return new WaitForFixedUpdate();
        }
        if (dead)
        {
            yield break;
        }
        Flat();
        if (rightHand)
        {
            while (transform.position.x > midpoint + 1.0f)
            {
                if (dead)
                {
                    yield break;
                }
                transform.position += new Vector3(-0.75f, 0.0f, 0.0f);
                elbow.position += new Vector3(-0.75f * ELBOW_RANGE, 0.0f, 0.0f);
                VerticalHitbox();
                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            while (transform.position.x < midpoint - 1.0f)
            {
                if (dead)
                {
                    yield break;
                }
                transform.position += new Vector3(0.75f, 0.0f, 0.0f);
                elbow.position += new Vector3(0.75f * ELBOW_RANGE, 0.0f, 0.0f);
                VerticalHitbox();
                yield return new WaitForFixedUpdate();
            }
        }
        sfxController.PlaySFX2D("SJHS/Clap_Trimmed", 1.0f, 15, 0.05f, false);
        for (int i = 0; i < 30; i++)
        {
            if (dead)
            {
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }
        if (dead)
        {
            yield break;
        }
        Dangle();
        state = true;
        cooldown = 150;
    }

    private IEnumerator SlamRoutine()
    {
        if (turned || !attackReady)
        {
            yield break;
        }
        state = false;
        primeAttack = false;
        yield return new WaitForFixedUpdate();
        float target = player.GetPosition().y - 0.25f;
        for (int i = 0; i < 30; i++)
        {
            if (dead)
            {
                yield break;
            }
            transform.position += new Vector3(0.0f, 0.05f, 0.0f);
            elbow.position += new Vector3(0.0f, 0.05f * ELBOW_RANGE, 0.0f);
            yield return new WaitForFixedUpdate();
        }
        if (dead)
        {
            yield break;
        }
        Flat(); if (dead)
        {
            yield break;
        }
        while (transform.position.y > target)
        {
            if (dead)
            {
                yield break;
            }
            transform.position += new Vector3(0.0f, -0.45f, 0.0f);
            elbow.position += new Vector3(0.0f, -0.45f * ELBOW_RANGE, 0.0f);
            HorizontalHitbox();
            yield return new WaitForFixedUpdate();
        }
        if (dead)
        {
            yield break;
        }
        Slam();
        for (int i = 0; i < 40; i++)
        {
            if (dead)
            {
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }
        if (dead)
        {
            yield break;
        }
        Dangle();
        state = true;
        cooldown = 120;
    }

    private IEnumerator FireRoutine()
    {
        state = false;
        primeAttack = false;
        yield return new WaitForFixedUpdate();
        float targetX = midpoint + (rightHand ? 6.0f : -6.0f);
        float targetY = player.GetPosition().y - 0.25f;
        for (int i = 0; i < 30; i++)
        {
            if (dead)
            {
                yield break;
            }
            float x = 0.0f;
            if (Mathf.Abs(targetX - transform.position.x) > SPACING)
            {
                x = targetX < transform.position.x ? -0.1f : 0.1f;
            }
            transform.position += new Vector3(x, 0.05f, 0.0f);
            elbow.position += new Vector3(x * ELBOW_RANGE, 0.05f * ELBOW_RANGE, 0.0f);
            yield return new WaitForFixedUpdate();
        }
        if (dead)
        {
            yield break;
        }
        Flat();
        while (transform.position.y > targetY)
        {
            if (dead)
            {
                yield break;
            }
            transform.position += new Vector3(0.0f, -0.45f, 0.0f);
            elbow.position += new Vector3(0.0f, -0.45f * ELBOW_RANGE, 0.0f);
            HorizontalHitbox();
            yield return new WaitForFixedUpdate();
        }
        if (dead)
        {
            yield break;
        }
        Slam();
        for (int i = 0; i < 560; i++)
        {
            if (dead)
            {
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }
        if (dead)
        {
            yield break;
        }
        Dangle();
        state = true;
        cooldown = 150;
    }

    public void HorizontalHitbox()
    {
        CreateHitbox(hand.position + new Vector3((rightHand ? -1 : 1) * 0.0061f, -0.02821f, 0.0f), 0.5642596f * 6, 0.1574631f * 6, 50, 2);
    }

    public void VerticalHitbox()
    {
        CreateHitbox(hand.position + new Vector3((rightHand ? -1 : 1) * 0.01607f, 0.00945f, 0.0f), 0.1573392f * 6, 0.5868945f * 6, 50, 2);
    }

    public void CreateHitbox(Vector3 vector, float x, float y, int ttl, int damage)
    {
        HitboxController hit = Instantiate(hitbox, vector, transform.rotation);
        hit.SetX(x);
        hit.SetY(y);
        hit.SetTtl(ttl);
        hit.SetDamage(damage);
        hit.SetParent(transform);
    }

    // When I decided on this approach I thought these would be more dissimilar than they ended up being
    public void Grip()
    {
        animator.SetTrigger("Grip");
        elbow.localPosition = new Vector3(elbow.localPosition.x, elbow.localPosition.y, -0.002f);
        lowerArmT.localPosition = new Vector3(lowerArmT.localPosition.x, lowerArmT.localPosition.y, -0.003f);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -0.01f);
        hand.localPosition = new Vector3(0.0f, 0.0f, -0.1f);

        hurtbox1.localPosition = new Vector3((rightHand ? -1 : 1) * -0.05598f, 0.24528f, 0.0f);
        hurtbox1.localScale = new Vector3(0.5354816f, 0.1017087f, 1.0f);

        hurtbox2.localPosition = new Vector3((rightHand ? -1 : 1) * -0.05598f, 0.24528f, 0.0f);
        hurtbox2.localScale = new Vector3(0.5354816f, 0.1017087f, 1.0f);

        hurtbox3.localPosition = new Vector3((rightHand ? -1 : 1) * -0.05598f, 0.24528f, 0.0f);
        hurtbox3.localScale = new Vector3(0.5354816f, 0.1017087f, 1.0f); 

        sfxController.PlaySFX2D("SJHS/Slap", 0.3f, 15, 0.05f, false);
    }

    // Needs to be far back or it clips through the pool top
    public void GripCutscene()
    {
        animator.SetTrigger("Grip");
        elbow.localPosition = new Vector3(elbow.localPosition.x, elbow.localPosition.y, 1.0f);
        lowerArmT.localPosition = new Vector3(lowerArmT.localPosition.x, lowerArmT.localPosition.y, 1.1f);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 1.2f);
        hand.localPosition = new Vector3(0.0f, 0.0f, -0.0001f);

        hurtbox1.localPosition = new Vector3((rightHand ? -1 : 1) * -0.05598f, 0.24528f, 0.0f);
        hurtbox1.localScale = new Vector3(0.5354816f, 0.1017087f, 1.0f);

        hurtbox2.localPosition = new Vector3((rightHand ? -1 : 1) * -0.05598f, 0.24528f, 0.0f);
        hurtbox2.localScale = new Vector3(0.5354816f, 0.1017087f, 1.0f);

        hurtbox3.localPosition = new Vector3((rightHand ? -1 : 1) * -0.05598f, 0.24528f, 0.0f);
        hurtbox3.localScale = new Vector3(0.5354816f, 0.1017087f, 1.0f);
    }

    public void Dangle()
    {
        animator.SetTrigger("Dangle");
        elbow.localPosition = new Vector3(elbow.localPosition.x, elbow.localPosition.y, -0.950001f);
        lowerArmT.localPosition = new Vector3(lowerArmT.localPosition.x, lowerArmT.localPosition.y, -0.950002f);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -0.950003f);
        hand.localPosition = new Vector3(0.0f, 0.0f, -0.05f);

        hurtbox1.localPosition = new Vector3((rightHand ? -1 : 1) * -0.123f, -0.07800001f, 0.0f);
        hurtbox1.localScale = new Vector3(0.3017008f, 0.1469181f, 1.0f);

        hurtbox2.localPosition = new Vector3((rightHand ? -1 : 1) * 0.03871995f, -0.002010047f, 0.0f);
        hurtbox2.localScale = new Vector3(0.489107f, 0.1437952f, 1.0f);

        hurtbox3.localPosition = new Vector3((rightHand ? -1 : 1) * 0.06488991f, 0.12145f, 0.0f);
        hurtbox3.localScale = new Vector3(0.3339755f, 0.0906952f, 1.0f);
    }

    public void Flat()
    {
        animator.SetTrigger("Flat");
        elbow.localPosition = new Vector3(elbow.localPosition.x, elbow.localPosition.y, -0.950001f);
        lowerArmT.localPosition = new Vector3(lowerArmT.localPosition.x, lowerArmT.localPosition.y, -0.950002f);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -0.950003f);
        hand.localPosition = new Vector3(0.0f, -0.069f, -0.05f);

        hurtbox1.localPosition = new Vector3((rightHand ? -1 : 1) * 0.00031f, -0.08633f, 0.0f);
        hurtbox1.localScale = new Vector3(0.606316f, 0.1755701f, 1.0f);

        hurtbox2.localPosition = new Vector3((rightHand ? -1 : 1) * 0.00031f, -0.08633f, 0.0f);
        hurtbox2.localScale = new Vector3(0.606316f, 0.1755701f, 1.0f);

        hurtbox3.localPosition = new Vector3((rightHand ? -1 : 1) * 0.00031f, -0.08633f, 0.0f);
        hurtbox3.localScale = new Vector3(0.606316f, 0.1755701f, 1.0f);
    }

    public void Slam()
    {
        animator.SetTrigger("Slam");
        elbow.localPosition = new Vector3(elbow.localPosition.x, elbow.localPosition.y, -0.950001f);
        lowerArmT.localPosition = new Vector3(lowerArmT.localPosition.x, lowerArmT.localPosition.y, -0.950002f);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -0.950003f);
        hand.localPosition = new Vector3(0.0f, -0.38f, -0.00001f);

        hurtbox1.localPosition = new Vector3((rightHand ? -1 : 1) * -0.02907f, -0.21064f, 0.0f);
        hurtbox1.localScale = new Vector3(0.38673f, 0.2104544f, 1.0f);

        hurtbox2.localPosition = new Vector3((rightHand ? -1 : 1) * -0.06555f, -0.44618f, 0.0f);
        hurtbox2.localScale = new Vector3(0.4521676f, 0.3387533f, 1.0f);

        hurtbox3.localPosition = new Vector3((rightHand ? -1 : 1) * 0.22394f, -0.34359f, 0.0f);
        hurtbox3.localScale = new Vector3(0.1718516f, 0.2753911f, 1.0f);

        sfxController.PlaySFX2D("SJHS/Slap", 1.0f, 15, 0.05f, false);
    }

    public void SlamCutscene()
    {
        animator.SetTrigger("Slam");
        elbow.localPosition = new Vector3(elbow.localPosition.x, elbow.localPosition.y, -0.002f);
        lowerArmT.localPosition = new Vector3(lowerArmT.localPosition.x, lowerArmT.localPosition.y, -0.003f);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -0.950003f);
        hand.localPosition = new Vector3(0.0f, -0.38f, -0.00001f);

        hurtbox1.localPosition = new Vector3((rightHand ? -1 : 1) * -0.02907f, -0.21064f, 0.0f);
        hurtbox1.localScale = new Vector3(0.38673f, 0.2104544f, 1.0f);

        hurtbox2.localPosition = new Vector3((rightHand ? -1 : 1) * -0.06555f, -0.44618f, 0.0f);
        hurtbox2.localScale = new Vector3(0.4521676f, 0.3387533f, 1.0f);

        hurtbox3.localPosition = new Vector3((rightHand ? -1 : 1) * 0.22394f, -0.34359f, 0.0f);
        hurtbox3.localScale = new Vector3(0.1718516f, 0.2753911f, 1.0f);

        sfxController.PlaySFX2D("SJHS/Slap", 0.6f, 15, 0.05f, false);
    }

    public bool CanTurn()
    {
        return state && cooldown == 0;
    }

    public void Turn()
    {
        // Have to use coroutines because the hand is no longer an active object
        primeAttack = false;
        StartCoroutine(TurnRoutine());
    }

    private IEnumerator TurnRoutine()
    {
        state = false;
        animator.SetTrigger("Turn");
        for (int i = 0; i < 13; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        HalfTurned();
        for (int i = 0; i < 12; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        if (turned)
        {
            Dangle();
            animator.ResetTrigger("Dangle");
        }
        else
        {
            SideDangle();
        }
        turned = !turned;
        state = true;
    }

    public void HalfTurned()
    {
        // animation controlled externally
        elbow.localPosition = new Vector3(elbow.localPosition.x, elbow.localPosition.y, -0.950001f);
        lowerArmT.localPosition = new Vector3(lowerArmT.localPosition.x, lowerArmT.localPosition.y, -0.950002f);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -0.950003f);
        hand.localPosition = new Vector3(0.0f, 0.0f, -0.05f);

        hurtbox1.localPosition = new Vector3((rightHand ? -1 : 1) * -0.09239f, -0.15295f, 0.0f);
        hurtbox1.localScale = new Vector3(0.245129f, 0.163256f, 1.0f);

        hurtbox2.localPosition = new Vector3((rightHand ? -1 : 1) * -0.04671f, 0.08322f, 0.0f);
        hurtbox2.localScale = new Vector3(0.23874f, 0.2771883f, 1.0f);

        hurtbox3.localPosition = new Vector3((rightHand ? -1 : 1) * 0.12845f, 0.1571f, 0.0f);
        hurtbox3.localScale = new Vector3(0.1287564f, 0.1891992f, 1.0f);
    }

    public void SideDangle()
    {
        // animation controlled externally
        elbow.localPosition = new Vector3(elbow.localPosition.x, elbow.localPosition.y, -0.950001f);
        lowerArmT.localPosition = new Vector3(lowerArmT.localPosition.x, lowerArmT.localPosition.y, -0.950002f);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -0.950003f);
        hand.localPosition = new Vector3(0.0f, 0.0f, -0.05f);

        hurtbox1.localPosition = new Vector3((rightHand ? -1 : 1) * -0.02431f, -0.13557f, 0.0f);
        hurtbox1.localScale = new Vector3(0.2205033f, 0.2761263f, 1.0f);

        hurtbox2.localPosition = new Vector3((rightHand ? -1 : 1) * -0.04671f, 0.07399f, 0.0f);
        hurtbox2.localScale = new Vector3(0.23874f, 0.2587192f, 1.0f);

        hurtbox3.localPosition = new Vector3((rightHand ? -1 : 1) * 0.0614f, 0.188f, 0.0f);
        hurtbox3.localScale = new Vector3(0.1759559f, 0.1891992f, 1.0f);
    }

    public void SideFlat()
    {
        animator.SetTrigger("Flat");
        elbow.localPosition = new Vector3(elbow.localPosition.x, elbow.localPosition.y, -0.950001f);
        lowerArmT.localPosition = new Vector3(lowerArmT.localPosition.x, lowerArmT.localPosition.y, -0.950002f);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -0.950003f);
        hand.localPosition = new Vector3((rightHand ? -1 : 1) * 0.09f, 0.0f, -0.05f);

        hurtbox1.localPosition = new Vector3((rightHand ? -1 : 1) * 0.0934f, 0.005f, 0.0f);
        hurtbox1.localScale = new Vector3(0.1650952f, 0.6065252f, 1.0f);

        hurtbox2.localPosition = new Vector3((rightHand ? -1 : 1) * 0.0934f, 0.005f, 0.0f);
        hurtbox2.localScale = new Vector3(0.1650952f, 0.6065252f, 1.0f);

        hurtbox3.localPosition = new Vector3((rightHand ? -1 : 1) * 0.0934f, 0.005f, 0.0f);
        hurtbox3.localScale = new Vector3(0.1650952f, 0.6065252f, 1.0f);
    }

    public void Vertical()
    {
        animator.SetTrigger("Die");
        elbow.localPosition = new Vector3(elbow.localPosition.x, elbow.localPosition.y, -0.002f);
        lowerArmT.localPosition = new Vector3(lowerArmT.localPosition.x, lowerArmT.localPosition.y, -0.003f);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -0.01f);
        hand.localPosition = new Vector3(0.0f, 0.323f, -0.0001f);

        hurtbox1.localPosition = new Vector3((rightHand ? -1 : 1) * -0.00919f, 0.33251f, 0.0f);
        hurtbox1.localScale = new Vector3(0.3908336f, 0.624998f, 1.0f);

        hurtbox2.localPosition = new Vector3((rightHand ? -1 : 1) * -0.24075f, 0.4019f, 0.0f);
        hurtbox2.localScale = new Vector3(0.1012449f, 0.2669264f, 1.0f);

        hurtbox3.localPosition = new Vector3((rightHand ? -1 : 1) * 0.205f, 0.22924f, 0.0f);
        hurtbox3.localScale = new Vector3(0.1759559f, 0.3451636f, 1.0f);
    }


    public void Hurt(DamagePacket packet)
    {
        if (stun == 0)
        {
            transform.parent.gameObject.SendMessage("Hurt", packet);
            stun = 5;
        }
    }

    public void Die()
    {
        Vertical();
        state = false;
        dead = true;
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        float target = midpoint + (rightHand ? 6.0f : -6.0f);
        for (int i = 0; i < 600; i++)
        {
            if (transform.localPosition.y < 0.2f)
            {
                transform.position += new Vector3(0.0f, 0.010f, 0.0f);
            }
            if (elbow.localPosition.y < -0.3f)
            {
                elbow.position += new Vector3(0.0f, 0.010f * ELBOW_RANGE, 0.0f);
            }
            bool inPos = Mathf.Abs(target - transform.position.x) < SPACING;
            if (!inPos)
            {
                // Go Left
                if (target - transform.position.x < 0.0f)
                {
                    transform.position += new Vector3(-0.010f, 0.0f, 0.0f);
                    elbow.position += new Vector3(-0.010f * ELBOW_RANGE, 0.0f, 0.0f);
                }
                // Go Right
                else
                {
                    transform.position += new Vector3(0.010f, 0.0f, 0.0f);
                    elbow.position += new Vector3(0.010f * ELBOW_RANGE, 0.0f, 0.0f);
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public void ReduceCooldown()
    {
        this.cooldownModifier -= 5;
    }
}
