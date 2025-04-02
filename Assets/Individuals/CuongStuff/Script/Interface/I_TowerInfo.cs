using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_TowerInfo
{
    public virtual string GetCurrentStats()
    { 
        return null;
    }

    public virtual string GetName()
    {
        return null;
    }

    public virtual string GetLevelString()
    {

        return null;
    }
    public virtual int GetLevelInt()
    {
        return 0;
    }

    public virtual string GetUpgradeStats()
    {
        return null;
    }

    public virtual string GetCost()
    {
        return null; 
    }

    public virtual string GetSellValue()
    {
        return null;
    }
}
