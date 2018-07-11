using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match : Photon.MonoBehaviour
{
	public static Match instance { get; private set; }

	public Level level           { get; private set; }
	public PlayerData playerData { get; private set; }

	void Awake()
	{
		instance = this;		
	}

	void Start()
	{
		playerData = FindObjectOfType<PlayerData>();
		level      = FindObjectOfType<Level>();

		if(FindObjectOfType<SimpleNetworkStarter>() == null)
			StartOnAllLoaded();
	}

	void OnDestroy()
	{
		instance = null;	
	}

	void StartOnAllLoaded()
	{
		if (PhotonNetwork.isMasterClient)
			photonView.RPC("NetworkStartGame", PhotonTargets.AllViaServer);
	}

	[PunRPC]
	void NetworkStartGame()
	{
		FindObjectOfType<CollisionTracker>().ManualStart();
		level.ManualStart();
	}

}
