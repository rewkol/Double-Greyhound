using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MitchellController : MonoBehaviour
{
    public FalseImageController image;
    public RingAuraController ring;
    public EnemyAxeController axe;
    public Material lerpMaterial;

    private UIController ui;
    private float limitXLeft;
    private float limitXRight;
    private FloorBreakController destroyer;

    private Transform transform;
    private Animator animator;
    private SpriteRenderer renderer;
    private PlayerController player;
    private int health;
    private int state;
    private bool hasHurt;
    private bool facingLeft;

    private GameObject musicController;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        renderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindObjectsOfType<PlayerController>()[0];

        ui = GameObject.FindObjectsOfType<UIController>()[0];
        musicController = GameObject.Find("MusicController");
        destroyer = GameObject.FindObjectsOfType<FloorBreakController>()[0];

        health = 9999;
        state = 0;
        facingLeft = false;
        hasHurt = false;

        limitXLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x + 1.5f;
        limitXRight = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x - 1.5f;

        StartCoroutine(EntranceRoutine());

        if (musicController != null)
        {
            musicController.SendMessage("StartNextSong");
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (state == 0)
        {
            return;
        }

        // Before player can punch, go to other side and increment state
        if (Mathf.Abs(transform.position.x - player.GetPosition().x) < 2.5f)
        {
            state++;
            StartCoroutine(TurnRoutine());
        }
    }

    private IEnumerator EntranceRoutine()
    {
        yield return new WaitForFixedUpdate();
        ui.StartManualCutscene();
        float playerX = player.GetPosition().x;
        float midpointX = (limitXRight + limitXLeft) / 2.0f;
        float startingX = transform.position.x;
        if (playerX < midpointX)
        {
            transform.position += new Vector3(limitXRight - startingX, 0.0f, 0.0f);
            facingLeft = true;
            transform.localScale = new Vector3(-6.0f, transform.localScale.y, transform.localScale.z);

        }
        else
        {
            transform.position += new Vector3(limitXLeft - startingX, 0.0f, 0.0f);
            facingLeft = false;
            transform.localScale = new Vector3(6.0f, transform.localScale.y, transform.localScale.z);
        }
        // Float down
        while (transform.position.y > -1.9f)
        {
            transform.position += new Vector3(0.0f, -0.04f, 0.0f);
            yield return new WaitForFixedUpdate();
        }
        animator.SetTrigger("Land");
        for (int i = 0; i < 26; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForFixedUpdate();
        ui.EndManualCutscene();
        ui.DisplayDialogue("MitchellHeadshot", "First, let me just say one thing:|Thank you for playing my game!|However, I do not want you to win.|I stand before you as I did|on my graduation day|so that I can represent|what you will never achieve.|Captain Greyhound, you do not have|what it takes to move beyond this place.|Let me prove it to you!");
        ui.BossEntrance(health, "MITCHELL");
        while (!ui.GameActive())
        {
            yield return new WaitForFixedUpdate();
        }
        state = 1;
    }

    private IEnumerator TurnRoutine()
    {
        int prevState = state;
        state = 0;
        animator.SetTrigger("Turn");
        bool direction = facingLeft;
        float dist = (limitXRight - limitXLeft) / 13.0f;
        for (int i = 0; i < 13; i++)
        {
            transform.position += new Vector3(dist * (direction ? -1.0f : 1.0f), 0.0f, 0.0f);
            yield return new WaitForFixedUpdate();
        }
        state = prevState;

        if (state == 4)
        {
            ui.DisplayDialogue("MitchellHeadshot", "This is fun, but I want more!|How about a ring slalom?");
            while (!ui.GameActive())
            {
                yield return new WaitForFixedUpdate();
            }
            animator.SetTrigger("Snap");
            StartCoroutine(RingRoutine());
        }
        else if (state == 5)
        {
            ui.DisplayDialogue("MitchellHeadshot", "Again!");
            while (!ui.GameActive())
            {
                yield return new WaitForFixedUpdate();
            }
            animator.SetTrigger("Snap");
            StartCoroutine(RingRoutine());
        }
        else if (state == 6)
        {
            ui.DisplayDialogue("MitchellHeadshot", "Still not right is it...|I could copy a famous boss.|Let's go back to 1985!");
            while (!ui.GameActive())
            {
                yield return new WaitForFixedUpdate();
            }
            animator.SetTrigger("Snap");
            StartCoroutine(AxeRoutine());
        }
        else if (state == 7)
        {
            ui.DisplayDialogue("MitchellHeadshot", "No, no, no!|It's all wrong, isn't it?|This isn't what you wanted|out of the final battle:|some nerd in a suit dancing around?|Where's the spectacle?|...|...");
            while (!ui.GameActive())
            {
                yield return new WaitForFixedUpdate();
            }
            animator.SetTrigger("Snap");
            ui.DisplayDialogue("MitchellHeadshot", "I've got it!|One last challenge for you|you who have come so far.|I will fulfill your desires.|I will give you the battle|you are longing for!|Join me, in one last fight!");
            while (!ui.GameActive())
            {
                yield return new WaitForFixedUpdate();
            }
            animator.SetTrigger("Jump");
            StartCoroutine(DeathRoutine());
        }
    }

    public void Turn()
    {
        facingLeft = !facingLeft;
        if (facingLeft)
        {
            transform.localScale = new Vector3(-6.0f, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(6.0f, transform.localScale.y, transform.localScale.z);
        }
    }

    /**
     * Creates a false image at the current location of the current sprite
     */
    public void CreateImage()
    {
        FalseImageController imageInstance = Instantiate(image, transform.position + new Vector3(0.0f, 0.0f, 0.0001f), transform.rotation);
        imageInstance.SetImage(renderer.sprite, facingLeft);
    }

    public void Hurt(DamagePacket packet)
    {
        if (state == 0)
        {
            return;
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("MitchellIdle"))
        {
            animator.SetTrigger("Snap");
        }
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        int prevState = state;
        state = 0;
        for (int i = 0; i < 33; i++)
        {
            if (i < 5)
            {
                renderer.color = new Color(1.0f, 1.0f, 1.0f, 0.82f - (0.18f * i));
            }
            else if (i > 27)
            {
                renderer.color = new Color(1.0f, 1.0f, 1.0f, 0.1f + (0.18f * (i - 27)));
            }
            yield return new WaitForFixedUpdate();
        }
        // Just to be safe
        renderer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        if (!hasHurt)
        {
            ui.DisplayDialogue("MitchellHeadshot", "How impersonal, fighting from afar.|This is supposed to be fun.|Come closer, let's dance!");
            while (!ui.GameActive())
            {
                yield return new WaitForFixedUpdate();
            }
            hasHurt = true;
        }
        state = prevState;
    }

    private IEnumerator AxeRoutine()
    {
        while (ui.GameActive())
        {
            // Spawn axes like Bowser
            for (int i = 0; i < 100; i++)
            {
                if (state == 6)
                {
                    if (i < 80 && i % 20 == 0)
                    {
                        ThrowAxe();
                    }
                    if (i < 80 && Random.value < 0.5f && (i + 10) % 20 == 0)
                    {
                        ThrowAxe();
                    }
                }
                else
                {
                    yield break;
                }
                yield return new WaitForFixedUpdate();
            }
        }
    }

    private IEnumerator RingRoutine()
    {
        bool up = Random.value > 0.5f;
        while (ui.GameActive())
        {
            for (int i = 0; i < 40; i++)
            {
                if (state == 4 || state == 5)
                {
                    if (i == 0)
                    {
                        SpawnRing(up);
                        up = !up;
                    }
                }
                else
                {
                    yield break;
                }
                yield return new WaitForFixedUpdate();
            }
        }
    }

    private void SpawnRing(bool up)
    {
        float y = up ? -0.95f : -3.5f;
        RingAuraController instance = Instantiate(ring, new Vector3(transform.position.x + (facingLeft ? -1.0f : 1.0f), y, -1.0f), transform.rotation);
        instance.SetDirection(facingLeft);
        instance.LimitAir();
    }

    public void ThrowAxe()
    {
        EnemyAxeController thrownAxe = Instantiate(axe, transform.position + new Vector3(0.1f * (facingLeft ? -1 : 1), 1.65f, 0.0f), transform.rotation);
        thrownAxe.SetDirection(facingLeft);
    }

    private IEnumerator DeathRoutine()
    {
        state = 0;
        ui.StartManualCutscene();
        if (musicController != null)
        {
            musicController.SendMessage("StopCurrentSong");
        }

        renderer.material = lerpMaterial;
        // Fade to white
        float amount = 0.1f;
        for (int i = 0; i < 10; i++)
        {
            renderer.material.SetFloat("_LerpAmount", amount);
            amount += 0.1f;
            transform.position += new Vector3(0.0f, 0.03f, 0.0f);
            yield return new WaitForFixedUpdate();
        }

        float midpointX = (limitXRight + limitXLeft) / 2.0f;
        while (Mathf.Abs(midpointX - transform.position.x) > 1.0f)
        {
            transform.position += new Vector3(facingLeft ? -0.1f : 0.1f, 0.0f, 0.0f);
            if (transform.position.y < 3.0f)
            {
                transform.position += new Vector3(0.0f, 0.03f, 0.0f);
            }
            yield return new WaitForFixedUpdate();
        }

        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        animator.SetTrigger("Morph");
        // Bump him up because sprite change moves him down a smidge
        transform.position += new Vector3((facingLeft ? 1 : -1) * 0.06f, 0.12f, 0.0f);
        for (int i = 0; i < 150; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        for (int i = 0; i < 90; i++)
        {
            transform.position += new Vector3(0.0f, (i < 20 ? (i / 20.0f): 1.0f) * 0.2f, 0.0f);
            yield return new WaitForFixedUpdate();
        }

        for (int i = 0; i < 15; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        transform.localScale = new Vector3(transform.localScale.x * 2, transform.localScale.y * 2, 1.0f);
        for (int i = 0; i < 40; i++)
        {
            transform.position += new Vector3(0.0f, -1.75f, 0.0f);
            if (i == 7)
            {
                destroyer.Break();
                ui.BossExit();
            }
            yield return new WaitForFixedUpdate();
        }

        Destroy(gameObject);
    }
}
