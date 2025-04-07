using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LightningStrikeEvent : MonoBehaviour
{
    [Header("Lightning Strike Settings")]
    [SerializeField] private SceneField targetScene;
    [SerializeField] private GameObject lightningEffectPrefab;

    public List<GameObject> placeableSpots = new List<GameObject>();
    private Dictionary<GameObject, GameObject> activeEffects = new Dictionary<GameObject, GameObject>();

    [Header("Events")]
    public UnityEvent<string, int> OnLightningStrike;

    private void Start()
    {
        StartCoroutine(InitializeLightningStrike());

        if (WaveManager.Instance != null)
        {
            HookWaveEvents();
        }
        else
        {
            StartCoroutine(WaitForWaveManager());
        }
    }

    private void OnDestroy()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete -= StopLightning;
    }

    private IEnumerator WaitForWaveManager()
    {
        while (WaveManager.Instance == null)
            yield return null;

        HookWaveEvents();
    }

    private void HookWaveEvents()
    {
        WaveManager.Instance.OnWaveComplete += StopLightning;
        StartCoroutine(WatchForWaveStart());
    }

    private IEnumerator InitializeLightningStrike()
    {
        yield return new WaitForSeconds(1f);

        if (SceneManager.GetActiveScene().name != targetScene) yield break;

        GameObject[] placeholders = GameObject.FindGameObjectsWithTag("Placeable");
        placeableSpots.AddRange(placeholders);
    }

    private IEnumerator WatchForWaveStart()
    {
        bool wasSpawning = false;

        while (true)
        {
            if (WaveManager.Instance != null)
            {
                bool isSpawning = typeof(WaveManager).GetField("_isSpawning", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(WaveManager.Instance) as bool? ?? false;

                if (isSpawning && !wasSpawning)
                {
                    StartLightning();
                }

                wasSpawning = isSpawning;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void StartLightning()
    {
        if (placeableSpots.Count == 0) return;

        GameObject targetSpot = placeableSpots[Random.Range(0, placeableSpots.Count)];

        if (targetSpot.TryGetComponent(out Collider col))
            col.enabled = false;

        GameObject lightningEffect = Instantiate(lightningEffectPrefab, targetSpot.transform.position + Vector3.up * 1.5f, Quaternion.Euler(-90f, 0f, 0f));
        AudioManager.Instance.PlaySoundEffect("LightningStrike_SFX");

        if (targetSpot.TryGetComponent(out PlaceholderID pathInfo))
            OnLightningStrike?.Invoke(targetSpot.name, pathInfo.placeholderID);

        activeEffects[targetSpot] = lightningEffect;
    }

    private void StopLightning()
    {
        foreach (var pair in activeEffects)
        {
            if (pair.Key.TryGetComponent(out Collider col))
                col.enabled = true;

            Destroy(pair.Value);
        }

        activeEffects.Clear();
    }
}
