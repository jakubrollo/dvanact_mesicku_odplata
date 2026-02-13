using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class MainMenu : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public TMP_InputField playerNameInput;
    public Button createRoomButton; // Drag your button here!
    public Button joinRoomButton;   // Drag your button here!
    public TextMeshProUGUI statusText; // Optional: To see what's happening

    void Start()
    {
        // 1. Lock buttons by default so you can't spam click
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
        if (statusText) statusText.text = "Connecting...";

        // 2. Check current status
        if (PhotonNetwork.IsConnectedAndReady)
        {
            OnConnectedToMaster();
        }
        else if (PhotonNetwork.InRoom)
        {
            // If we are stuck in a room, leave it!
            if (statusText) statusText.text = "Leaving old room...";
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            // Connect if not connected
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    // This runs automatically when Photon is ready
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master! Buttons unlocked.");
        if (statusText) statusText.text = "Ready to Play";
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (statusText) statusText.text = "Disconnected: " + cause;
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
    }

    public void SetPlayerName()
    {
        if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text))
        {
            PhotonNetwork.NickName = playerNameInput.text;
        }
        else
        {
            PhotonNetwork.NickName = "Survivor " + Random.Range(1, 100);
        }
    }

    public void CreateNewLobby()
    {
        SetPlayerName();
        // Safety Check
        if (!PhotonNetwork.IsConnectedAndReady) return;

        NetworkManager.Instance.CreateRoom();
    }

    public void JoinLobby()
    {
        SetPlayerName();
        if (!PhotonNetwork.IsConnectedAndReady) return;

        NetworkManager.Instance.OpenRoomBrowser();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}