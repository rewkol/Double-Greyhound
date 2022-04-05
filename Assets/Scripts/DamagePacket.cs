using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePacket
{
    private int damage;
    private bool direction;

    public DamagePacket()
    {
        damage = 1;
        direction = false;
    }

    public DamagePacket(int dmg, bool hitFromLeft)
    {
        damage = dmg;
        direction = hitFromLeft;
    }

    public int getDamage()
    {
        return this.damage;
    }

    public bool getDirection()
    {
        return this.direction;
    }
}
