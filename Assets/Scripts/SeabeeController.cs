using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeabeeController : MonoBehaviour
{
    //Public Objects
    public HitboxController hitbox;
    public FlyingBarbController barb;
    public bool freeWill;

    //Public variables
    public float speed;
    public float limitYTop;
    public float limitYBottom;
    public float shootChance;

    //Private variables
    private Transform transform;
    private Animator animator;
    private UIController ui;
    private bool facingLeft;
    private int cooldown;
    private int stun;
    private int swarmStun;
    private int health;
    private PlayerController player;
    private bool nextAttackSwoop;
    private float height;
    private bool running;
    private float targetX;
    private float targetZ;
    private float targetHeight;
    private int untilAttack;
    private int attackPrimed;
    private float attackDistance;
    private bool stuck;
    private float limitXLeft;
    private float limitXRight;
    private bool inPos;

    //Constants
    private float DEFAULT_HEIGHT = 1.32f;

    private SFXController sfxController;


    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        ui = GameObject.FindObjectsOfType<UIController>()[0];
        player = GameObject.FindObjectsOfType<PlayerController>()[0];
        facingLeft = true;
        cooldown = 0;
        stun = 0;
        //Stun for the miniboss version
        swarmStun = 0;
        health = 10;
        inPos = false;
        height = 0.0f;
        running = false;
        untilAttack = Random.Range(5, 11);
        attackPrimed = 0;
        attackDistance = 0.0f;

        targetHeight = 0.0f;
        targetX = player.GetPosition().x + Random.Range(-3.0f, 3.0f);
        targetZ = player.GetPosition().z + Random.Range(0.042f, 0.048f);

        //Add height to Y position without changing Z. Height and Z can both change, and both affect where Y on screen is
        transform.position = new Vector3(transform.position.x, limitYTop + height, -1.0f);

        nextAttackSwoop = false;
        if (Random.Range(0.0f, 1.0f) < 0.5f)
        {
            nextAttackSwoop = true;
        }

        limitXLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        limitXRight = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;

        sfxController = GameObject.FindObjectOfType<SFXController>();
    }

    void FixedUpdate()
    {
        if (player.IsDead() && health > 0 && cooldown == 0)
        {
            if (!running)
            {
                animator.SetTrigger("Fly");
            }
            else
            {
                animator.SetTrigger("Run");
            }
            transform.localScale = new Vector3(-6.0f, transform.localScale.y, transform.localScale.z);
            transform.position += new Vector3(speed, 0.0f, 0.0f);
            // Using stun instead because of how attacks work here
            stun++;
            if (stun > 1000)
            {
                transform.position += new Vector3(speed, 0.0f, 0.0f);
                //Don't despawn because despawning moves the camera around
            }
            return;
        }

        if (!ui.GameActive())
        {
            return;
        }

        //Movement code
        float moveHorizontal = targetX - transform.position.x;
        float moveVertical = (targetZ - transform.position.z) * 100.0f;

        //Can't flip around while cooling down
        if (cooldown <= 1 && stun <= 1)
        {
            if (moveHorizontal > 0)
            {
                facingLeft = false;
                transform.localScale = new Vector3(-6.0f, transform.localScale.y, transform.localScale.z);
            }
            else if (moveHorizontal < 0)
            {
                facingLeft = true;
                transform.localScale = new Vector3(6.0f, transform.localScale.y, transform.localScale.z);
            }
        }

        if (moveHorizontal != 0)
        {
            moveHorizontal = moveHorizontal / Mathf.Abs(moveHorizontal);
        }
        if (moveVertical != 0)
        {
            moveVertical = moveVertical / Mathf.Abs(moveVertical);
        }

        float moveZ = 0.01f * moveVertical;

        if (moveZ == 0 && targetHeight != height)
        {
            moveVertical = targetHeight - height;
            moveVertical = moveVertical / Mathf.Abs(moveVertical);
        }

        //Reduce if in range of target
        if (Mathf.Abs(targetX - transform.position.x) < speed)
        {
            moveHorizontal = (targetX - transform.position.x) / speed;
        }
        if (moveZ > 0.0f && Mathf.Abs(targetZ - transform.position.z) < speed * 0.01f)
        {
            moveVertical = ((targetZ - transform.position.z) * 100.0f) / speed;
            moveZ = (targetZ - transform.position.z) / speed;
        }
        if (moveZ == 0.0f && Mathf.Abs(targetHeight - height) < speed)
        {
            moveVertical = (targetHeight - height) / speed;
            moveZ = 0.0f;
        }

        Vector3 movement = new Vector3(moveHorizontal, moveVertical, moveZ);

        //If in position choose new target or do attack
        if (freeWill && movement == new Vector3(0.0f, 0.0f, 0.0f))
        {
            untilAttack--;
            if (untilAttack == 0 && !running)
            {
                //Shoot then run
                if (Random.Range(0.0f, 1.0f) <= ((shootChance / 2.0f) + ((5 - health) / 5.0f)))
                {
                    if (transform.position.x < player.GetPosition().x)
                    {
                        targetX = player.GetPosition().x + Random.Range(2.5f, 4.0f);
                    }
                    else
                    {
                        targetX = player.GetPosition().x - Random.Range(2.5f, 4.0f);
                    }
                    float targetY = 0.0f;
                    targetY = player.GetPosition().y + Random.Range(-1.5f, 0.5f);
                    BindTargetY(targetY);
                    attackPrimed = 3;
                }
                else
                {
                    //Swoop
                    if (nextAttackSwoop)
                    {
                        float diff = Random.Range(4.0f, 7.0f);
                        targetX = player.GetPosition().x + (diff * (transform.position.x < player.GetPosition().x ? 1 : -1));
                        targetZ = player.GetPosition().z;
                        targetHeight = diff;
                        attackDistance = diff;
                        attackPrimed = 1;
                    }
                    //Drop
                    else
                    {
                        targetX = player.GetPosition().x + Random.Range(-0.2f, 0.2f);
                        targetZ = player.GetPosition().z;
                        targetHeight = Random.Range(4.0f, 6.0f);
                        attackDistance = targetHeight;
                        attackPrimed = 2;
                    }
                }

                untilAttack = Random.Range(6, 12);
                nextAttackSwoop = Random.Range(0.0f, 1.0f) < 0.5f ? true : false;
            }
            else if (attackPrimed > 0)
            {
                if (attackPrimed == 1)
                {
                    cooldown = 999;
                    animator.SetTrigger("Swoop");
                    if (transform.position.x < player.GetPosition().x)
                    {
                        facingLeft = false;
                        transform.localScale = new Vector3(-6.0f, transform.localScale.y, transform.localScale.z);
                    }
                    else
                    {
                        facingLeft = true;
                        transform.localScale = new Vector3(6.0f, transform.localScale.y, transform.localScale.z);
                    }
                    StartCoroutine(SwoopRoutine());
                }
                if (attackPrimed == 2)
                {
                    cooldown = 999;
                    animator.SetTrigger("Drop");
                    StartCoroutine(DropRoutine());
                }
                if (attackPrimed == 3)
                {
                    cooldown = 999;
                    animator.SetTrigger("Shoot");
                    if (transform.position.x < player.GetPosition().x)
                    {
                        facingLeft = false;
                        transform.localScale = new Vector3(-6.0f, transform.localScale.y, transform.localScale.z);
                    }
                    else
                    {
                        facingLeft = true;
                        transform.localScale = new Vector3(6.0f, transform.localScale.y, transform.localScale.z);
                    }
                    StartCoroutine(ShootRoutine());
                }
                attackPrimed = 0;
            }
            else
            {
                ChooseTarget();
            }
        }
        else if (!freeWill)
        {
            if (movement == new Vector3(0.0f, 0.0f, 0.0f))
            {
                inPos = true;
            }
            else
            {
                inPos = false;
            }
        }
        if (running)
        {
            RunningTarget();
        }

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
        if (swarmStun > 0)
        {
            swarmStun--;
        }

        //Add height if no Z movement
        if (movement.z == 0.0f)
        {
            height += movement.y * speed;
        }

        transform.position = transform.position + (movement * speed);
    }

    private void ChooseTarget()
    {
        float space = Random.Range(0.0f, 1.0f) <= 0.95f ? 4.5f : 0.0f;
        if (transform.position.x < player.GetPosition().x)
        {
            targetX = player.GetPosition().x + Random.Range(1.0f, 3.0f);
        }
        else
        {
            targetX = player.GetPosition().x - Random.Range(1.0f, 3.0f);
        }
        float targetY = 0.0f;
        targetY = player.GetPosition().y + Random.Range(-1.0f, 2.0f) + space;
        BindTargetY(targetY);
    }

    private void BindTargetY(float targetY)
    {
        if (targetY < limitYBottom)
        {
            targetZ = ((limitYBottom - limitYTop) * 0.01f) - 1.0f;
            targetHeight = 0.0f;
        }
        else if (targetY > limitYTop)
        {
            targetZ = -1.0f;
            targetHeight = targetY - limitYTop;
        }
        else
        {
            targetZ = ((targetY - limitYTop) * 0.01f) - 1.0f;
            targetHeight = 0.0f;
        }
    }

    private IEnumerator SwoopRoutine()
    {
        int i = 0;
        if (health > 0)
        {
            sfxController.PlaySFX2D("SHS/Bee_Swoosh", 0.8f, 15, 0.1f, false);
        }
        while (attackDistance > 0)
        {
            if (stun > 0)
            {
                break;
            }

            if (i >= 13)
            {
                float dist = 0.3f;
                if (attackDistance < dist)
                {
                    dist = attackDistance;
                }

                transform.position += new Vector3(dist * (facingLeft ? -1 : 1), -dist, 0.0f);
                height -= dist;
                if (height < 0)
                {
                    float diff = height;
                    height = 0;
                    transform.position += new Vector3(0.0f, 0.0f, diff * 0.01f);
                }
                attackDistance -= dist;
            }

            i++;
            yield return new WaitForFixedUpdate();
        }
        if (stun == 0 && animator.GetCurrentAnimatorStateInfo(0).IsName("SeabeeSwoop"))
        {
            animator.SetTrigger("Fly");
        }
        cooldown = 0;
        if (freeWill)
        {
            ChooseTarget();
        }
    }

    private IEnumerator DropRoutine()
    {
        int i = 0;
        if (!freeWill && health > 0)
        {
            float diff = (limitYTop - limitYBottom) / 2;
            float diffZ = diff * 0.01f;
            transform.position += new Vector3(0.0f, 0.0f, -diffZ);
            height += diff;
        }
        if (health > 0)
        {
            sfxController.PlaySFX2D("SHS/Bee_Swoosh", 0.8f, 15, 0.1f, false);
        }
        while (attackDistance > 0)
        {
            if (stun > 0)
            {
                break;
            }

            if (i >= 13)
            {
                float dist = 0.42f;
                if (attackDistance < dist)
                {
                    dist = attackDistance;
                }

                transform.position += new Vector3(0.0f, -dist, 0.0f);
                height -= dist;
                attackDistance -= dist;
            }

            i++;
            yield return new WaitForFixedUpdate();
        }
        if (stun == 0)
        {
            sfxController.PlaySFX2D("SHS/Stinger_Stuck_Short", 0.9f, 20, 0.1f, false);
            animator.SetTrigger("Stuck");
            stuck = true;
            if (freeWill)
            {
                ChooseTarget();
            }
            for (i = 0; i < 20; i++)
            {
                yield return new WaitForFixedUpdate();
            }
            bool loseBarb = Random.Range(0.0f, 1.0f) > (((health / 5.0f) / 2) + ((Random.Range(0.0f, 1.0f) / 2))) + (freeWill ? 0 : 999) ? true : false;
            if (loseBarb)
            {
                animator.SetTrigger("Lose Barb");
            }
            else
            {
                animator.SetTrigger("Keep Barb");
            }
        }
    }

    public void LoseBarb()
    {
        health = 1;
        stuck = false;
        cooldown = 0;
        running = true;
        // The sounds fit better for this than the original Pop audio
        sfxController.PlaySFX2D("SHS/Stinger_Fire", 0.8f, 20, 0.1f, false);
    }

    public void KeepBarb()
    {
        stuck = false;
        cooldown = 0;
    }

    private IEnumerator ShootRoutine()
    {
        for (int i = 0; i < 28; i++)
        {
            if (i > 14 && i < 18 && stun == 0)
            {
                transform.position += new Vector3(0.04f * (facingLeft ? 1 : -1), 0.0f, 0.0f);
            }
            yield return new WaitForFixedUpdate();
        }
        //Make sure cooldown is completed if not stunned before hand
        cooldown = 0;
    }

    public void ShootBarb()
    {
        running = true;
        FlyingBarbController proj = Instantiate(barb, transform.position + new Vector3(1.2f * (facingLeft ? -1 : 1), 0.0f, 0.0f), transform.rotation);
        proj.SetDirection(facingLeft ? -2 : 0);
        RunningTarget();
    }

    private void RunningTarget()
    {
        float midX = (limitXRight + limitXLeft) / 2.0f;
        float midY = (limitYTop + limitYBottom) / 2.0f;
        float targetY = 0.0f;
        if (player.GetPosition().x > midX)
        {
            targetX = limitXLeft + 1.0f;
        }
        else
        {
            targetX = limitXRight - 1.0f;
        }
        if (player.GetPosition().y > midY)
        {
            targetY = limitYBottom;
        }
        else
        {
            targetY = limitYTop;
        }
        BindTargetY(targetY);
    }

    public void SwoopHitbox()
    {
        HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(0.113f * (facingLeft ? -1 : 1) * 6.0f, -0.1073f * 6.0f, 0.0f), transform.rotation);
        hit.SetX(0.1139783f * 6.0f);
        hit.SetY(0.1064522f * 6.0f);
        //Because Swoop is variable length this needs to spawn a new hitbox for each frame
        hit.SetTtl(21);
        hit.SetDamage(2);
        hit.SetParent(transform);
    }

    public void DropHitbox()
    {
        HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(0.008f * (facingLeft ? -1 : 1) * 6.0f, -0.189f * 6.0f, 0.0f), transform.rotation);
        hit.SetX(0.1139783f * 6.0f);
        hit.SetY(0.1064522f * 6.0f);
        //Because Drop is variable length this needs to spawn a new hitbox for each frame
        hit.SetTtl(21);
        hit.SetDamage(2);
        hit.SetParent(transform);
    }

    public void Hurt(DamagePacket packet)
    {
        if (stun == 0 && swarmStun == 0 && transform.position.x > limitXLeft - 2.0f && transform.position.x < limitXRight + 2.0f)
        {
            sfxController.PlaySFX2D("General/Hit_LowPitch", 1.0f, 15, 0.15f, false);
            if (freeWill)
            {
                facingLeft = packet.getDirection();
                //Bandaid fix for letting swarm have more health
                if (health > 5)
                {
                    health = 5;
                }
            }
            transform.localScale = new Vector3((facingLeft ? 1.0f : -1.0f) * 6.0f, transform.localScale.y, transform.localScale.z);
            stun = 20;
            //Don't stick around too long
            if (!stuck && freeWill)
            {
                cooldown = 0;
            }
            int actualDamage = health;
            health -= packet.getDamage();
            if (health <= 0)
            {
                stun = 9999;
                animator.SetTrigger("Die");
                if (freeWill)
                {
                    ui.UpdateScore(100L + (running ? 0L : 300L));
                }
                StartCoroutine(DeathRoutine());
                sfxController.PlaySFX2D("SHS/Death_Bee_Short", 1.0f, 20, 0.1f, false);
            }
            else
            {
                actualDamage = actualDamage - health;
                if (!stuck && freeWill)
                {
                    animator.SetTrigger("Stun");
                    StartCoroutine(StunRoutine());
                }
                if (!freeWill)
                {
                    stun = 0;
                    swarmStun = 10;
                }
                StartCoroutine(BlinkRoutine());
            }
            if (!freeWill)
            {
                transform.parent.gameObject.SendMessage("Hurt", actualDamage);
            }
        }
    }

    private IEnumerator BlinkRoutine()
    {
        GetComponent<SpriteRenderer>().color = new Color(255.0f, 0.0f, 0.0f, 1.0f);
        for (int i = 0; i < 7; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        for (int i = 0; i < 13; i++)
        {
            float ratio = i / 13.0f;
            GetComponent<SpriteRenderer>().color = new Color(1.0f, ratio, ratio, 1.0f);
            yield return new WaitForFixedUpdate();
        }
        GetComponent<SpriteRenderer>().color = new Color(255.0f, 255.0f, 255.0f, 1.0f);
    }

    private IEnumerator StunRoutine()
    {
        for (int i = 0; i < 7; i++)
        {
            transform.position = transform.position + new Vector3((facingLeft ? 1 : -1) * 0.055f, 0, 0.0f);
            yield return new WaitForFixedUpdate();
        }
    }


    private IEnumerator DeathRoutine()
    {
        gameObject.tag = "Untagged";
        // Little extra to ensure they trigger the fall and don't appear too high up
        float toFall = height + (height > 0 ? DEFAULT_HEIGHT : 0.7f);
        int i = 0;
        // Precalculate final Y position to establish what the Z Position should be on landing
        float toFallPre = toFall;
        int k = 0;
        float yPos = transform.position.y;
        while (toFallPre > 0.0f || yPos > limitYTop - 0.7f)
        {
            yPos += ((22 - k) / 205.0f);
            toFallPre += (22 - k) / 205.0f;
            k++;
        }
        // Need to correct by 0.01f to match player's starting point and correct for the 0.7f flying level
        float zPos = ((limitYTop - yPos) * -0.01f) - 0.995f;
        transform.position = new Vector3(transform.position.x, transform.position.y, zPos);


        while (toFall > 0.0f || transform.position.y > limitYTop - 0.7f)
        {
            // If doing a safety adjustment to fall height also adjust the z axis
            transform.position = transform.position + new Vector3((facingLeft ? 1 : -1) * 0.0105f, (22 - i) / 205.0f, 0.0f);
            toFall += (22 - i) / 205.0f;
            i++;
            yield return new WaitForFixedUpdate();
        }
        animator.SetTrigger("Dead");
        for (int j = 0; j < 150; j++)
        {
            yield return new WaitForFixedUpdate();
        }

        if (freeWill)
        {
            Destroy(gameObject);
            stun = 99999;
            //Give 'em a half hour of stun so that they stay down
        }
    }

    public void SetParent(Transform parent)
    {
        transform.parent = parent;
    }

    public void SetNewTarget(float x, float z, float h)
    {
        targetX = x;
        targetZ = z;
        targetHeight = h;
    }

    public void SetNewTarget(float x, float y)
    {
        targetX = x;
        BindTargetY(y);
    }

    public void SetDirection(bool facingLeft)
    {
        this.facingLeft = facingLeft;
    }

    public void SetCooldown(int cooldown)
    {
        this.cooldown = cooldown;
    }

    public bool IsInPosition()
    {
        return health <= 0 || (inPos && cooldown == 0 && swarmStun == 0);
    }

    public void SetAttackDistance(float attack)
    {
        attackDistance = attack;
    }

    public void TriggerSwoop()
    {
        cooldown = 999;
        animator.SetTrigger("Swoop");
        StartCoroutine(SwoopRoutine());
    }

    public void TriggerDrop()
    {
        cooldown = 999;
        animator.SetTrigger("Drop");
        StartCoroutine(DropRoutine());
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public void SpawnRandom()
    {
        // Does nothing
        targetHeight += Random.value * 3.0f;
    }

    public void BuzzSound()
    {
        // Once again the original sound didn't work out at all lol. I understand TCRF so much now
        if (!player.IsDead())
        {
            sfxController.PlaySFX2D("STM/Flap_Boosted", 0.9f, 30, 0.1f, false);
        }
    }
}
