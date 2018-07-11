using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// THIS CLASS IS USED TO START A GAME DIRECTLY FROM THE MAIN SCENE
// EVERYONE WHO STARTS THE GAME FROM THIS SCENE WILL JOIN THE SAME ROOM AND GAME
public class SimpleNetworkStarter : Photon.MonoBehaviour
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

		Match.instance.level.ManualStart();

		if (PhotonNetwork.isMasterClient)
			FindObjectOfType<CollisionTracker>().ManualStart();
    }

    public static float CalculateNetDelta(double inTimestamp) =>
        (float)(PhotonNetwork.time - inTimestamp);
	
}
