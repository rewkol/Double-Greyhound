using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingBarbController : MonoBehaviour
{
    private Transform transform;
    private bool facingLeft;
    private bool hit;
    private int count;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        hit = false;
        count = 0;

        if (!facingLeft)
        {
            transform.localScale = new Vector3(-6.0f, 6.0f, 1.0f);
        }
    }

    void FixedUpdate()
    {
        if (!hit)
        {
            transform.position += new Vector3(0.2f * (facingLeft ? -1 : 1), 0.0f, 0.0f);
            count++;
        }
        else
        {
            transform.position += new Vector3((count < 40 ? ((40 - count) / 40.0f) * 0.2f : 0.0f) * (facingLeft ? -1 : 1), (count < 40 ? (count / 40.0f) * -0.2f : -0.2f), 0.0f);
            count++;
        }

        if (count > 500)
        {
            Destroy(gameObject);
        }
    }

    public void Hurt()
    {
        hit = true;
        count = 0;
        Transform hitbox = transform.Find("BarbHitbox");
        if (hitbox != null)
        {
            Destroy(hitbox.gameObject);
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f * (facingLeft ? 1 : -1));
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
