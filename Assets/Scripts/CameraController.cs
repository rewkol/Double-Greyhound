using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlayerController player;

    private Transform transform;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        int enemiesOnScreen = GameObject.FindGameObjectsWithTag("Enemy").Length;
        int bossOnScreen = GameObject.FindGameObjectsWithTag("Boss").Length;

        int totalEnemies = enemiesOnScreen + bossOnScreen;
        if(totalEnemies == 0 && player.transform.position.x > transform.position.x)
        {
            float dist = player.transform.position.x - transform.position.x;
            if (dist > 0.1f)
            {
                dist = 0.1f;
            }
            transform.position = transform.position + new Vector3(dist, 0.0f, 0.0f);
        }
    }
}
