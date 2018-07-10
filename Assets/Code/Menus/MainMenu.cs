using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
	// THIS WILL BE THE MASTER CLASS TO HANDLE ALL THE DIFFERENT MENU PAGES IN THE GAME
	[SerializeField] bool _debugMessages = false;

	void Awake()
	{
		PhotonNetwork.sendRate = 64;
		PhotonNetwork.sendRateOnSerialize = 64;

		PhotonNetwork.automaticallySyncScene = true;

		PhotonNetwork.ConnectUsingSettings(Constants.GAME_VERSION);

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
