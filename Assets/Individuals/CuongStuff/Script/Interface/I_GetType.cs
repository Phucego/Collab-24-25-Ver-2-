using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_GetType
{
    TargetTypeEnum[] GetTargetType() { return new TargetTypeEnum[] { TargetTypeEnum.Grounded }; }
    float GetSpeed() { return 0f; }
    Vector3 GetVelocity() { return new Vector3(0,0,0); }
}
