using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleVikingController : MonoBehaviour
{
    public float speed;

    private bool facingLeft;
    private bool walking;

    private Transform transform;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        facingLeft = false;
        walking = false;
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (walking)
        {
            transform.position += new Vector3((facingLeft ? -1 : 1) * speed, 0.0f, 0.0f);
        }
    }

    public void StartWalk()
    {
        animator.SetTrigger("Walk");
        this.walking = true;
    }

    public void StopWalk()
    {
        this.walking = false;
        animator.SetTrigger("Idle");
    }

    public void NudgeUp()
    {
        transform.position += new Vector3(0.0f, speed, 0.01f * speed);
    }

    public void NudgeDown()
    {
        transform.position += new Vector3(0.0f, -speed, -0.01f * speed);
    }

    public void FaceLeft()
    {
        transform.localScale = new Vector3(-6.0f, transform.localScale.y, transform.localScale.z);
        this.facingLeft = true;
    }

    public void FaceRight()
    {
        transform.localScale = new Vector3(6.0f, transform.localScale.y, transform.localScale.z);
        this.facingLeft = false;
    }

    public void Attack()
    {
        animator.SetTrigger("Swing");
    }

    public void Damaged()
    {
        this.walking = false;
        animator.SetTrigger("Stun");
        StartCoroutine(BlinkRoutine());
        StartCoroutine(StunRoutine());
    }

    public void Death(bool vikingSide)
    {
        this.walking = false;
        animator.SetTrigger("Knockback");
        StartCoroutine(DeathRoutine(vikingSide));
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

    private IEnumerator DeathRoutine(bool vikingSide)
    {
        for (int i = 0; i < 201; i++)
        {
            if (i < 50)
            {
                transform.position = transform.position + new Vector3((facingLeft ? 1 : -1) * 0.0105f, (22 - i) / 205.0f, 0.0f);
            }
            if (i == 200)
            {
                if (vikingSide)
                {
                    transform.position = new Vector3(-11.6f, -2.27f, -2.99f);
                }
                else
                {
                    transform.position = new Vector3(12.19f, -2.27f, -2.99f);
                }
                // lol, this was a completely unused/removed feature put in 3 years ago that I get to finally take advantage of
                animator.SetTrigger("Stand");
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
