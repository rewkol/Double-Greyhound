using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaloController : MonoBehaviour
{
    private Transform transform;
    private bool facingLeft;
    private bool enemy;
    private int count;
    private float speed;

    private SFXController sfxController;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        count = 0;
        speed = 0.23f;

        sfxController = GameObject.FindObjectOfType<SFXController>();
        sfxController.PlaySFX2D("General/Halo", 1.0f, 10, 0.1f, false);
    }

    void FixedUpdate()
    {
        transform.position += new Vector3(speed * (facingLeft ? -1 : 1), 0.0f, 0.0f);
        count++;
        if (count > 500)
        {
            Destroy(gameObject);
        }
    }

    public void Hurt()
    {
        count = 0;
        // Can't hit your own halo down. That would suck
        Transform hitbox = transform.Find("HaloEHitbox");
        if (hitbox != null)
        {
            Destroy(hitbox.gameObject);
            facingLeft = !facingLeft;
            speed = speed * 1.25f;
            sfxController.PlaySFX2D("General/Ping", 0.5f, 20, 0.05f, false);
        }
    }

    public void SetDirection(bool left)
    {
        facingLeft = left;
    }

    public bool GetDirection()
    {
        return this.facingLeft;
    }

    public void SetAsEnemy(bool enemy)
    {
        transform = GetComponent<Transform>();
        this.enemy = enemy;
        // Change which hitbox is active
        if (enemy)
        {
            Transform hitbox = transform.Find("HaloPHitbox");
            Destroy(hitbox.gameObject);
        }
        else
        {
            Transform hitbox = transform.Find("HaloEHitbox");
            Destroy(hitbox.gameObject);
            Transform hurtbox = transform.Find("HaloHurtbox");
            Destroy(hurtbox.gameObject);
        }
    }

    public Vector3 GetPosition()
    {
        return this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
