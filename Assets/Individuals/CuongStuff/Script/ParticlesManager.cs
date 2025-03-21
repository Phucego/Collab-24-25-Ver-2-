using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesManager : MonoBehaviour
{
    public static ParticlesManager Instance;
    public ParticlesHandler ParticlesPrefab;

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public GameObject SpawnParticles(int particleID, float timeDestroy)
    {
        GameObject particle = Pooling.Spawn("Particles", ParticlesPrefab.ParticlesList[particleID], "_Particles");
        return particle;
    }
}
