using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPuppet
{
    public void Die();
    public void Disappear();
    public void StartBattle();
    public void SetParent(PuppetMasterController master);
}
