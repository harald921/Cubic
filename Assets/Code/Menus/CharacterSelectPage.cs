using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPage : MenuPage
{
	int _playersReady;

	[SerializeField] Button[] _characterButtons;

	public void OnCharacterSelected(string name)
	{
		for (int i = 0; i < _characterButtons.Length; i++)
			_characterButtons[i].interactable = false;

		FindObjectOfType<PlayerData>().character = name;

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
			PhotonNetwork.LoadLevel(FindObjectOfType<PlayerData>().level);

	}

	
}
