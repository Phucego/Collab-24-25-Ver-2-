using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffectCallback : MonoBehaviour
{
    public void Start()
    {
        var main = GetComponent<ParticleSystem>().main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    private void OnParticleSystemStopped()
    {
        Pooling.Despawn(gameObject.name, gameObject);
        transform.gameObject.SetActive(false);
    }
}

