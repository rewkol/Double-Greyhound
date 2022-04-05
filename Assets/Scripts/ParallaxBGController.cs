using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBGController : MonoBehaviour
{
    public GameObject layer1;
    public GameObject layer2;
    public GameObject layer3;
    public float speed1x;
    public float speed1y;
    public float start1;
    public float stop1;
    public float speed2x;
    public float speed2y;
    public float start2;
    public float stop2;
    public float speed3x;
    public float speed3y;
    public float start3;
    public float stop3;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(float dist, float current)
    {
        //WATCH OUT! Y moves by the X distance. Remember that
        if (current >= start1 && current <= stop1)
        {
            layer1.transform.position = layer1.transform.position + new Vector3(dist * speed1x, dist * speed1y, 0.0f);
        }
        if (current >= start2 && current <= stop2)
        {
            layer2.transform.position = layer2.transform.position + new Vector3(dist * speed2x, dist * speed2y, 0.0f);
        }
        if (current >= start3 && current <= stop3)
        {
            layer3.transform.position = layer3.transform.position + new Vector3(dist * speed3x, dist * speed3y, 0.0f);
        }
    }
}
