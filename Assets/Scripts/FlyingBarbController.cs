using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingBarbController : MonoBehaviour
{
    private Transform transform;
    private int direction;
    private bool hit;
    private int count;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        hit = false;
        count = 0;

        TurnBarb(direction);
    }

    void FixedUpdate()
    {
        if (!hit)
        {
            transform.position += new Vector3(0.2f * ((direction + 1) % 2), 0.15f * (direction % 2), 0.0f);
            count++;
        }
        else
        {
            transform.position += new Vector3((count < 40 ? ((40 - count) / 40.0f) * 0.2f : 0.0f) * ((direction + 1) % 2), (count < 40 ? (count / 40.0f) * -0.2f : -0.2f), 0.0f);
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
            transform.localScale = new Vector3(6.0f, 6.0f, 1.0f);
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f * (direction == -1 ? 3.0f : 1.0f));
        }
    }

    public void SetDirection(int direction)
    {
        // Directions: 1 == up, 0 == right, -1 == down, -2 == left
        this.direction = direction;
        TurnBarb(direction);
    }

    private void TurnBarb(int direction)
    {
        if (direction != 0)
        {
            gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f * (direction + 2));
        }
        else
        {
            gameObject.transform.localScale = new Vector3(-6.0f, 6.0f, 1.0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
