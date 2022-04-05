using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDoorController : MonoBehaviour
{
    public float chance;
    public GameObject enemy;

    private Animator animator;
    private Transform transform;
    private PlayerController player;
    private bool willTrigger;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        transform = GetComponent<Transform>();
        player = GameObject.FindObjectsOfType<PlayerController>()[0];

        if (Random.Range(0.0f, 1.0f) < chance)
        {
            willTrigger = true;
        }
        else if (Random.Range(0.0f, 1.0f) < 0.5f)
        {
            animator.SetTrigger("ForceOpen");
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (willTrigger && player.transform.position.x > transform.position.x)
        {
            willTrigger = false;
            StartCoroutine(DoorWiggleRoutine());
        }
    }

    private IEnumerator DoorWiggleRoutine()
    {
        animator.SetTrigger("Open");
        for (int i = 0; i < 70; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        GameObject spawn = Instantiate(enemy, transform.position, transform.rotation);
    }
}
