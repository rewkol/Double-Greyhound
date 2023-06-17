using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchAuraController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Real simple
        float x = -0.12f;
        transform.position = transform.position + new Vector3(x, 0.0f, 0.0f);
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
