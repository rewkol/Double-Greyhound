using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OphanimController : MonoBehaviour
{
    public HitboxController hitbox;

    private Transform transform;
    private Animator animatorTop;
    private Animator animatorBottom;
    private PlayerController player;
    private bool facingLeft;
    private bool hit;
    private bool open;
    private int frameCounter;
    private int chaseTimer;
    private int state;

    private float speed;

    private SFXController sfxController;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        animatorTop = transform.Find("OphanimTop").GetComponent<Animator>();
        animatorBottom = transform.Find("OphanimBottom").GetComponent<Animator>();
        player = GameObject.FindObjectsOfType<PlayerController>()[0];
        facingLeft = true;
        hit = false;
        open = false;
        // Annoyingly the animations are based on a cycle and breaking it could look weird so need to only change animations when animation one frame from completion
        // So that the execution logic does not try to act upon a new animation before the old one has finished playing through
        frameCounter = 0;
        chaseTimer = Random.Range(50, 150);
        // States: 0 initial wait, 1 fly toward, 2 encircle, 3 fly away
        state = 0;
        speed = 0.19f;

        sfxController = GameObject.FindObjectOfType<SFXController>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        frameCounter++;

        // Safety so frameCounter never overflows (should never be an issue since this is 300+ hours of gametime)
        if (frameCounter > 999999)
        {
            Destroy(gameObject);
        }

        // Wow this could be a switch couldn't it? No that would be ugly with this much code
        if (state == 0)
        {
            if (chaseTimer > 0)
            {
                chaseTimer--;
                if (chaseTimer == 0 || transform.position.x - player.GetPosition().x < 11.0f)
                {
                    StartCoroutine(OpenRoutine());
                    sfxController.PlaySFX2D("STM/Wind_Rushing_Trimmed2", 0.3f, 20, 0.05f, false);
                }
            }
        }
        else if (state == 1 || state == 2)
        {

            // Rotate only if opened. Otherwise it'd be weird
            if (player.GetPosition().x > transform.position.x && state == 1)
            {
                facingLeft = false;
                transform.localScale = new Vector3(-1.0f, transform.localScale.y, transform.localScale.z);
            }
            else if (player.GetPosition().x <= transform.position.x && state == 1)
            {
                facingLeft = true;
                transform.localScale = new Vector3(1.0f, transform.localScale.y, transform.localScale.z);
            }

            //Movement code
            float moveHorizontal = player.GetPosition().x - transform.position.x;
            float moveVertical = player.GetPosition().y - transform.position.y;
            float moveZ = (player.GetPosition().z - transform.position.z) / speed;

            if (Mathf.Abs(moveHorizontal) < speed * 6 && Mathf.Abs(moveVertical) < speed * 6 && state == 1)
            {
                state++;
                StartCoroutine(CloseRoutine());
                StartCoroutine(HealRoutine());
            }

            if (moveHorizontal != 0)
            {
                moveHorizontal = moveHorizontal / Mathf.Abs(moveHorizontal);
            }
            if (moveVertical != 0)
            {
                moveVertical = moveVertical / Mathf.Abs(moveVertical);
                if (moveVertical > 0 && open)
                {
                    animatorTop.SetTrigger("FlyUp");
                    animatorBottom.SetTrigger("FlyUp");
                }
                else if (moveVertical <= 0 && open)
                {
                    animatorTop.SetTrigger("FlyDown");
                    animatorBottom.SetTrigger("FlyDown");
                }
            }
            else if (moveVertical == 0 && open)
            {
                animatorTop.SetTrigger("FlyLevel");
                animatorBottom.SetTrigger("FlyLevel");
            }

            if (Mathf.Abs(player.GetPosition().x - transform.position.x) < speed)
            {
                moveHorizontal = (player.GetPosition().x - transform.position.x) / speed;
            }
            if ( Mathf.Abs(player.GetPosition().y - transform.position.y) < speed)
            {
                moveVertical = (player.GetPosition().y- transform.position.y) / speed;
            }

            Vector3 movement = new Vector3(moveHorizontal, moveVertical, moveZ);

            transform.position = transform.position + (movement * speed);
        } 
        if (state == 3)
        {
            animatorTop.SetTrigger("FlyUp");
            animatorBottom.SetTrigger("FlyUp");
            facingLeft = true;
            transform.localScale = new Vector3(1.0f, transform.localScale.y, transform.localScale.z);
            transform.position = transform.position + (new Vector3(-1.0f, 1.0f, 0.0f) * speed);

            chaseTimer++;
            if (chaseTimer >= 30)
            {
                gameObject.tag = "Untagged";
            }
            if (chaseTimer == 300)
            {
                Destroy(gameObject);
            }
        }



        // Blinks
        if (state % 2 == 0)
        {
            if ((frameCounter + 1) % 180 == 0)
            {
                int rand = Random.Range(0, 3);
                switch (rand)
                {
                    case 0:
                    {
                        animatorTop.SetTrigger("BlinkLeft");
                        break;
                    }
                    case 1:
                    {
                        animatorTop.SetTrigger("BlinkRight");
                        break;
                    }
                    case 2:
                    {
                        animatorTop.SetTrigger("BlinkCentre");
                        break;
                    }
                    default:
                    {
                        animatorTop.SetTrigger("BlinkLeft");
                        break;
                    }
                }
            }
        }
    }

    public void Hurt(DamagePacket packet)
    {
        if (state < 3)
        {
            sfxController.PlaySFX2D("General/Hit_LowPitch", 1.0f, 15, 0.15f, false);
            chaseTimer = 0;
            state = 3;
            StartCoroutine(OpenRoutine());
            sfxController.PlaySFX2D("STM/Wind_Rushing_Trimmed2", 0.3f, 20, 0.05f, false);
            sfxController.PlaySFX2D("STM/Death_Ophanim_modified", 0.7f, 20, 0.1f, false);

            StartCoroutine(StunRoutine());
            StartCoroutine(BlinkRoutine());
            GameObject.FindObjectsOfType<UIController>()[0].UpdateScore(2500L);
        }
    }

    private IEnumerator OpenRoutine()
    {
        animatorTop.ResetTrigger("Close");
        animatorBottom.ResetTrigger("Close");

        animatorTop.SetTrigger("Open");
        animatorBottom.SetTrigger("Open");
        for (int i = 0; i < 18; i++)
        {
            if (i == 11 && (state == 0 || state == 2))
            {
                state++;
            }
            yield return new WaitForFixedUpdate();
        }
        open = true;
    }

    private IEnumerator CloseRoutine()
    {
        // Clear flying triggers
        animatorTop.ResetTrigger("FlyLevel");
        animatorBottom.ResetTrigger("FlyLevel");
        animatorTop.ResetTrigger("FlyUp");
        animatorBottom.ResetTrigger("FlyUp");
        animatorTop.ResetTrigger("FlyDown");
        animatorBottom.ResetTrigger("FlyDown");

        animatorTop.ResetTrigger("Open");
        animatorBottom.ResetTrigger("Open");

        animatorTop.SetTrigger("Close");
        animatorBottom.SetTrigger("Close");
        for (int i = 0; i < 18; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        open = false;
    }

    private IEnumerator HealRoutine()
    { 
        for (int i = 0; i < 300; i++)
        {
            // Get out if player sucks
            if (state == 3)
            {
                break;
            }
            if (i % 75 == 0)
            {
                sfxController.PlaySFX2D("STM/Spin_modified", 0.6f, 10, 0.1f, false);
            }
            if ((i + 1) % 75 == 0)
            {
                HitboxController hit = Instantiate(hitbox, transform.position, transform.rotation);
                hit.SetX(3.0f);
                hit.SetY(3.0f);
                hit.SetTtl(100);
                hit.SetDamage(-1);
                hit.SetParent(transform);
            }
            yield return new WaitForFixedUpdate();
            if (i == 299)
            {
                chaseTimer = 0;
                StartCoroutine(OpenRoutine());
                sfxController.PlaySFX2D("STM/Wind_Rushing_Trimmed2", 0.3f, 20, 0.05f, false);
            }
        }
    }

    private IEnumerator StunRoutine()
    {
        for (int i = 0; i < 5; i++)
        {
            transform.position += new Vector3(-0.05f * (facingLeft ? -1 : 1), 0.0f, 0.0f);
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator BlinkRoutine()
    {
        transform.Find("OphanimTop").GetComponent<SpriteRenderer>().color = new Color(255.0f, 0.0f, 0.0f, 1.0f);
        transform.Find("OphanimBottom").GetComponent<SpriteRenderer>().color = new Color(255.0f, 0.0f, 0.0f, 1.0f);
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        transform.Find("OphanimTop").GetComponent<SpriteRenderer>().color = new Color(255.0f, 255.0f, 255.0f, 1.0f);
        transform.Find("OphanimBottom").GetComponent<SpriteRenderer>().color = new Color(255.0f, 255.0f, 255.0f, 1.0f);
    }

    public void SpawnRandom()
    {
        transform.position = new Vector3(transform.position.x, player.GetPosition().y + Random.Range(-6.0f, 6.0f), -1.0f);
    }
}
