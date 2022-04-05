using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxDoorController : MonoBehaviour
{
    private CameraController player;
    private Animator animator;
    private Transform transform;

    public GameObject leftBlock;
    public GameObject rightBlock;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        transform = GetComponent<Transform>();
        player = GameObject.FindObjectsOfType<CameraController>()[0];
    }

    // Update is called once per frame
    void Update()
    {
        float dist = player.transform.position.x - transform.position.x;

        dist = dist * 5;

        if (dist < -57)
        {
            dist = -57;
        }
        else if (dist > 57)
        {
            dist = 57;
        }

        if (dist < 0)
        {
            leftBlock.SetActive(false);
            rightBlock.SetActive(true);
        }
        else
        {
            leftBlock.SetActive(true);
            rightBlock.SetActive(false);
        }

        dist += 57;
        dist = dist / 115;

        animator.SetFloat("frame", dist);
    }
}
