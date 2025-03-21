using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffectCallback : MonoBehaviour
{
    public void Start()
    {
        var main = GetComponent<ParticleSystem>().main;
        var name = gameObject.name;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    private void OnParticleSystemStopped()
    {
        Pooling.Despawn(name, gameObject);
        transform.gameObject.SetActive(false);
    }
}

