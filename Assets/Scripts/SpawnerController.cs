using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerController : MonoBehaviour
{
    public int distance;
    public int number;
    public GameObject enemyType;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        for(int i = 0; i < number; i++)
        {
            Instantiate(enemyType, transform.position + new Vector3(distance, 0.0f, 0.0f), transform.rotation);
        }

        Destroy(gameObject);
    }
}
