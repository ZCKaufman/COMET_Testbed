using UnityEngine;
using TMPro;
using Photon.Pun;
using ExitGames.Client.Photon;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;


public class ConfigButtonHandler : MonoBehaviour
{
    public TMP_Dropdown dropdown;

    public void OnLoadConfigClicked()
    {
        string selectedFile = dropdown.options[dropdown.value].text;

        if (selectedFile == "-- Select Configuration --")
        {
            Debug.LogWarning("Please select a valid configuration.");
            return;
        }

        // Master client sets shared config property only
        PhotonHashtable props = new PhotonHashtable
        {
            { "ConfigFile", selectedFile }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

}