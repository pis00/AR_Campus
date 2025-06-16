using UnityEngine;

public class RoomPanelManager : MonoBehaviour
{
    public GameObject roomPanel;
    public GameObject arController;

    public void OpenARFromRoom()
    {
        // Hide the panel
        roomPanel.SetActive(false);

        // Activate the AR system
        arController.SetActive(true);
    }
}
