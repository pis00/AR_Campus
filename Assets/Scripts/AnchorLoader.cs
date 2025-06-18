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
        Debug.Log($"[AnchorLoader] Waiting {delaySeconds} seconds before loading anchors...");
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
            SerializableGuid guid;

            try
            {
                guid = ParseGuidFromString(guidString);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AnchorLoader] Failed to parse GUID '{guidString}' - {ex.Message}");
                continue;
            }

            yield return LoadAndPlaceAnchor(guid, i);
        }
    }

    private async System.Threading.Tasks.Task LoadAndPlaceAnchor(SerializableGuid guid, int index)
    {
        Debug.Log($"[AnchorLoader] Trying to load anchor {index}: {guid}");

        var result = await anchorManager.TryLoadAnchorAsync(guid);
        if (result.status.ToString() == "Success")
        {
            ARAnchor anchor = result.value;

            if (prefab != null)
            {
                GameObject visual = Instantiate(prefab, anchor.transform.position, Quaternion.identity);

                Vector3 camForward = Camera.main.transform.forward;
                camForward.y = 0;
                if (camForward != Vector3.zero)
                    visual.transform.rotation = Quaternion.LookRotation(camForward);
            }

            Debug.Log($"✅ [AnchorLoader] Anchor {index} loaded at {anchor.transform.position}");
        }
        else
        {
            Debug.LogWarning($"❌ [AnchorLoader] Failed to load anchor {index}. Status: {result.status}");
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