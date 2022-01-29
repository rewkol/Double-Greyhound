using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //Public Objects
    public HitboxController hitbox;

    //Public Variables
    public float speed;
    public float limitYTop;
    public float limitYBottom;
    public Text debugText;

    //Private variables
    private Transform transform;
    private Animator animator;
    private SpriteRenderer renderer;
    private float limitXLeft;
    private float limitXRight;
    private bool facingLeft;
    private int special;
    private int cooldown;
    private int specialCooldown;
    private int stun;
    private int health;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        facingLeft = false;
        special = 0;
        cooldown = 0;
        specialCooldown = 0;
        stun = 0;
        health = 3;
        limitXLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        limitXRight = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
    }

    void FixedUpdate()
    {
        //Movement code
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

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


        if (transform.position.x + (moveHorizontal * speed) > limitXRight)
        {
            moveHorizontal = (limitXRight - transform.position.x) / speed;
        }
        else if (transform.position.x + (moveHorizontal * speed) < limitXLeft)
        {
            moveHorizontal = (limitXLeft - transform.position.x) / speed;
        }
        if (transform.position.y + (moveVertical * speed) > limitYTop)
        {
            moveVertical = (limitYTop - transform.position.y) / speed;
        }
        else if (transform.position.y + (moveVertical * speed) < limitYBottom)
        {
            moveVertical = (limitYBottom - transform.position.y) / speed;
        }

        //Z decreases (toward camera) as the player moves down/increases while moving up
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

        //Attack code
        if (cooldown == 0 && stun == 0 && Input.GetAxis("Fire1") > 0)
        {
            Punch();
        }
        else if (cooldown == 0 && stun == 0 && Input.GetAxis("Fire2") > 0)
        {
            animator.SetTrigger("Kick");
        }

        debugText.text = "Facing Left: " + cooldown;
    }

    // Update is called once per frame
    void Update()
    {
        limitXLeft = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        limitXRight = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
    }

    private void Punch()
    {
        cooldown = 14;
        animator.SetTrigger("Punch");
        HitboxController hit = Instantiate(hitbox, transform.position + new Vector3(1.1f * (facingLeft ? -1 : 1), 0.63f, 0.0f), transform.rotation);
        hit.SetX(1.2f);
        hit.SetY(1.8f);
        hit.SetTtl(280);
    }

    public void Hurt(int damage)
    {
        if (stun == 0)
        {
            //TODO: Should make stun do more if you got hurt more or do knockback instead if hurt too much
            stun = 20;
            health -= damage;
            if (health <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
