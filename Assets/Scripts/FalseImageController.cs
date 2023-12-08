using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FalseImageController : MonoBehaviour
{
    private SpriteRenderer renderer;
    private int timer;
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        timer = 16;
        
        renderer.color = new Color(0.0f, 0.77f, 1.0f, 0.8f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (timer == 0)
        {
            Destroy(gameObject);
        }

        if (timer < 4)
        {
            renderer.color = new Color(0.1f, 0.0f, 1.0f, 0.2f);
        }
        else if (timer < 8)
        {
            renderer.color = new Color(0.0f, 0.18f, 1.0f, 0.4f);
        }
        else if (timer < 12)
        {
            renderer.color = new Color(0.0f, 0.53f, 1.0f, 0.6f);
        }
        timer--;
    }

    public void SetImage(Sprite sprite, bool direction)
    {
        if (renderer == null)
        {
            GetComponent<SpriteRenderer>().sprite = sprite;
        }
        else
        {
            renderer.sprite = sprite;
        }
        if (direction)
        {
            Transform t = GetComponent<Transform>();
            Vector3 scale = t.localScale;
            t.localScale = new Vector3(-6.0f, scale.y, scale.z);
        }
    }
}
