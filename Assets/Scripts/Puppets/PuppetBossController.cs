using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppetBossController : MonoBehaviour, IPuppet
{
    public bool landTravel;
    public Material lerpMaterial;

    private Transform transform;
    private Animator animator;
    private SpriteRenderer renderer;
    private int state;
    private int timer;

    private PuppetMasterController parent;

    private SFXController sfxController;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        renderer = GetComponent<SpriteRenderer>();
        state = 0;
        timer = Random.Range(50,450);

        sfxController = GameObject.FindObjectOfType<SFXController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (state == 1)
        {
            if (timer > 0)
            {
                timer--;
            }
            else
            {
                timer = Random.Range(50, 450);
                StartCoroutine(EmoteRoutine());
            }
        }
    }

    void Move(int direction)
    {
        if (landTravel)
        {
            transform.position += new Vector3(1.38f * direction, 0.0f, 0.0f);
        }
    }

    private IEnumerator EmoteRoutine()
    {
        state = 0;
        animator.SetTrigger("Emote");
        for (int i = 0; i < 75; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        state = 1;
    }

    public bool Enter()
    {
        if (state != 0)
        {
            return false;
        }
        StartCoroutine(EntranceRoutine());
        return true;
    }

    public bool Exit()
    {
        if (state != 1)
        {
            return false;
        }
        StartCoroutine(ExitRoutine());
        return true;
    }

    public bool HasEntered()
    {
        return transform.position.x > 58.8f;
    }

    public bool HasExited()
    {
        return transform.position.x < 50.2f;
    }

    private IEnumerator EntranceRoutine()
    {
        state = 2;
        transform.position += new Vector3(0.0f, -10.0f, 0.0f);
        while(transform.position.x < 59.0f)
        {
            if (landTravel)
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("PChiefForward"))
                {
                    animator.SetTrigger("Forward");

                }
            }
            else
            {
                transform.position += new Vector3(0.1f, 0.0f, 0.0f);
            }
            yield return new WaitForFixedUpdate();
        }
        state = 1;
    }

    private IEnumerator ExitRoutine()
    {
        state = 2;
        while (transform.position.x > 50.0f)
        {
            if (landTravel)
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("PChiefBackward"))
                {
                    animator.SetTrigger("Backward");
                }
            }
            else
            {
                transform.position += new Vector3(-0.1f, 0.0f, 0.0f);
            }
            yield return new WaitForFixedUpdate();
        }
        transform.position += new Vector3(0.0f, 10.0f, 0.0f);
        state = 0;
    }

    private IEnumerator DisappearRoutine()
    {
        state = 0;
        renderer.material = lerpMaterial;
        float amount = 0.2f;
        sfxController.PlaySFX2D("SJHS/Blip", 0.2f, 128, 0.2f, true);
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
        // The bosses cannot die!
    }

    public void StartBattle()
    {
        // The bosses don't need to wait to start
    }

    public void Disappear()
    {
        StartCoroutine(DisappearRoutine());
    }

    public void Stomp()
    {
        if (state >= 1)
        {
            sfxController.PlaySFX2D("HVHS/Stomp", 0.3f, 190, 0.0f, true);
        }
    }

    public void FlapWings()
    {
        if (state >= 1)
        {
            sfxController.PlaySFX2D("STM/Flap_Boosted", 0.6f, 190, 0.0f, true);
        }
    }
}
