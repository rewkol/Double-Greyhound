using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBGController : MonoBehaviour
{
    public float speedx;
    public float speedy;
    public float start;
    public float stop;

    private Transform transform;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(float dist, float current)
    {
        //WATCH OUT! Y moves by the X distance. Remember that
        if (current >= start && current <= stop)
        {
            transform.position = transform.position + new Vector3(dist * speedx, dist * speedy, 0.0f);
        }
    }
}
