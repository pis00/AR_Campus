using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AnchorSaver : MonoBehaviour
{
    [Header("AR Foundation References")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARAnchorManager anchorManager;

    [Header("Prefab to Instantiate (Optional)")]
    [SerializeField] private GameObject visualPrefab;

    private static readonly List<ARRaycastHit> hits = new();
    private int anchorIndex = 0;

    private void Awake()
    {
        // Auto-assign if not manually set (using updated Unity API)
        if (raycastManager == null)
            raycastManager = FindFirstObjectByType<ARRaycastManager>();
        if (anchorManager == null)
            anchorManager = FindFirstObjectByType<ARAnchorManager>();
    }

    private void Start()
    {
        anchorIndex = PlayerPrefs.GetInt("anchor_count", 0);
        Debug.Log($"[AnchorSaver] Loaded anchor index: {anchorIndex}");
    }

private void Update()
{

    if (Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);
        Debug.Log($"[DEBUG] Touch detected at: {touch.position}, phase: {touch.phase}");
    }
}

    private async void SaveAnchorAsync(ARPlane plane, Pose pose)
    {
        Debug.Log("[AnchorSaver] Attempting to attach anchor...");

        ARAnchor anchor = anchorManager.AttachAnchor(plane, pose);
        if (anchor == null)
        {
            Debug.LogError("[AnchorSaver] Failed to attach anchor to plane.");
            return;
        }

        Debug.Log($"[AnchorSaver] Anchor created at: {anchor.transform.position}");

        var result = await anchorManager.TrySaveAnchorAsync(anchor);
        Debug.Log($"[AnchorSaver] TrySaveAnchorAsync result: {result.status}");

        if (result.status.ToString() == "Success")
        {
            SerializableGuid guid = result.value;
            PlayerPrefs.SetString($"anchor_guid_{anchorIndex}", guid.ToString());
            anchorIndex++;
            PlayerPrefs.SetInt("anchor_count", anchorIndex);
            PlayerPrefs.Save();

            Debug.Log($"[AnchorSaver] Anchor #{anchorIndex - 1} saved. GUID: {guid}");

            if (visualPrefab != null)
            {
                Instantiate(visualPrefab, anchor.transform.position, anchor.transform.rotation);
                Debug.Log("[AnchorSaver] Visual prefab instantiated at anchor.");
            }
        }
        else
        {
            Debug.LogWarning($"[AnchorSaver] Anchor save failed: {result.status}");
        }
    }
}