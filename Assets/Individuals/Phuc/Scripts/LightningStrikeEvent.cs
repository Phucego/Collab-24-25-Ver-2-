using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LightningStrikeEvent : MonoBehaviour
{
    [Header("Lightning Strike Settings")]
    [SerializeField] private SceneField targetScene; // Specify the scene where the event occurs
    [SerializeField] private GameObject lightningEffectPrefab;
    [SerializeField] private float strikeInterval = 10f;
    [SerializeField] private float effectDuration = 3f;

    public List<GameObject> placeableSpots = new List<GameObject>();

    private void Start()
    {
        // Check if the current scene matches the target scene
        if (SceneManager.GetActiveScene().name != targetScene)
        {
            Debug.Log("Lightning Strike Event is disabled in this scene.");
            return;
        }

        // Find all placeholders with the "Placeable" tag
        GameObject[] placeholders = GameObject.FindGameObjectsWithTag("Placeable");
        placeableSpots.AddRange(placeholders);

        // Start the lightning strike coroutine
        StartCoroutine(LightningStrikeRoutine());
    }

    private IEnumerator LightningStrikeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(strikeInterval);
            TriggerLightningStrike();
        }
    }

    private void TriggerLightningStrike()
    {
        if (placeableSpots.Count == 0) return;

        // Pick a random placeholder
        GameObject targetSpot = placeableSpots[Random.Range(0, placeableSpots.Count)];

        // Disable tower placement
        targetSpot.GetComponent<Collider>().enabled = false;

        // Spawn lightning effect
        GameObject lightningEffect = Instantiate(lightningEffectPrefab, targetSpot.transform.position + Vector3.up * 5f, Quaternion.identity);

            //ENABLE THIS LATER
        //AudioManager.Instance.PlaySoundEffect("LightningStrike_SFX");
        

        // Re-enable placement after effectDuration
        StartCoroutine(ResetPlaceholder(targetSpot, lightningEffect));
    }

    private IEnumerator ResetPlaceholder(GameObject placeholder, GameObject effect)
    {
        yield return new WaitForSeconds(effectDuration);

        // Re-enable tower placement
        placeholder.GetComponent<Collider>().enabled = true;

        // Destroy lightning effect
        Destroy(effect);
    }
}
