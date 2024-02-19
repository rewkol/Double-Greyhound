using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageConduit : MonoBehaviour
{
    private int stun;

    void Start()
    {
        stun = 0;
    }

    void FixedUpdate()
    {
        if (stun > 0)
        {
            stun--;
        }
    }

    public void Hurt(DamagePacket packet)
    {
        if (stun == 0)
        {
            transform.parent.gameObject.SendMessage("Hurt", packet);
            stun = 5;
        }
    }
}
