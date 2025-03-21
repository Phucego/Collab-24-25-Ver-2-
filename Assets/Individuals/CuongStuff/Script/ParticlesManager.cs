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

    public GameObject SpawnParticles(int particleID, string particleName)
    {
        string particlename = ParticlesPrefab.ParticlesList[particleID].name + "(Clone)";
        GameObject particle = Pooling.Spawn(particlename, ParticlesPrefab.ParticlesList[particleID], "_Particles");
        return particle;
    }
}
