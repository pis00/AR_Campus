using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AnchorSaver : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARAnchorManager anchorManager;

    private static List<ARRaycastHit> hits = new();
    private int anchorIndex = 0;

    private void Start()
    {
        anchorIndex = PlayerPrefs.GetInt("anchor_count", 0);
        Debug.Log($"[AnchorSaver] Loaded anchor index: {anchorIndex}");
    }

    private void Update()
    {

        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);
        Debug.Log($"[Update] Touch detected. Phase: {touch.phase}");

        if (touch.phase != TouchPhase.Began)
            return;

        Vector2 touchPosition = touch.position;
        Debug.Log($"[Touch] Touched at screen position: {touchPosition}");

        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;
            ARPlane plane = hits[0].trackable as ARPlane;

            Debug.Log($"[Raycast] Hit at world position: {pose.position}");

            if (plane != null)
            {
                Debug.Log("[Raycast] Plane is valid. Creating anchor...");
                SaveAnchorAsync(plane, pose);
            }
            else
            {
                Debug.LogWarning("[AnchorSaver] Hit is NOT an ARPlane.");
            }
        }
        else
        {
            Debug.LogWarning("[Raycast] No AR plane hit.");
        }
    }

    private async void SaveAnchorAsync(ARPlane plane, Pose pose)
    {
        Debug.Log("[AnchorSaver] Trying to attach anchor...");

        ARAnchor anchor = anchorManager.AttachAnchor(plane, pose);
        if (anchor == null)
        {
            Debug.LogError("[AnchorSaver] Failed to attach anchor.");
            return;
        }

        Debug.Log($"[AnchorSaver] Anchor created at position: {anchor.transform.position}");

        var result = await anchorManager.TrySaveAnchorAsync(anchor);
        Debug.Log($"[AnchorSaver] Save result: {result.status}");

        if (result.status.ToString() == "Success")
        {
            SerializableGuid guid = result.value;
            PlayerPrefs.SetString($"anchor_guid_{anchorIndex}", guid.ToString());
            anchorIndex++;
            PlayerPrefs.SetInt("anchor_count", anchorIndex);
            PlayerPrefs.Save();

            Debug.Log($"[AnchorSaver] Anchor #{anchorIndex - 1} saved. GUID: {guid}");
        }
        else
        {
            Debug.LogWarning($"[AnchorSaver] Save failed. Status: {result.status}");
        }
    }
}