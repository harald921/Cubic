﻿
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System;


public class CharacterSelectPage : MenuPage
{
	[Header("UI REFERENCES"), Space(2)]
	[SerializeField] MenuPlayerInfoUI _playerInfo;
	[SerializeField] Button[] _characterButtons;
	[SerializeField] Button _readyButton;
	[SerializeField] RectTransform _dotsParent;
	[SerializeField] Image _dotPrefab;
	[SerializeField] MessagePromt _promt;
	[SerializeField] Button _leftarrow;
	[SerializeField] Button _rightArrow;
	[SerializeField] StartCounterUI _counter;

	[Header("3D MODEL SETTINGS"),Space(2)]
	[SerializeField] Transform _modelTransform;
	[SerializeField] float _rotationSpeed = 1.0f;

	CharacterDatabase.ViewData _currentView;
	GameObject _currentViewObject;

	int _playersReady;
	Vector3 _rotation;
	int _numSkins;
	int _currentSkin;
	bool _imReady;

	public void OnCharacterSelected(string name)
	{
		if (_currentViewObject)
			Destroy(_currentViewObject);

		_currentView = CharacterDatabase.instance.GetViewFromName(name);

		_currentViewObject = Instantiate(_currentView.prefab, _modelTransform);

		_currentSkin = 0;
		_numSkins = _currentView.materials.Length;
		UpdateSkinDots();

		_playerInfo.photonView.RPC("UpdatePlayerUI", PhotonTargets.All, PhotonNetwork.player.ID, name);
	}

	public void OnReady()
	{
		if (_imReady)
			return;

		ChangeAllButtonsState(false);
		_imReady = true;

		// set nickname to the name of the character for now (this will store the steam nick later instead)
		PhotonNetwork.player.NickName = _currentView.name;

		// set character chosen so we can spawn it when the game starts
		Hashtable p = PhotonNetwork.player.CustomProperties;
		p.Add(Constants.CHARACTER_NAME, _currentView.name);
		p.Add(Constants.SKIN_ID, _currentSkin);
		PhotonNetwork.player.SetCustomProperties(p);

		// tell server that we are selected and ready
		_playerInfo.photonView.RPC("SetReadyUI", PhotonTargets.All, PhotonNetwork.player.ID, true);
		photonView.RPC("AddPlayerReady", PhotonTargets.MasterClient);
	}

	public void OnChangeSkin(bool increment)
	{
		if (_numSkins == 0)
			return;

		_dotsParent.transform.GetChild(_currentSkin).GetComponent<Image>().color = Color.white;

		if (increment)
		{
			_currentSkin++;
			if (_currentSkin == _numSkins)
				_currentSkin = 0;
		}
		else
		{
			_currentSkin--;
			if (_currentSkin < 0)
				_currentSkin = _numSkins - 1;
		}

		_dotsParent.transform.GetChild(_currentSkin).GetComponent<Image>().color = Color.green;

		Renderer renderer = _currentViewObject.GetComponent<Renderer>();
		if (renderer != null)
			renderer.material = _currentView.materials[_currentSkin];

		for (int i = 0; i < _currentViewObject.transform.childCount; i++)
		{
			renderer = _currentViewObject.transform.GetChild(i).GetComponent<Renderer>();
			if (renderer != null)
				renderer.material = _currentView.materials[_currentSkin];
		}
	}

	void UpdateSkinDots()
	{
		for (int i = 0; i < _dotsParent.transform.childCount; i++)
			Destroy(_dotsParent.GetChild(i).gameObject);

		float xPosition = 0;
		for(int i = 0; i < _numSkins; i++)
		{
			xPosition = i * 40;
			Image dot = Instantiate(_dotPrefab, _dotsParent);
			dot.GetComponent<RectTransform>().localPosition = new Vector3(xPosition, 0, 0);
			if (i == 0)
			   dot.GetComponent<Image>().color = Color.green;
		}	
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
		_currentView = CharacterDatabase.instance.GetFirstView();
		_currentViewObject = Instantiate(_currentView.prefab, _modelTransform);

		_playerInfo.photonView.RPC("UpdatePlayerUI", PhotonTargets.All, PhotonNetwork.player.ID, _currentView.name);

		_playersReady = 0;
		_currentSkin = 0;
		_numSkins = _currentView.materials.Length;
		UpdateSkinDots();

		if (PhotonNetwork.isMasterClient)
			photonView.RPC("StartCountdown", PhotonTargets.All, PhotonNetwork.time);
	}

	public override void OnPageExit()
	{
		ChangeAllButtonsState(true);
	}

	public override void UpdatePage()
	{
		_rotation += Vector3.up * _rotationSpeed * Time.deltaTime;
		if (_currentViewObject != null)
			_currentViewObject.transform.rotation = Quaternion.Euler(_rotation);		
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

	[PunRPC]
	void StartCountdown(double delta)
	{
		_counter.StartCount(delta, 20, () => OnReady());
	}

	public override void OnPlayerLeftRoom(PhotonPlayer player)
	{
		_playerInfo.DisableUIOfPlayer(player.ID);
		if (PhotonNetwork.room.PlayerCount == 1)
			_promt.SetAndShow("All other players have left the room!!\n\n Returning to menu!!!",
				() => {
					_playerInfo.DisableUIOfPlayer(PhotonNetwork.player.ID);
					if (_currentViewObject != null)
						Destroy(_currentViewObject);

					_counter.CancelCount();
					PhotonNetwork.RemovePlayerCustomProperties(null);
					PhotonNetwork.LeaveRoom();
					MainMenuSystem.instance.SetToPage("StartScreen");
				});
	}

	public void LeaveRoom()
	{
		if (_currentViewObject != null)
			Destroy(_currentViewObject);

		_counter.CancelCount();
		PhotonNetwork.RemovePlayerCustomProperties(null);
		_playerInfo.DisableAllPlayerUI();
		PhotonNetwork.LeaveRoom();
		MainMenuSystem.instance.SetToPage("StartScreen");
	}

	void ChangeAllButtonsState(bool enable)
	{
		for (int i = 0; i < _characterButtons.Length; i++)
			_characterButtons[i].interactable = enable;

		_readyButton.interactable = enable;
		_leftarrow.interactable   = enable;
		_rightArrow.interactable  = enable;
	}
}
