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

    public void MoveDirect(Vector3 vector)
    {
        transform.localPosition = vector;
    }


    void OnTriggerEnter2D(Collider2D other)
	{
        
        //Hurt enemy!
		if (other.tag == "PHitbox" && gameObject.tag == "EHurtbox")
		{
            //Need to include this code inside this check to make sure it isn't trying to grab this info off objects that won't have parents
            bool hitFromLeft = false;
            //Use parent's centre points to decide how to hit, so that punches passing through aren't wonky
            if (other.transform.parent.position.x < transform.parent.position.x)
            {
                hitFromLeft = true;
            }
            transform.parent.gameObject.SendMessage("Hurt", new DamagePacket(other.GetComponent<HitboxController>().GetDamage(), hitFromLeft));
        }
        //Hurt player!
        else if (other.tag == "EHitbox" && gameObject.tag == "PHurtbox")
        {
            bool hitFromLeft = false;
            //Use parent's centre points to decide how to hit, so that punches passing through aren't wonky
            if (other.transform.parent.position.x < transform.parent.position.x)
            {
                hitFromLeft = true;
            }
            transform.parent.gameObject.SendMessage("Hurt", new DamagePacket(other.GetComponent<HitboxController>().GetDamage(), hitFromLeft));
        }
    }
}
