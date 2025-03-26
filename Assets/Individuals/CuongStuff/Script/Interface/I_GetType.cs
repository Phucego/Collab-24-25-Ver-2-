using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_GetType
{
    eType[] GetTargetType() { return new eType[] { eType.Normal }; }
}
