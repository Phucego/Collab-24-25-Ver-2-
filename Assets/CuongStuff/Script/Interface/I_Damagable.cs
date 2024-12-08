using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_Damagable
{
    public void TakeDamage(float damage);
    public void ApplyDebuff(float smth);
}
