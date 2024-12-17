using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    public LevelDataSO m_LevelDataSO;
    public LevelDataSO LevelDataSO => m_LevelDataSO;

    [Header("Sound Interval Settings")]
    public float minTime = 2f;    
    public float maxTime = 5f;    

    [Header("Pause Interval Settings")]
    public float minPauseTime = 5f;   
    public float maxPauseTime = 10f;  

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartCoroutine(PlaySoundAtRandomIntervals());
    }

    private IEnumerator PlaySoundAtRandomIntervals()
    {
        while (true)
        {
            // Play sounds for a while
            for (int i = 0; i < 5; i++) 
            {
                float randomTime = Random.Range(minTime, maxTime); 
                yield return new WaitForSeconds(randomTime);       
                AudioManager.Instance.PlaySoundEffect("Wind_SFX");
            }

            // Generate a random pause duration
            float randomPause = Random.Range(minPauseTime, maxPauseTime);
            Debug.Log($"Pausing sound playback for {randomPause} seconds...");
            yield return new WaitForSeconds(randomPause); 
            Debug.Log("Resuming sound playback...");
        }
    }
}