using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
	// ALL CODE HERE IS TEMP FOR THE MOMENT, JUST TO TRY TO SYNC LEVEL LOADING ON ALL PEOPLE IN A ROOM FROM MENU SCENE
	// THIS WILL BE THE MASTER CLASS TO HANDLE ALL THE DIFFERENT MENU PAGES IN THE GAME
	[SerializeField] bool _debugMessages = false;

	void Awake()
	{
		PhotonNetwork.sendRate = 64;
		PhotonNetwork.sendRateOnSerialize = 64;

		PhotonNetwork.automaticallySyncScene = true;

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

		Debug.LogFormat("Num players connected to room {0}", PhotonNetwork.playerList.Length);
	}

	void Update()
	{		
		if (Input.GetKeyDown(KeyCode.M) && (PhotonNetwork.isMasterClient))
		{
			PhotonNetwork.automaticallySyncScene = true;
			PhotonNetwork.LoadLevel("Level1");
		}
	}

}
