
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
			PhotonNetwork.LoadLevel(PhotonNetwork.player.CustomProperties[Constants.LEVEL_NAME].ToString());

	}

	
}
