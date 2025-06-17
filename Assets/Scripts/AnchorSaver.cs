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
        // Load anchor count at startup
        anchorIndex = PlayerPrefs.GetInt("anchor_count", 0);
        Debug.Log($"[AnchorSaver] Loaded anchor index: {anchorIndex}");
    }

    private void Update()
    {
        if (Input.touchCount == 0 || Input.GetTouch(0).phase != TouchPhase.Began)
            return;

        Vector2 touchPosition = Input.GetTouch(0).position;

        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;
            ARPlane plane = hits[0].trackable as ARPlane;

            if (plane != null)
            {
                SaveAnchorAsync(plane, pose);
            }
            else
            {
                Debug.LogWarning("[AnchorSaver] Trackable is not an ARPlane.");
            }
        }
    }

    private async void SaveAnchorAsync(ARPlane plane, Pose pose)
    {
        ARAnchor anchor = anchorManager.AttachAnchor(plane, pose);
        if (anchor == null)
        {
            Debug.LogError("[AnchorSaver] Failed to create anchor.");
            return;
        }

        var result = await anchorManager.TrySaveAnchorAsync(anchor);
        if (result.status.ToString() == "Success")
        {
            SerializableGuid guid = result.value;

            // Save guid as string
            PlayerPrefs.SetString($"anchor_guid_{anchorIndex}", guid.ToString());
            anchorIndex++;
            PlayerPrefs.SetInt("anchor_count", anchorIndex);
            PlayerPrefs.Save();

            Debug.Log($"[AnchorSaver] Anchor #{anchorIndex - 1} saved. GUID: {guid}");
        }
        else
        {
            Debug.LogWarning($"[AnchorSaver] Anchor saving failed. Status: {result.status}");
        }
    }
}