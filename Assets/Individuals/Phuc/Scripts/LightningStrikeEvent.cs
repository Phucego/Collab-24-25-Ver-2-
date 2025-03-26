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
    [SerializeField] private float strikeInterval = 10f;
    [SerializeField] private float effectDuration = 3f;

    public List<GameObject> placeableSpots = new List<GameObject>();

    [Header("Events")]
    public UnityEvent<string, int> OnLightningStrike; // Sends both placeholder name & path ID

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1f); // Small delay to ensure everything initializes
        if (SceneManager.GetActiveScene().name != targetScene) yield break;

        GameObject[] placeholders = GameObject.FindGameObjectsWithTag("Placeable");
        placeableSpots.AddRange(placeholders);

        StartCoroutine(LightningStrikeRoutine());
    }


    private IEnumerator LightningStrikeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(strikeInterval);

            if (GameStatesManager.Instance.GetCurrentState() == GameStates.Pause || 
                (DialogueDisplay.instance != null && DialogueDisplay.instance.isDialogueActive)) 
            {
                continue;
            }


            TriggerLightningStrike();
        }
    }

    private void TriggerLightningStrike()
    {
        if (placeableSpots.Count == 0) return;

        GameObject targetSpot = placeableSpots[Random.Range(0, placeableSpots.Count)];

        // Identify path ID
        PlaceholderID pathInfo = targetSpot.GetComponent<PlaceholderID>();
        int pathID = pathInfo != null ? pathInfo.placeholderID : -1; // Default to -1 if not found

        // Disable tower placement
        targetSpot.GetComponent<Collider>().enabled = false;

        // Spawn lightning effect with 90-degree X-axis rotation
        GameObject lightningEffect = Instantiate(lightningEffectPrefab, targetSpot.transform.position + Vector3.up * 1.5f, Quaternion.Euler(-90f, 0f, 0f));

        // Play sound effect
        AudioManager.Instance.PlaySoundEffect("LightningStrike_SFX");

        // Notify UI with placeholder & path ID
        OnLightningStrike?.Invoke(targetSpot.name, pathID);

        StartCoroutine(ResetPlaceholder(targetSpot, lightningEffect));
    }

    private IEnumerator ResetPlaceholder(GameObject placeholder, GameObject effect)
    {
        yield return new WaitForSeconds(effectDuration);

        placeholder.GetComponent<Collider>().enabled = true;
        Destroy(effect);
    }
}
