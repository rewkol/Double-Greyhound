using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxController : MonoBehaviour
{
    public long ttl;
    public int damage;
    public float x;
    public float y;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        long delta = (long) (Time.fixedDeltaTime * 1000);

        ttl -= delta;
        if (ttl < 0)
        { 
            Destroy(gameObject);
        }
    }

    public void SetParent(Transform parent)
    {
        transform.parent = parent;
    }

    public void SetTtl(long ttl)
    {
        this.ttl = ttl;
    }

    public void SetX(float x)
    {
        this.x = x;
        UpdateScale();
    }

    public void SetY(float y)
    {
        this.y = y;
        UpdateScale();
    }

    private void UpdateScale()
    {
        transform.localScale = new Vector3(x, y, 1.0f);
    }

    public void SetVisible(bool visible)
    {
        GetComponent<SpriteRenderer>().enabled = visible;
    }

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    public int GetDamage()
    {
        return damage;
    }

    public void Kill()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
