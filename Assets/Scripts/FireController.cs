using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireController : MonoBehaviour
{
    public HitboxController hitbox;

    private Transform transform;
    private PlayerController player;
    private Vector3[] targets;
    private int trackTimer;
    private bool onTarget;
    private bool facingLeft;

    private const float speed = 0.05f;
    private const float VERTICAL_PADDING = -0.7f;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        player = GameObject.FindObjectsOfType<PlayerController>()[0];
        // Player hasn't been instantiated fully yet if fire exists on first frame...
        targets = new Vector3[20];
        for (int i = 0; i < 20; i++)
        {
            targets[i] = new Vector3(0.0f, 0.0f, 0.0f);
        }
        trackTimer = 100;
        onTarget = false;
        facingLeft = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Track player for some time, after this target stops updating
        if (trackTimer > 0)
        {
            trackTimer--;
            for (int i = 0; i < 19; i++)
            {
                targets[i] = targets[i + 1];
            }
            targets[19] = player.GetPosition() + new Vector3(0.0f, VERTICAL_PADDING, 0.0f);
        }

        // If on target start healing routine
        if (onTarget)
        {
            StartCoroutine(HealingRoutine());
        }
        else
        {
            float moveHorizontal = targets[0].x - transform.position.x;
            float moveVertical = targets[0].y - transform.position.y; 

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

            if (moveHorizontal != 0)
            {
                moveHorizontal = moveHorizontal / Mathf.Abs(moveHorizontal);
            }
            if (moveVertical != 0)
            {
                moveVertical = moveVertical / Mathf.Abs(moveVertical);
            }

            if (Mathf.Abs(targets[0].x - transform.position.x) < speed)
            {
                moveHorizontal = (targets[0].x - transform.position.x) / speed;
            }
            if (Mathf.Abs(targets[0].y - transform.position.y) < speed)
            {
                moveVertical = (targets[0].y - transform.position.y) / speed;
            }

            if (moveHorizontal == 0.0f && moveVertical == 0.0f)
            {
                onTarget = true;
            }

            float moveZ = 0.01f * moveVertical;

            Vector3 movement = new Vector3(moveHorizontal, moveVertical, moveZ);
            transform.position = transform.position + (movement * speed);
        }
    }

    private IEnumerator HealingRoutine()
    {
        for (int i = 0; i < 50; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        // Instantiate healing hitbox
        // TODO: Align hitbox properly
        HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(0.0f, 0.1f, 0.0f), transform.rotation);
        hit.SetX(0.1139783f * 6.0f);
        hit.SetY(0.1064522f * 6.0f);
        hit.SetTtl(400);
        hit.SetDamage(-2);
        hit.SetParent(transform);

        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        // Disappear
        Destroy(gameObject);
    }
}
