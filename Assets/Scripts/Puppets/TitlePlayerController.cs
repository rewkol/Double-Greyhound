using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitlePlayerController : MonoBehaviour
{
    public float speed;

    private bool facingLeft;
    private bool walking;
    private bool inPlace;

    private Transform transform;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        facingLeft = false;
        walking = false;
        inPlace = false;
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (walking && !inPlace)
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

    public void SetInPlace(bool inPlace)
    {
        this.inPlace = inPlace;
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

    public float GetSpeed()
    {
        return this.speed;
    }

    public void Attack()
    {
        animator.SetTrigger("Punch");
    }

    public void Damaged()
    {
        this.walking = false;
        animator.SetTrigger("Stun");
        StartCoroutine(BlinkRoutine());
        StartCoroutine(StunRoutine());
    }

    public void Death()
    {
        this.walking = false;
        animator.SetTrigger("Knockback");
        StartCoroutine(KnockbackRoutine());
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
        GetComponent<SpriteRenderer>().color = new Color(255.0f, 0.0f, 0.0f, 1.0f);
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        GetComponent<SpriteRenderer>().color = new Color(255.0f, 255.0f, 255.0f, 1.0f);
    }

    private IEnumerator KnockbackRoutine()
    {
        for (int i = 0; i < 49; i++)
        {
            transform.position += new Vector3(0.03f * (facingLeft ? 1 : -1), 0.01f * ((48 - i) - 24), 0.0f);
            yield return new WaitForFixedUpdate();
        }

        animator.SetTrigger("Downed");
        transform.position += new Vector3(0.0f, -0.85f, -0.0085f);
    }

    public void Reset()
    {
        animator.SetTrigger("Stand");
        transform.position = new Vector3(-16.13f, -2.27f, -3.0f);
    }

    public void Punch()
    {
        // do nothing
    }

    public void ChangeHurtbox()
    {
        // do nothing
    }

    public void SetStunnable()
    {
        // do nothing
    }
}
