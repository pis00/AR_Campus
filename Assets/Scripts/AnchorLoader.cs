using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;

public class AnchorLoader : MonoBehaviour
{
    [SerializeField] private ARAnchorManager anchorManager;
    [SerializeField] private GameObject prefab;

    private void Start()
    {
        StartCoroutine(LoadAnchorsWithDelay(3f));
    }

    private IEnumerator LoadAnchorsWithDelay(float delaySeconds)
    {
        Debug.Log($"Waiting {delaySeconds} seconds before loading anchors...");
        yield return new WaitForSeconds(delaySeconds);

        int count = PlayerPrefs.GetInt("anchor_count", 0);

        for (int i = 0; i < count; i++)
        {
            string keyLow = $"anchor_guid_low_{i}";
            string keyHigh = $"anchor_guid_high_{i}";

            if (!PlayerPrefs.HasKey(keyLow) || !PlayerPrefs.HasKey(keyHigh))
            {
                Debug.LogWarning($"Missing keys for anchor {i}");
                continue;
            }

            ulong low = ulong.Parse(PlayerPrefs.GetString(keyLow));
            ulong high = ulong.Parse(PlayerPrefs.GetString(keyHigh));
            SerializableGuid guid = new SerializableGuid(low, high);

            // Launch the async load using a coroutine-friendly pattern
            yield return LoadAndPlaceAnchor(guid, i);
        }
    }

    private async System.Threading.Tasks.Task LoadAndPlaceAnchor(SerializableGuid guid, int index)
    {
        var result = await anchorManager.TryLoadAnchorAsync(guid);
        if (result.status.ToString() == "Success")
        {
            ARAnchor anchor = result.value;
            Instantiate(prefab, anchor.transform);
            Debug.Log($"✅ Anchor {index} loaded.");
        }
        else
        {
            Debug.LogWarning($"❌ Failed to load anchor {index}. Status: {result.status}");
        }
    }
}