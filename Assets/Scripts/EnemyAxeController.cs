using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAxeController : MonoBehaviour
{
    private int count;
    private Transform transform;
    private int animationDivider;
    private float rotateModifier;
    private bool facingLeft;

    private SFXController sfxController;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        count = 0;
        rotateModifier = 1.0f;
        animationDivider = 24;

        sfxController = GameObject.FindObjectOfType<SFXController>();

        if (facingLeft)
        {
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
            //Need to change rotation too
            rotateModifier = -1.0f;
        }
    }

    void FixedUpdate()
    {
        if (count % animationDivider == 0)
        {
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotateModifier * -90.0f);
        }
        else if (count % animationDivider == animationDivider / 4)
        {
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotateModifier * -180.0f);
        }
        else if (count % animationDivider == animationDivider / 2)
        {
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotateModifier * -270.0f);
        }
        else if (count % animationDivider == (3 * animationDivider) / 4)
        {
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        }

        float yMovement = (30 - count) / 170.0f;
        Vector3 movement = new Vector3(rotateModifier * 0.055f, yMovement, 0.0f);

        transform.position = transform.position + movement;

        count++;

        if (count == 120)
        {
            Destroy(gameObject);
        }
    }

    public void Hurt()
    {
        Transform hitbox = transform.Find("AxeHitbox");
        if (hitbox != null)
        {
            Destroy(hitbox.gameObject);
            facingLeft = !facingLeft;
            rotateModifier = -1 * rotateModifier;
            sfxController.PlaySFX2D("General/Ping", 0.5f, 20, 0.05f, false);
        }
    }

    public void SetDirection(bool facingLeft)
    {
        this.facingLeft = facingLeft;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
