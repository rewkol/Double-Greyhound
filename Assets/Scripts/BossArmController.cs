using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossArmController : MonoBehaviour
{
    // We are the branch
    public GameObject rootObject;
    public GameObject leafObject;

    private Transform root;
    private Transform leaf;

    // Start is called before the first frame update
    void Start()
    {
        root = rootObject.GetComponent<Transform>();
        leaf = leafObject.GetComponent<Transform>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    public void Interpolate()
    {
        // Don't want to change the Z position based on the lerp
        float z = transform.position.z;
        transform.position = Vector3.Lerp(root.position, leaf.position, 0.5f);
        transform.position = new Vector3(transform.position.x, transform.position.y, z);
    }

}
