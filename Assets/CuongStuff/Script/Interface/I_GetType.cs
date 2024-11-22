using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_GetType
{
    TargetType[] GetTargetType() { return new TargetType[] { TargetType.Grounded }; }
}
