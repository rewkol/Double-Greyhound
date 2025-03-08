using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDoorController : MonoBehaviour
{
    public float chance;
    public GameObject enemy;
    public HitboxController hitbox;

    private Animator animator;
    private Transform transform;
    private PlayerController player;
    private bool willTrigger;

    private SFXController sfxController;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        transform = GetComponent<Transform>();
        player = GameObject.FindObjectsOfType<PlayerController>()[0];

        sfxController = GameObject.FindObjectOfType<SFXController>();

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
        sfxController.PlaySFX2D("HVHS/Door_Rattle_Short", 0.6f, 15, 0.05f, false);
        animator.SetTrigger("Open");
        for (int i = 0; i < 70; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        GameObject spawn = Instantiate(enemy, transform.position, transform.rotation);
        sfxController.PlaySFX2D("HVHS/Door_Open_Boosted", 1.0f, 15, 0.05f, false); 
        HitboxController hit = Instantiate(hitbox, transform.position - new Vector3(0.85f, 0.0f, 0.0f), transform.rotation);
        hit.SetX(2.3f);
        hit.SetY(5.0f);
        hit.SetTtl(200);
        hit.SetDamage(0);
        hit.SetParent(transform);
    }
}
