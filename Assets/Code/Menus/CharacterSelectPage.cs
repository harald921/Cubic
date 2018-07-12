
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class CharacterSelectPage : MenuPage
{
	int _playersReady;

	[SerializeField] Button[] _characterButtons;

	public void OnCharacterSelected(string name)
	{
		for (int i = 0; i < _characterButtons.Length; i++)
			_characterButtons[i].interactable = false;
		
		Hashtable p = PhotonNetwork.player.CustomProperties;
		p.Add(Constants.CHARACTER_NAME, name);
		PhotonNetwork.player.SetCustomProperties(p);

		photonView.RPC("AddPlayerReady", PhotonTargets.MasterClient);
	}

	[PunRPC]
	void SetSpawnDataToPlayerID(int id, int spawnPoint)
	{
		if(PhotonNetwork.player.ID == id)
		{
			Hashtable p = PhotonNetwork.player.CustomProperties;
			p.Add(Constants.SPAWN_ID, spawnPoint);
			PhotonNetwork.player.SetCustomProperties(p);
		}		
	}

	public override void OnPageEnter()
	{		
	}

	public override void OnPageExit()
	{		
	}

	public override void UpdatePage()
	{		
	}

	[PunRPC]
	void AddPlayerReady()
	{
		_playersReady++;
		if (_playersReady == PhotonNetwork.room.PlayerCount)
		{
			for (int i =0; i < PhotonNetwork.room.PlayerCount; i++)			
				photonView.RPC("SetSpawnDataToPlayerID", PhotonTargets.All, PhotonNetwork.playerList[i].ID, i);
			
			PhotonNetwork.LoadLevel(PhotonNetwork.player.CustomProperties[Constants.LEVEL_NAME].ToString());
		}

	}

	
}
