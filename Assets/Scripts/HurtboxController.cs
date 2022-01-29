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

	void OnTriggerEnter2D(Collider2D other)
	{
        //Hurt enemy!
		if (other.tag == "PHitbox" && gameObject.tag == "EHurtbox")
		{
            transform.parent.gameObject.SendMessage("Hurt", other.GetComponent<HitboxController>().GetDamage());
        }
        //Hurt player!
        else if (other.tag == "EHitbox" && gameObject.tag == "PHurtbox")
        {
            transform.parent.gameObject.SendMessage("Hurt", other.GetComponent<HitboxController>().GetDamage());
        }
    }
}
