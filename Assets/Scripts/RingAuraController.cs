using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingAuraController : MonoBehaviour
{
    private Transform transform;
    private Animator animatorTop;
    private Animator animatorBottom;

    private int i;
    private bool goesLeft;
    private bool manual;

    // Start is called before the first frame update
    void Start()
    {
        GameObject.FindObjectOfType<SFXController>().PlaySFX2D("STM/Halo_Large_Swoosh_modified", 0.7f, 10, 0.0f, false);
        transform = GetComponent<Transform>();
        animatorTop = transform.Find("RingTop").GetComponent<Animator>();
        animatorBottom = transform.Find("RingBottom").GetComponent<Animator>();
        transform.Find("GroundHitbox").gameObject.SetActive(false);
        i = 0;

        if (manual)
        {
            transform.Find("GroundHitbox").gameObject.SetActive(true);
            transform.Find("SkyHitbox").gameObject.SetActive(false);
            animatorTop.SetTrigger("Grounded");
            animatorBottom.SetTrigger("Grounded");
        }
        else
        {
            // Make sure height is right
            float deltaY = 1.80f - transform.position.y;
            float deltaZ = -1.016f - transform.position.z;
            transform.position = transform.position + new Vector3(0.0f, deltaY, deltaZ);

            if (Random.Range(0.0f, 1.0f) < 0.33f)
            {
                Ground();
            }
        }
    }

    void Ground()
    {
        // Default Pos is 1.80
        // Grounded Pos is -2.47
        transform.position = transform.position + new Vector3(0.0f, -4.27f, 0.0f);

        // Needs new Hitbox to close off bottom
        transform.Find("GroundHitbox").gameObject.SetActive(true);
        transform.Find("SkyHitbox").gameObject.SetActive(false);
        animatorTop.SetTrigger("Grounded");
        animatorBottom.SetTrigger("Grounded");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float x = -0.25f * (goesLeft ? 1.0f : -1.0f);
        if (i < 7)
        {
            x = 0.0f;
        }
        else if (i < 14)
        {
            x = x * ((i - 6) / 7.0f);
        }

        transform.position = transform.position + new Vector3(x, 0.0f, 0.0f);
        i++;

        if ( i == 500)
        {
            Destroy(gameObject);
        }
    }

    public void SetDirection(bool direction)
    {
        this.goesLeft = direction;
        if (!direction)
        {
            GetComponent<Transform>().localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }
    }

    public void LimitAir()
    {
        this.manual = true;
    }
}
