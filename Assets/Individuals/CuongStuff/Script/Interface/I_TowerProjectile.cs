using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_TowerProjectile
{
    public void SetDamage(float dmg);
    public void SetRadius(float radius);
    public void SetDebuff(float duration);
}
