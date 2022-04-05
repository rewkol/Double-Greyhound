using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerController : MonoBehaviour
{
    public int distance;
    public int number;
    public GameObject enemyType;

    private GameObject[] enemies;

    // Start is called before the first frame update
    void Start()
    {
        enemies = new GameObject[number];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "PHurtbox")
        {
            for (int i = 0; i < number; i++)
            {
                enemies[i] = Instantiate(enemyType, transform.position + new Vector3(distance, 0.0f, 0.0f), transform.rotation);
            }

            StartCoroutine(ResetSpawnRoutine());
        }
    }

    private IEnumerator ResetSpawnRoutine()
    {
        yield return new WaitForFixedUpdate();
        for (int i = 0; i < number; i++)
        {
            enemies[i].SendMessage("SpawnRandom");
        }

        Destroy(gameObject);
    }
}
