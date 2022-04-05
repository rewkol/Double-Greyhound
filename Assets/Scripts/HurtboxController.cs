using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtboxController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateScale(float x, float y)
    {
        transform.localScale = new Vector3(x, y, 1.0f);
    }

    public void Move(Vector3 vector)
    {
        transform.position = transform.position + vector;
    }


    void OnTriggerEnter2D(Collider2D other)
	{
        bool hitFromLeft = false;
        //Use parent's centre points to decide how to hit, so that punches passing through aren't wonky
        if (other.transform.parent.position.x < transform.parent.position.x)
        {
            hitFromLeft = true;
        }
        //Hurt enemy!
		if (other.tag == "PHitbox" && gameObject.tag == "EHurtbox")
		{
            transform.parent.gameObject.SendMessage("Hurt", new DamagePacket(other.GetComponent<HitboxController>().GetDamage(), hitFromLeft));
        }
        //Hurt player!
        else if (other.tag == "EHitbox" && gameObject.tag == "PHurtbox")
        {
            transform.parent.gameObject.SendMessage("Hurt", new DamagePacket(other.GetComponent<HitboxController>().GetDamage(), hitFromLeft));
        }
    }
}
