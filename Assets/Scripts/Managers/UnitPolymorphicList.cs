using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;

[System.Serializable]
public class UnitPolymorphicList
{
    
    public GameObject unit;
    public float moveSpeed;
    public float fogRange;
    
}

public class Farmer : UnitPolymorphicList
{
    public bool CanCollectCrops;
}

public class Scout : UnitPolymorphicList
{
    public bool CanScout;
}

public class Builder : UnitPolymorphicList
{
    public bool CanBuild;
}
