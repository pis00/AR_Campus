using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
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

    private InputAction touchAction;

    private void Awake()
    {
        if (raycastManager == null)
            raycastManager = FindFirstObjectByType<ARRaycastManager>();
        if (anchorManager == null)
            anchorManager = FindFirstObjectByType<ARAnchorManager>();

        // Set up InputAction to detect taps
        touchAction = new InputAction(type: InputActionType.PassThrough, binding: "<Touchscreen>/primaryTouch/press");
        touchAction.performed += OnTouchPressed;
    }

    private void OnEnable()
    {
        touchAction.Enable();
        anchorIndex = PlayerPrefs.GetInt("anchor_count", 0);
        Debug.Log($"[AnchorSaver] Loaded anchor index: {anchorIndex}");
    }

    private void OnDisable()
    {
        touchAction.Disable();
    }

    private void OnTouchPressed(InputAction.CallbackContext context)
    {
        Vector2 screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        Debug.Log($"[InputSystem] Tap at screen position: {screenPosition}");

        if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;
            ARPlane plane = hits[0].trackable as ARPlane;

            Debug.Log($"[Raycast] Hit plane at: {pose.position}");

            if (plane != null)
            {
                Debug.Log("[AnchorSaver] Valid plane found. Saving anchor...");
                SaveAnchorAsync(plane, pose);
            }
            else
            {
                Debug.LogWarning("[AnchorSaver] Trackable is not an ARPlane.");
            }
        }
        else
        {
            Debug.LogWarning("[Raycast] No AR plane hit at tap point.");
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