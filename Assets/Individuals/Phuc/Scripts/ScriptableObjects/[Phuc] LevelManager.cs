using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

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

    [Header("Post-Processing Effect")]
    [SerializeField] private PostProcessVolume _volume;
    public Vignette _vignette;
    public float effectTime = 1f;
    private float intensity = 0f;

    private void Awake()
    {
        instance = this;
        _volume = GetComponent<PostProcessVolume>();
    
        if (_volume.profile.TryGetSettings(out _vignette) == false || _vignette == null)
        {
            Debug.LogError("Vignette effect not found in PostProcessVolume!");
            return;
        }
    
        _vignette.enabled.Override(false);
    }


    void Start()
    {
        StartCoroutine(PlaySoundAtRandomIntervals());
    }

    private IEnumerator PlaySoundAtRandomIntervals()
    {
        while (true)
        {
            for (int i = 0; i < 5; i++)
            {
                float randomTime = Random.Range(minTime, maxTime);
                yield return new WaitForSeconds(randomTime);
                AudioManager.Instance.PlaySoundEffect("Wind_SFX");
            }

            float randomPause = Random.Range(minPauseTime, maxPauseTime);
            yield return new WaitForSeconds(randomPause);
        }
    }

    public void TriggerVignetteEffect()
    {
        StartCoroutine(VignetteEffect());
    }

    private IEnumerator VignetteEffect()
    {
        if (_vignette == null) yield break;

        _vignette.enabled.Override(true);
        _vignette.intensity.Override(0.4f);

        yield return new WaitForSeconds(effectTime);

        float duration = 1f; // Adjust fade duration
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newIntensity = Mathf.Lerp(0.4f, 0f, elapsedTime / duration);
            _vignette.intensity.Override(newIntensity);
            yield return null;
        }
       
        _vignette.enabled.Override(false);
    }

}
