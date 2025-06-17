using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Threading.Tasks;

public class AnchorLoader : MonoBehaviour
{
    [SerializeField] private ARAnchorManager anchorManager;
    [SerializeField] private GameObject prefab;

    private async void Start()
    {
        int count = PlayerPrefs.GetInt("anchor_count", 0);

        for (int i = 0; i < count; i++)
        {
            string keyLow = $"anchor_guid_low_{i}";
            string keyHigh = $"anchor_guid_high_{i}";

            if (!PlayerPrefs.HasKey(keyLow) || !PlayerPrefs.HasKey(keyHigh))
                continue;

            ulong low = ulong.Parse(PlayerPrefs.GetString(keyLow));
            ulong high = ulong.Parse(PlayerPrefs.GetString(keyHigh));
            SerializableGuid guid = new SerializableGuid(low, high);

            var result = await anchorManager.TryLoadAnchorAsync(guid);
            if (result.status.ToString() == "Success")
            {
                ARAnchor anchor = result.value;
                Instantiate(prefab, anchor.transform);
                Debug.Log($"Loaded anchor #{i} successfully.");
            }
            else
            {
                Debug.LogWarning($"Failed to load anchor #{i}.");
            }
        }
    }
}