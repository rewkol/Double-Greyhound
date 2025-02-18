using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppetSaintController : MonoBehaviour, IPuppet
{
    public Material lerpMaterial;

    private Transform transform;
    private Animator animator;
    private SpriteRenderer renderer;
    private int state;
    private int timer;
    private int health;
    private float xBase;
    private bool start;

    private PuppetMasterController parent;

    void Start()
    {
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        renderer = GetComponent<SpriteRenderer>();
        state = 0;
        timer = 0;
        health = 3;
        xBase = 0.0f;

        StartCoroutine(EntranceRoutine());
    }

    void FixedUpdate()
    {
        if (!start)
        {
            return;
        }

        if (timer > 0)
        {
            timer--;
        }

        if (state == 1 && timer == 0)
        {
            // More likely to move if back, more likely to attack if forward
            float distance = transform.position.x;
            float chance = Random.Range(xBase - 1.0f, xBase + 9.0f);
            if (chance + 2 < distance)
            {
                if (Random.value < (distance - xBase) / 12.0f)
                {
                    StartCoroutine(HurtRoutine());
                }
                else
                {
                    StartCoroutine(AttackRoutine());
                }
            }
            else
            {
                if (chance < distance)
                {
                    StartCoroutine(MoveBackRoutine());
                }
                else
                {
                    StartCoroutine(MoveForwardRoutine());
                }
            }
        }

        if (timer == 0)
        {
            timer = Random.Range(50, 100);
        }
    }

    private IEnumerator EntranceRoutine()
    {
        int steps = 100 + Random.Range(0, 10);
        animator.SetTrigger("Moving");
        for (int i = 0; i < steps; i++)
        {
            transform.position += new Vector3(0.08f, 0.0f, 0.0f);
            yield return new WaitForFixedUpdate();
        }
        animator.SetTrigger("Idle");
        state = 1;
        timer = Random.Range(20, 50);
        xBase = transform.position.x;
    }

    private IEnumerator MoveForwardRoutine()
    {
        state = 0;
        int steps = Random.Range(10, 40);
        animator.SetTrigger("Moving");
        for (int i = 0; i < steps; i++)
        {
            transform.position += new Vector3(0.08f, 0.0f, 0.0f);
            yield return new WaitForFixedUpdate();
        }
        animator.SetTrigger("Idle");
        state = 1;
    }

    private IEnumerator MoveBackRoutine()
    {
        state = 0;
        int steps = Random.Range(10, 40);
        animator.SetTrigger("Moving");
        for (int i = 0; i < steps; i++)
        {
            transform.position += new Vector3(-0.08f, 0.0f, 0.0f);
            yield return new WaitForFixedUpdate();
        }
        animator.SetTrigger("Idle");
        state = 1;
    }

    private IEnumerator AttackRoutine()
    {
        state = 0;
        animator.SetTrigger("Attack");
        for (int i = 0; i < 24; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        state = 1;
    }

    private IEnumerator HurtRoutine()
    {
        state = 0;
        health--;
        if (health > 0)
        {
            animator.SetTrigger("Hurt");
            for (int i = 0; i < 7; i++)
            {
                transform.position += new Vector3(-0.055f, 0.0f, 0.0f);
                yield return new WaitForFixedUpdate();
            }
            state = 1;
        }
        else
        {
            StartCoroutine(DieRoutine());
        }
    }

    private IEnumerator DieRoutine()
    {
        timer = -99;
        animator.SetTrigger("Die");
        for (int i = 0; i < 70; i++)
        {
            if (i < 50)
            {
                transform.position = transform.position + new Vector3(-0.0105f, (22 - i) / 205.0f, 0.0f);
            }
            yield return new WaitForFixedUpdate();
        }
        this.parent.RemoveAlly(this);
        Destroy(gameObject);
    }

    private IEnumerator DisappearRoutine()
    {
        state = 0;
        renderer.material = lerpMaterial;
        float amount = 0.2f;
        for (int i = 1; i <= 3; i++)
        {
            renderer.material.SetFloat("_LerpAmount", amount);
            amount += amount + (i / 10.0f);
            yield return new WaitForFixedUpdate();
        }
        Destroy(gameObject);
        // Don't care about telling puppet master because this means that phase is already over anyway
        // It will clean up its own messes
    }

    public void SetParent(PuppetMasterController master)
    {
        this.parent = master;
    }

    public void Die()
    {
        StartCoroutine(DieRoutine());
    }

    public void Disappear()
    {
        StartCoroutine(DisappearRoutine());
    }

    public void StartBattle()
    {
        this.start = true;
    }
}
