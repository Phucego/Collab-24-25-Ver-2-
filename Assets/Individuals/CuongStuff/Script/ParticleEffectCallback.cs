using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffectCallback : MonoBehaviour
{

    public void Init()
    {

    }

    private void OnParticleSystemStopped()
    {
        //;Pooling.Despawn(gameObject);
    }
}

