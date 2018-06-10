using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : Photon.MonoBehaviour
{
    [SerializeField] bool _debugMessages = false;

    void Awake()
    {
        PhotonNetwork.sendRate = 64;
        PhotonNetwork.sendRateOnSerialize = 64;

        ConnectToServer();
    }

    void ConnectToServer() =>
        PhotonNetwork.ConnectUsingSettings(Constants.GAME_VERSION);

    
    void OnConnectedToMaster() =>
        JoinRoom();

    void JoinRoom() =>
        PhotonNetwork.JoinOrCreateRoom("debugRoomName", new RoomOptions(), TypedLobby.Default);

    void OnCreatedRoom()
    {
        if (_debugMessages) Debug.Log("New room created");
    }

    void OnJoinedRoom()
    {
        if (_debugMessages) Debug.Log("Connected to room");
    }

    public static float CalculateNetDelta(double inTimestamp) =>
        (float)(PhotonNetwork.time - inTimestamp);
}
