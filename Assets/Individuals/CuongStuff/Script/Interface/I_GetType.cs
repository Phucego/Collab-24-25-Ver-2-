using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_GetType
{
    List<eType> GetTargetType();

    public float GetSpeed() { return 0f; }
}
