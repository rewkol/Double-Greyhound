using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //Public Objects
    public HitboxController hitbox;
    public HurtboxController hurtbox;

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

    //Timers/Flags
    private int cooldown;
    private bool punchPushed;
    private int specialCooldown;
    private bool specialPushed;
    private bool specialChangePushed;
    private int stun;

    //Character information
    private int health;
    private int special;
    private float comboCounter;
    private List<FlightInstruction> flightProgram;
    private List<HitboxController> activeHitboxes;

    // Start is called before the first frame update
    void Start()
    {
        //Get components
        ui = GameObject.FindObjectsOfType<UIController>()[0];
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        transform.position = new Vector3(transform.position.x, limitYTop, -1.0f);

        //Initialize all the flags, timers, etc.
        facingLeft = false;
        downed = false;
        special = 0;
        cooldown = 0;
        punchPushed = false;
        comboCounter = 0.0f;
        //Special cooldown is so that specials with projectiles (axes/halos) can't be spammed. Only allowing one on screen by the player at a time
        specialCooldown = 0;
        specialPushed = false;
        specialChangePushed = false;
        stun = 0;
        health = 20;
        flightProgram = new List<FlightInstruction>();
        activeHitboxes = new List<HitboxController>();


        //Get camera position to limit x movement
        limitXLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        limitXRight = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
    }

    void FixedUpdate()
    {
        if(!ui.GameActive())
        {
            animator.SetTrigger("Idle");
            return;
        }

        if (flightProgram.Count > 0)
        {
            //If Program Queue has data, perform those actions instead of accepting User Input
            FlightInstruction currentInstruction = flightProgram[0];

            for(int i = 0; i < currentInstruction.GetCount(); i++)
            {
                HandleInstruction(currentInstruction, i);
            }

            flightProgram.RemoveAt(0);
        }
        else
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
        if (stun > 0)
        {
            stun--;
            movement = new Vector3(0.0f, 0.0f, 0.0f);
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
        if ((moveHorizontal != 0.0f || moveVertical != 0.0f) && animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerDowned"))
        {
            animator.SetTrigger("Stand");
            movement = new Vector3(0.0f, 0.0f, 0.0f);
            flightProgram.AddRange(PlayerInstructionSets.GetStandingInstructions());
        }

        transform.position = transform.position + (movement * speed);
    }

    private void PerformActions()
    {
        //TODO: Press button to choose selected Special
        if (Input.GetAxis("Jump") > 0 && specialChangePushed == false)
        {
            special = (special + 1) % 4;
            specialChangePushed = true;
        }
        if (Input.GetAxis("Jump") == 0)
        {
            specialChangePushed = false;
        }

        //Attack code
        if (cooldown == 0 && stun == 0 && !downed && !punchPushed && Input.GetAxis("Fire1") > 0)
        {
            Punch();
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
        cooldown = 14;
        animator.SetTrigger("Punch");
        CreateHitbox(new Vector3(1.1f, 0.63f, 0.0f), 1.2f, 0.8f, 200, 1);
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
        //TODO: Put the actual specials implementation here

        //Testing out Flight Instructions
        if (flightProgram.Count == 0)
        {//Activate specials
            animator.SetTrigger("Jump");
            cooldown = 1;
            flightProgram.AddRange(PlayerInstructionSets.GetJumpInstructions(facingLeft));
        }
    }

    public void Hurt(DamagePacket packet)
    {
        if (stun == 0)
        {
            int damage = packet.getDamage();
            facingLeft = packet.getDirection();
            transform.localScale = new Vector3((facingLeft ? -1.0f : 1.0f) * 6.0f, transform.localScale.y, transform.localScale.z);

            //Clear the Flight Program so player doesn't continue previous action
            flightProgram.Clear();

            //Destroy all active hitboxes
            foreach(HitboxController hitbox in activeHitboxes)
            {
                if (hitbox != null)
                {
                    hitbox.Kill();
                }
            }
            activeHitboxes.Clear();

            comboCounter += damage;
            if (comboCounter >= 3.0f)
            {
                animator.SetTrigger("Knockback");
                flightProgram.AddRange(PlayerInstructionSets.GetKnockbackInstructions(facingLeft));
                cooldown = 1;
                comboCounter = 0.0f;
            }
            else
            {
                animator.SetTrigger("Stun");
                stun = 15;
                flightProgram.AddRange(PlayerInstructionSets.GetStunInstructions(facingLeft));
            }
            //Clear cooldown so user can act immediately out of stun in case they used a move with huge cooldown before being stunned
            cooldown = 0;
            health -= damage;
            if (health <= 0)
            {
                //TODO: This should tell UI to get you to enter a high score and then restart from HVHS
                animator.SetTrigger("Knockback");
                downed = true;
            }
            else 
            {
                StartCoroutine(BlinkRoutine());
            }
            ui.PlayerHealthBar(health);
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

    public void HandleInstruction(FlightInstruction instruction, int index)
    {
        string command = instruction.GetCommand(index);
        switch (command)
        {
            case "Move":
            {
                // Move player by the vector
                transform.position = transform.position + instruction.GetVector(index);

                //Bounds check the X axis movement
                if (transform.position.x > limitXRight)
                {
                    //Stopping the player exactly at the boundary causes an animation error so add the 0.0001f as an impossible to see spacer
                    transform.position = transform.position + new Vector3(limitXRight - transform.position.x - 0.0001f, 0.0f, 0.0f);
                }
                else if (transform.position.x < limitXLeft)
                {
                    transform.position = transform.position + new Vector3(limitXLeft - transform.position.x + 0.0001f, 0.0f, 0.0f);
                }
                break;
            }
            case "Hurtbox":
            {
                // Resize hurtbox using X and Y. Move hurtbox by Vector
                hurtbox.UpdateScale(instruction.GetX(index), instruction.GetY(index));
                hurtbox.Move(instruction.GetVector(index));
                break;
            }
            case "Hitbox":
            {
                // Create a new hitbox at Vector, with size X and Y, and TTL of Time
                Vector3 vector = instruction.GetVector(index);
                float x = instruction.GetX(index);
                float y = instruction.GetY(index);
                int ttl = instruction.GetTime(index);

                CreateHitbox(vector, x, y, ttl, 1);
                break;
            }
            case "Hitbox2":
            {
                // Same as above but more damage
                Vector3 vector = instruction.GetVector(index);
                float x = instruction.GetX(index);
                float y = instruction.GetY(index);
                int ttl = instruction.GetTime(index);

                CreateHitbox(vector, x, y, ttl, 2);
                break;
            }
            case "Ignore":
            {
                downed = true;
                break;
            }
            case "PayAttention":
            {
                downed = false;
                break;
            }
            case "Wait":
            {
                //Do nothing this frame
                break;
            }
            default:
            {
                //Default to sending the command as a function of this object
                gameObject.SendMessageUpwards(command);
                break;
            }
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
}
