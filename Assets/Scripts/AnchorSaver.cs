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

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPosition = Input.GetTouch(0).position;

            if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                var hit = hits[0];
                Pose pose = hit.pose;

                ARPlane plane = hit.trackable as ARPlane;
                if (plane != null)
                {
                    SaveAnchorAsync(plane, pose);
                }
                else
                {
                    Debug.LogWarning("Trackable is not an ARPlane.");
                }
            }
        }
    }

    private async void SaveAnchorAsync(ARPlane plane, Pose pose)
    {
        ARAnchor anchor = anchorManager.AttachAnchor(plane, pose);
        if (anchor == null)
        {
            Debug.LogError("Failed to create anchor.");
            return;
        }

        var result = await anchorManager.TrySaveAnchorAsync(anchor);
        if (result.status.ToString() == "Success")
        {
            SerializableGuid guid = result.value;
            Debug.Log($"Anchor saved successfully. GUID: {guid}");

            PlayerPrefs.SetString($"anchor_guid_{anchorIndex}", guid.ToString());
            PlayerPrefs.Save();
            anchorIndex++;
        }
        else
        {
            Debug.LogWarning($"Anchor saving failed. Status: {result.status}");
        }
    }
}