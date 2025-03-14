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

    public void SpawnParticles(Vector3 spawnPos, int particleID, float timeDestroy)
    {
        
        GameObject particle = Instantiate(ParticlesPrefab.ParticlesList[particleID], spawnPos, Quaternion.identity);
        if (timeDestroy <= 0)
            Destroy(particle, 1f);
        else
            Destroy(particle, timeDestroy);
    }
}
