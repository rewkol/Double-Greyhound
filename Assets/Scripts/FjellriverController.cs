using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FjellriverController : MonoBehaviour
{
    private int count;
    private Transform transform;
    private int animationDivider;
    private float rotateModifier;
    private bool facingLeft;

    private PlayerController player;

    private SFXController sfxController;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindObjectsOfType<PlayerController>()[0];
        transform = GetComponent<Transform>();
        count = 0;
        rotateModifier = 1.0f;
        animationDivider = 48;

        sfxController = GameObject.FindObjectOfType<SFXController>();

        if (facingLeft)
        {
            transform.localScale = new Vector3(-6.0f, 6.0f, 1.0f);
            //Need to change rotation too
            rotateModifier = -1.0f;
        }

        GetComponent<Transform>().Find("Hit1").gameObject.SetActive(false);
        GetComponent<Transform>().Find("Hit2").gameObject.SetActive(false);

        transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotateModifier * 75.0f);
    }

    public void SetDirection(bool facingLeft)
    {
        this.facingLeft = facingLeft;
    }

    public bool GetDirection()
    {
        return this.facingLeft;
    }

    void FixedUpdate()
    {
        if (count == 42)
        {
            GetComponent<Transform>().Find("Hit1").gameObject.SetActive(true);
            GetComponent<Transform>().Find("Hit2").gameObject.SetActive(true);
            sfxController.PlaySFX2D("General/Axe_Woosh_Large", 0.5f, 20, 0.05f, false);
        }

        if (count >= 42)
        { 
            if ((count - 42) % animationDivider == 0)
            {
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotateModifier * -90.0f);
                sfxController.PlaySFX2D("General/Axe_Woosh_Large", 0.5f, 20, 0.05f, false);
            }
            else if ((count - 42) % animationDivider == animationDivider / 8)
            {
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotateModifier * -135.0f);
            }
            else if ((count - 42) % animationDivider == animationDivider / 4)
            {
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotateModifier * -180.0f);
                sfxController.PlaySFX2D("General/Axe_Woosh_Large", 0.5f, 20, 0.05f, false);
            }
            else if ((count - 42) % animationDivider == (3 * animationDivider) / 8)
            {
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotateModifier * -225.0f);
            }
            else if ((count - 42) % animationDivider == animationDivider / 2)
            {
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotateModifier * -270.0f);
                sfxController.PlaySFX2D("General/Axe_Woosh_Large", 0.5f, 20, 0.05f, false);
            }
            else if ((count - 42) % animationDivider == (5 * animationDivider) / 8)
            {
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotateModifier * -315.0f);
            }
            else if ((count - 42) % animationDivider == (3 * animationDivider) / 4)
            {
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                sfxController.PlaySFX2D("General/Axe_Woosh_Large", 0.5f, 20, 0.05f, false);
            }
            else if ((count - 42) % animationDivider == (7 * animationDivider) / 8)
            {
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotateModifier * -45.0f);
            }

            float yMovement = ((72 - count) / 120.0f) + (count == 42 ? 2.0f : 0.0f);
            Vector3 movement = new Vector3(rotateModifier * 0.065f + (count == 42 ? 5.0f * rotateModifier: 0.0f), yMovement, 0.0f);

            transform.position = transform.position + movement;
        }
        count++;

        if (count == 134)
        {
            Destroy(gameObject);
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }


    void Update()
    {

    }
}
