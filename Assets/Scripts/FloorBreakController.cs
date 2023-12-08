using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorBreakController : MonoBehaviour
{
    private Animator animator;
    private Transform topLeft;
    private Transform topRight;
    private Transform bottomLeft;
    private Transform bottomRight;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        topLeft = transform.Find("SJHSGymPoolArena - RubbleTopLeft");
        topRight = transform.Find("SJHSGymPoolArena - RubbleTopRight");
        bottomLeft = transform.Find("SJHSGymPoolArena - RubbleBottomLeft");
        bottomRight = transform.Find("SJHSGymPoolArena - RubbleBottomRight");
    }

    public void Break()
    {
        StartCoroutine(BreakRoutine());
    }

    // Using Coroutines as a hack to do frame by frame animation instead of the smooth curves in the Animator
    private IEnumerator BreakRoutine()
    {
        // TODO remove after testing
        for (int i = 0; i < 500; i ++)
        {
            yield return new WaitForFixedUpdate();
        }
        animator.SetTrigger("Break");
        // Rest on this broken frame so they can appreciate it while Captain Greyhound starts his knockback animation
        // and boss plummets. Then start animation as Captain Greyhound begins to fall toward the hole
        for (int i = 0; i < 60; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        for (int i = 0; i < 100; i++)
        {
            float frontSpeed = 0.2f - ((i / 5) * 0.01f);
            float backSpeed = frontSpeed * (0.33f + ((i/10) * 0.067f));
            topLeft.position += new Vector3(-frontSpeed, frontSpeed, 0.0f);
            topRight.position += new Vector3(frontSpeed, frontSpeed, 0.0f);
            bottomLeft.position += new Vector3(-backSpeed / 2.25f, backSpeed, 0.0f);
            bottomRight.position += new Vector3(backSpeed / 2.25f, backSpeed, 0.0f);
            yield return new WaitForFixedUpdate();
        }
        animator.SetTrigger("Broken");
    }
}
