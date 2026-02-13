using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    [Header("Configuration")]
    public string roomNamePrefix = "HorrorCabin_";
    public string gameSceneName = "ForestCutscene";

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    private Vector2 scrollPos;

    // NEW: Controls visibility of the GUI
    public bool showRoomList = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {

            Destroy(gameObject);
        }
        else
        {

            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name)) cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }
    }

    // NEW: Call this from MainMenu to open the list
    public void OpenRoomBrowser()
    {
        showRoomList = true;
    }

    private void OnGUI()
    {
        // 1. Only show if the flag is TRUE and we are NOT in a room yet
        if (!showRoomList || PhotonNetwork.InRoom) return;

        // Background box
        GUI.Box(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 250, 400, 500), "Room Browser");

        GUILayout.BeginArea(new Rect(Screen.width / 2 - 180, Screen.height / 2 - 220, 360, 440));

        // Close Button
        if (GUILayout.Button("Close / Back", GUILayout.Height(30)))
        {
            showRoomList = false;
        }

        GUILayout.Space(10);
        GUILayout.Label($"Available Rooms: {cachedRoomList.Count}");

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        if (cachedRoomList.Count == 0)
        {
            GUILayout.Label("No rooms created yet...");
        }
        else
        {
            foreach (var roomInfo in cachedRoomList.Values)
            {
                string buttonText = $"{roomInfo.Name} [{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}]";
                if (GUILayout.Button(buttonText, GUILayout.Height(40)))
                {
                    JoinSpecificRoom(roomInfo.Name);
                }
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    public void CreateRoom()
    {
        RoomOptions options = new RoomOptions() { MaxPlayers = 10, IsVisible = true, IsOpen = true };
        PhotonNetwork.CreateRoom(roomNamePrefix + Random.Range(1000, 9999), options);
    }

    public void JoinSpecificRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        showRoomList = false; // Hide GUI
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);

        IntroOutroInfoHolder.stageScene = Stage.First;

        // Only Host loads the level, but they do it via the Fader now
        if (PhotonNetwork.IsMasterClient)
        {
            // Trigger the visual fade, which THEN calls PhotonNetwork.LoadLevel
            ScreenFader.Instance.FadeOutToNetworkScene(gameSceneName);
        }
    }

    private void OnApplicationQuit()
    {
        Photon.Pun.PhotonNetwork.Disconnect();
    }
}