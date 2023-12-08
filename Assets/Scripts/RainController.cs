using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainController : MonoBehaviour
{
    private int count;
    private Transform transform;
    private Animator animator;
    private Vector3 target;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        count = 0;

        target = transform.position;
        transform.position += new Vector3(7.2f, 26.4f, 0.0f);
    }

    void FixedUpdate()
    {
        Vector3 movement = new Vector3(-0.09f, -0.33f, 0.0f);


        count++;

        if (count >= 80)
        {
            animator.SetTrigger("Landed");
        }
        else
        {
            transform.position = transform.position + movement;
        }
    }

    public void Land()
    {
        Destroy(gameObject);
    }
}
