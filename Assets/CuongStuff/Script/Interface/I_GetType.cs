using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_GetType
{
    TargetTypeEnum[] GetTargetType() { return new TargetTypeEnum[] { TargetTypeEnum.Grounded }; }
}
