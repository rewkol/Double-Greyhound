using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DressCrownController : MonoBehaviour
{
    //Public Variables
    public bool crown;

    //Private variables
    private Transform transform;
    private Animator animator;


    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();

        if (crown)
        {
            animator.SetTrigger("Crown");
            StartCoroutine(CrownRoutine());
        }
    }

    private IEnumerator CrownRoutine()
    {
        for(int i = 0; i < 20; i++)
        {
            transform.position += new Vector3(0.0f, -0.09f - (i * 0.0015f), 0.0f);
            yield return new WaitForFixedUpdate();
        }
    }

    public void SetFacingLeft(bool facingLeft)
    {
        if(!facingLeft)
        {
            transform.localScale = new Vector3(-6.0f, 6.0f, 1.0f);
        }
    }
}
