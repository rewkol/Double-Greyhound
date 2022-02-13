using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasicEnemyController : MonoBehaviour
{
    //Public Objects
    public HitboxController hitbox;

    //Public variables
    public float speed;
    public float limitYTop;
    public float limitYBottom;

    //Private variables
    private Transform transform;
    private bool facingLeft;
    private int cooldown;
    private int stun;
    private float spacingX;
    private float spacingY;
    private bool inPosX;
    private bool inPosY;
    private int health;
    private PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        transform.position = new Vector3(transform.position.x, limitYTop, -1.0f);
        player = GameObject.FindObjectsOfType<PlayerController>()[0];
        facingLeft = true;
        cooldown = 0;
        stun = 0;
        spacingX = 3.0f;
        spacingY = 1.0f;
        health = 3;
        inPosX = false;
        inPosY = false;
    }

    void FixedUpdate()
    {
        //Movement code
        float moveHorizontal = player.GetPosition().x - transform.position.x;
        float moveVertical = player.GetPosition().y - transform.position.y;

        //Can't flip around while cooling down
        if (cooldown == 0 && stun == 0)
        {
            if (moveHorizontal > 0)
            {
                facingLeft = false;
                transform.localScale = new Vector3(1.0f, transform.localScale.y, transform.localScale.z);
            }
            else if (moveHorizontal < 0)
            {
                facingLeft = true;
                transform.localScale = new Vector3(-1.0f, transform.localScale.y, transform.localScale.z);
            }
        }

        //Space away from player for attacking
        if (Mathf.Abs(moveVertical) < spacingY)
        {
            moveVertical = 0.0f;
            inPosY = true;
        }
        else
        {
            inPosY = false;
        }
        if (Mathf.Abs(moveHorizontal) < spacingX)
        {
            moveHorizontal = 0.0f;
            inPosX = true;
        }
        else
        {
            inPosX = false;
        }

        if (moveHorizontal != 0)
        {
            moveHorizontal = moveHorizontal / Mathf.Abs(moveHorizontal);
        }
        if (moveVertical != 0)
        {
            moveVertical = moveVertical / Mathf.Abs(moveVertical);
        }

        //Bounds checks
        if (transform.position.y + (moveVertical * speed) > limitYTop)
        {
            moveVertical = (limitYTop - transform.position.y) / speed;
        }
        else if (transform.position.y + (moveVertical * speed) < limitYBottom)
        {
            moveVertical = (limitYBottom - transform.position.y) / speed;
        }

        float moveZ = 0.01f * moveVertical;

        Vector3 movement = new Vector3(moveHorizontal, moveVertical, moveZ);

        //Can't move while attacking!
        if (cooldown > 0)
        {
            cooldown--;
            movement = new Vector3(0.0f, 0.0f, 0.0f);
        }
        if (stun > 0)
        {
            stun--;
            movement = new Vector3(0.0f, 0.0f, 0.0f);
        }

        transform.position = transform.position + (movement * speed);

        //Dummy code to get rid of warnings
        facingLeft = !facingLeft;

        //Attack code
        if (inPosX && inPosY && cooldown == 0)
        {
            Punch();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Hurt(int damage)
    {
        if (stun == 0)
        {
            stun = 20;
            health -= damage;
            if (health <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private void Punch()
    {
        cooldown = 500;
        HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(2.2f * (facingLeft ? 1 : -1), 0.63f, 0.0f), transform.rotation);
        hit.SetX(1.2f);
        hit.SetY(1.8f);
        hit.SetTtl(280);
        hit.SetVisible(true);
    }
}
