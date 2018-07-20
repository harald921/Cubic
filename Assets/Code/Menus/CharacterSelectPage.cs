
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class CharacterSelectPage : MenuPage
{
	int _playersReady;

	[SerializeField] Button[] _characterButtons;
	[SerializeField] Text _numJoinedPlayersText;

	public void OnCharacterSelected(string name)
	{
		for (int i = 0; i < _characterButtons.Length; i++)
			_characterButtons[i].interactable = false;

		PhotonNetwork.player.NickName = name; // set nickname to the name of the character for now (this will store the steam nick later instead)

		// set character chosen so we can spawn it when the game starts
		Hashtable p = PhotonNetwork.player.CustomProperties;
		p.Add(Constants.CHARACTER_NAME, name);
		PhotonNetwork.player.SetCustomProperties(p);

		// tell server that we are selected and ready
		photonView.RPC("AddPlayerReady", PhotonTargets.MasterClient);
	}

	[PunRPC]
	void SetSpawnPointFromPlayerID(int id, int spawnPoint)
	{
		// is this my PlayerID
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
		if (PhotonNetwork.room == null)
			return;

		_numJoinedPlayersText.text = string.Format("{0}/4", PhotonNetwork.room.PlayerCount.ToString());
	}

	[PunRPC]
	void AddPlayerReady()
	{
		_playersReady++;
		if (_playersReady == PhotonNetwork.room.PlayerCount)
		{
			// loop over all players in room and give them a spawnpoint based on order in list
			// send an rpc to all players with the PlayerID and they will check if the PlayerID correspond to thier own, and if so they will set thier spawnpoint
			// this can not be done locally becuase the elements in photonnetwork.PlayerList can be in different order on every client
			for (int i =0; i < PhotonNetwork.room.PlayerCount; i++)			
				photonView.RPC("SetSpawnPointFromPlayerID", PhotonTargets.All, PhotonNetwork.playerList[i].ID, i);
			
			PhotonNetwork.LoadLevel(PhotonNetwork.player.CustomProperties[Constants.LEVEL_NAME].ToString());
		}

	}

	
}
