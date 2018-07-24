using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MenuPlayerInfoUI : Photon.MonoBehaviour
{
	[Serializable]
	public struct PlayerInfo
	{
		public GameObject content;		
		public Text nickName;
		public Image checkMark;

		[HideInInspector] public int ownerID;
		[HideInInspector] public bool taken;
		[HideInInspector] public int index;
	}

	[SerializeField] PlayerInfo[] _players;

	[PunRPC]
	void ClaimUIBox(int ID, string nickName, string CharacterName)
	{
		for (int i = 0; i < 4; i++)
			if (!_players[i].taken)
			{
				_players[i].content.SetActive(true);
				_players[i].ownerID = ID;
				_players[i].taken = true;
				_players[i].nickName.text = nickName;				
				_players[i].index = i;
				return;
			}
	}

	[PunRPC]
	public void DisableUIOfPlayer(int ID)
	{
		for (int i = 0; i < 4; i++)
			if (_players[i].ownerID == ID)
			{
				_players[i].content.SetActive(false);
				_players[i].ownerID = -99;
				_players[i].taken = false;
				_players[i].nickName.text = "";				
				_players[i].checkMark.color = new Color(1, 1, 1, 0.1f);
				return;
			}
	}

	[PunRPC]
	void UpdatePlayerUI(int ID, string characterName)
	{
		for (int i = 0; i < 4; i++)
			if (_players[i].ownerID == ID)
			{
				// will probably uppdate a 2d icon here later
				return;
			}
	}

	[PunRPC]
	void SetReadyUI(int ID, bool active)
	{
		for (int i = 0; i < 4; i++)
			if (_players[i].ownerID == ID)
			{
				_players[i].checkMark.color = Color.white;
				return;
			}
	}

	public void DisableAllPlayerUI()
	{
		for (int i = 0; i < 4; i++)			
		{
			_players[i].content.SetActive(false);
			_players[i].ownerID = -99;
			_players[i].taken = false;
			_players[i].nickName.text = "";			
			_players[i].checkMark.color = new Color(1, 1, 1, 0.1f);
		}
	}

	public int GetModelTransformIndexFromID(int ID)
	{
		for (int i = 0; i < 4; i++)
			if (_players[i].ownerID == ID)
				return i;

		return 0;
	}

}
