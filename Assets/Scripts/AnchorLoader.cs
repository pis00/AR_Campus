using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using System;
using System.Threading.Tasks;

public class AnchorLoader : MonoBehaviour
{
    [SerializeField] private ARAnchorManager anchorManager;
    [SerializeField] private GameObject prefab;
    [SerializeField] private float loadDelaySeconds = 3f;

    private void Start()
    {
        StartCoroutine(LoadAnchorsWithDelay(loadDelaySeconds));
    }

    private IEnumerator LoadAnchorsWithDelay(float delaySeconds)
    {
        Debug.Log($"[AnchorLoader] Waiting {delaySeconds} seconds...");
        yield return new WaitForSeconds(delaySeconds);
        yield return WaitForStableTracking();

        LoadAnchorsNow();
    }

    private IEnumerator WaitForStableTracking()
    {
        ARSession session = FindFirstObjectByType<ARSession>();
        Debug.Log("[AnchorLoader] Waiting for AR session tracking...");
        while (session == null || ARSession.notTrackingReason != NotTrackingReason.None)
        {
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("[AnchorLoader] AR session tracking is stable.");
    }

    private void LoadAnchorsNow()
    {
        int count = PlayerPrefs.GetInt("anchor_count", 0);
        Debug.Log($"[AnchorLoader] Attempting to load {count} anchors...");

        for (int i = 0; i < count; i++)
        {
            string guidKey = $"anchor_guid_{i}";
            if (!PlayerPrefs.HasKey(guidKey))
            {
                Debug.LogWarning($"[AnchorLoader] Missing GUID for anchor {i}");
                continue;
            }

            string guidString = PlayerPrefs.GetString(guidKey);
            SerializableGuid guid;

            try
            {
                guid = ParseGuidFromString(guidString);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AnchorLoader] GUID parse failed: {guidString}, Exception: {ex.Message}");
                continue;
            }

            LoadAndPlaceAnchor(guid, i);
        }
    }

    private async void LoadAndPlaceAnchor(SerializableGuid guid, int index)
    {
        Debug.Log($"[AnchorLoader] Loading anchor {index} with GUID: {guid}");

        var result = await anchorManager.TryLoadAnchorAsync(guid);
        Debug.Log($"[AnchorLoader] Result status: {result.status}");

        if (result.status.ToString() == "Success")
        {
            ARAnchor anchor = result.value;

            if (prefab != null)
            {
                GameObject arrow = Instantiate(prefab, anchor.transform.position, Quaternion.identity);
                Vector3 camForward = Camera.main.transform.forward;
                camForward.y = 0;
                if (camForward != Vector3.zero)
                    arrow.transform.rotation = Quaternion.LookRotation(camForward);
            }

            Debug.Log($"✅ [AnchorLoader] Anchor {index} loaded at position: {anchor.transform.position}");
        }
        else
        {
            Debug.LogWarning($"❌ [AnchorLoader] Failed to load anchor {index}. Using fallback position.");

            Vector3 fallback = new Vector3(
                PlayerPrefs.GetFloat($"anchor_x_{index}", 0f),
                PlayerPrefs.GetFloat($"anchor_y_{index}", 0f),
                PlayerPrefs.GetFloat($"anchor_z_{index}", 0f)
            );

            if (prefab != null)
            {
                GameObject fallbackArrow = Instantiate(prefab, fallback, Quaternion.identity);
                Vector3 camForward = Camera.main.transform.forward;
                camForward.y = 0;
                if (camForward != Vector3.zero)
                    fallbackArrow.transform.rotation = Quaternion.LookRotation(camForward);
            }

            Debug.Log($"[AnchorLoader] Anchor {index} fallback instantiated at: {fallback}");
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