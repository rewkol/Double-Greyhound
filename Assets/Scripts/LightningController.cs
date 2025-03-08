using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningController : MonoBehaviour
{
    private const float Y_OFFSET = 5.2f;
    private Transform transform;
    private Animator animator;
    private PlayerController player;

    private int life;

    private SFXController sfxController;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();

        //Frames to live
        life = 8;

        float strike = Random.Range(0.0f, 1.0f);
        if ( strike > 0.33f && strike <= 0.67f )
        {
            animator.SetTrigger("Lightning2");
        }
        else if ( strike > 0.67f)
        {
            animator.SetTrigger("Lightning3");
        }

        sfxController = GameObject.FindObjectOfType<SFXController>();
        sfxController.PlaySFX2D("STM/Thunder_Trimmed", 1.0f, 10, 0.05f, false);
    }

    void FixedUpdate()
    {
        life--;
        if (life < 0)
        {
            Destroy(gameObject);
        }
    }
}
