using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using System;

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
        Debug.Log($"[AnchorLoader] Anchors to load: {count}");

        for (int i = 0; i < count; i++)
        {
            string key = $"anchor_guid_{i}";
            if (!PlayerPrefs.HasKey(key))
            {
                Debug.LogWarning($"[AnchorLoader] Missing key: {key}");
                continue;
            }

            string guidString = PlayerPrefs.GetString(key);
            SerializableGuid guid = ParseGuidFromString(guidString);

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

    private SerializableGuid ParseGuidFromString(string guidString)
    {
        Guid guid = Guid.Parse(guidString);
        byte[] bytes = guid.ToByteArray();
        ulong low = BitConverter.ToUInt64(bytes, 0);
        ulong high = BitConverter.ToUInt64(bytes, 8);
        return new SerializableGuid(low, high);
    }
}