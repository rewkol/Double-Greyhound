using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorBreakController : MonoBehaviour
{
    public RainController rain;
    public BossController finalBoss;

    private Animator animator;
    private PlayerController player;
    private UIController ui;

    // Rubble transforms
    private Transform topLeft;
    private Transform topRight;
    private Transform bottomLeft;
    private Transform bottomRight;

    // "Static" transforms
    private Transform doorTop;
    private Transform doorBottom;
    private Transform floorNormal;
    private Transform brokenTop;
    private Transform brokenBottom;
    private Transform poolBottom;
    private Transform poolTop;

    // Splash
    private Transform splash;

    private GameObject musicController;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindObjectsOfType<PlayerController>()[0];
        ui = GameObject.FindObjectsOfType<UIController>()[0];
        musicController = GameObject.Find("MusicController");

        topLeft = transform.Find("SJHSGymPoolArena - RubbleTopLeft");
        topRight = transform.Find("SJHSGymPoolArena - RubbleTopRight");
        bottomLeft = transform.Find("SJHSGymPoolArena - RubbleBottomLeft");
        bottomRight = transform.Find("SJHSGymPoolArena - RubbleBottomRight");

        doorTop = transform.Find("SJHSParallaxDoor - Top");
        doorBottom = transform.Find("SJHSParallaxDoor - Bottom");
        floorNormal = transform.Find("SJHSGymPoolArena - FloorNormal");
        brokenTop = transform.Find("SJHSGymPoolArena - FloorBrokenTop");
        brokenBottom = transform.Find("SJHSGymPoolArena - FloorBrokenBottom");
        poolTop = transform.Find("SJHSGymPoolArena - PoolTop");
        poolBottom = transform.Find("SJHSGymPoolArena - Base");

        splash = transform.Find("SJHSGymPoolArena - Splash");
    }

    public void Break()
    {
        StartCoroutine(BreakRoutine());
    }

    // Using Coroutines as a hack to do frame by frame animation instead of the smooth curves in the Animator
    private IEnumerator BreakRoutine()
    {
        gameObject.tag = "Boss";
        animator.SetTrigger("Break");
        for (int i = 0; i < 100; i++)
        {
            float frontSpeed = 0.4f - ((i / 5) * 0.01f);
            float backSpeed = frontSpeed * (0.33f + ((i/10) * 0.067f));
            topLeft.position += new Vector3(-frontSpeed, frontSpeed, 0.0f);
            topRight.position += new Vector3(frontSpeed, frontSpeed, 0.0f);
            bottomLeft.position += new Vector3(-backSpeed / 2.25f, backSpeed, 0.0f);
            bottomRight.position += new Vector3(backSpeed / 2.25f, backSpeed, 0.0f);

            if (i == 25)
            {
                player.KnockbackAnimation(false);
            }
            if (i > 30)
            {
                float panSpeed = (i < 65) ? 0.15f : 0.16f;
                doorTop.position += new Vector3(0.0f, panSpeed, 0.0f);
                doorBottom.position += new Vector3(0.0f, panSpeed, 0.0f);
                floorNormal.position += new Vector3(0.0f, panSpeed, 0.0f);
                brokenTop.position += new Vector3(0.0f, panSpeed, 0.0f);
                brokenBottom.position += new Vector3(0.0f, panSpeed, 0.0f);
                poolTop.position += new Vector3(0.0f, panSpeed, 0.0f);
                poolBottom.position += new Vector3(0.0f, panSpeed, 0.0f);

                if ( i > 50)
                splash.position += new Vector3(0.0f, panSpeed * 4.0f, 0.0f);
            }
            yield return new WaitForFixedUpdate();
        }

        for (int i = 0; i < 64; i++)
        {
            float panSpeed = 0.17f;
            doorTop.position += new Vector3(0.0f, panSpeed, 0.0f);
            doorBottom.position += new Vector3(0.0f, panSpeed, 0.0f);
            floorNormal.position += new Vector3(0.0f, panSpeed, 0.0f);
            brokenTop.position += new Vector3(0.0f, panSpeed, 0.0f);
            brokenBottom.position += new Vector3(0.0f, panSpeed, 0.0f);
            poolTop.position += new Vector3(0.0f, panSpeed, 0.0f);
            poolBottom.position += new Vector3(0.0f, panSpeed, 0.0f);

            splash.position += new Vector3(0.0f, panSpeed * 4.0f, 0.0f);

            if (i == 63)
            {
                player.LandingAnimation();
            }
            yield return new WaitForFixedUpdate();
        }

        for (int i = 0; i < 600; i++)
        {
            if (i % 3 == 0)
            {
                float y = Random.Range(-4.5f, -0.45f);
                float z = -1.0f + ((y + 0.45f) * 0.01f);
                Instantiate(rain, new Vector3(player.GetPosition().x + Random.Range(-25.0f, 25.0f), y, z), transform.rotation);
            }
        }

        for (int i = 0; i < 100; i++)
        {
            if (i == 80 && musicController != null)
            {
                musicController.SendMessage("StartNextSong");
            }
            yield return new WaitForFixedUpdate();
        }

        float midpoint = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        Instantiate(finalBoss, new Vector3(midpoint + 0.06f, -4.47f, -0.05f), transform.rotation);
        ui.EndManualCutscene();

        animator.SetTrigger("Broken");
        gameObject.tag = "Untagged";
    }
}
