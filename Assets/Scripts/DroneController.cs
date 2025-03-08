using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneController : MonoBehaviour
{
    //Public variables
    public float limitYBottom;

    //Private variables
    private Animator animator;
    private PlayerController player;
    private float height;
    private bool facingLeft;

    //Constants
    private float DEFAULT_HEIGHT = 1.32f;

    private SFXController sfxController;


    // Start is called before the first frame update
    void Start()
    { 
        animator = GetComponent<Animator>();
        player = GameObject.FindObjectsOfType<PlayerController>()[0];
        facingLeft = true;

        sfxController = GameObject.FindObjectOfType<SFXController>();
    }

    void FixedUpdate()
    {
        
    }

    public void SwitchDirection(bool left)
    {
        facingLeft = left;
    }

    public void Hurt()
    {
        StartCoroutine(BlinkRoutine());
    }

    public void Die()
    {
        StartCoroutine(DeathRoutine());
        sfxController.PlaySFX2D("SHS/Death_Bee_Short", 1.0f, 20, 0.1f, false);
    }

    private IEnumerator BlinkRoutine()
    {
        GetComponent<SpriteRenderer>().color = new Color(255.0f, 0.0f, 0.0f, 1.0f);
        for (int i = 0; i < 7; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        GetComponent<SpriteRenderer>().color = new Color(255.0f, 255.0f, 255.0f, 1.0f);
    }

    private IEnumerator DeathRoutine()
    {
        int i = 0;
        animator.SetTrigger("Die");
        while (i < 999)
        {
            transform.position = transform.position + new Vector3((facingLeft ? 1 : -1) * 0.0145f, (22 - i) / 205.0f, 0.0f);
            i++;
            yield return new WaitForFixedUpdate();
        }

        Destroy(gameObject);
    }

    public void BuzzSound()
    {
        // Once again the original sound didn't work out at all lol. I understand TCRF so much now
        if (!player.IsDead())
        {
            sfxController.PlaySFX2D("STM/Flap_Boosted", 0.8f, 30, 0.1f, false);
        }
    }
}
