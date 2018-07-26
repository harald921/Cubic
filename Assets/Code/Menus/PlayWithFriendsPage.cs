﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayWithFriendsPage : MenuPage
{
	[SerializeField] MenuPlayerInfoUI _playerInfo;

	[SerializeField] Text _roomNameText;
	[SerializeField] Text _joinRoomInput;

	[SerializeField] Button _continueButton;
	
	public void HostRoom()
	{
		// create a private room that can only be joined from invite
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.IsVisible = false;
		roomOptions.MaxPlayers = 4;
		

		// generate a random name, if we let photon create a name for us its about 100 characters long, works untill we intergrate steam
		string roomName = Random.Range(1000, 50000).ToString();

		PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
	}

	// called from button
	public void JoinRoom()
	{
		if (!PhotonNetwork.isMasterClient)
		   PhotonNetwork.JoinRoom(_joinRoomInput.text);
	}

	// called from button on server
	public void GoToLevelSelect()
	{
		PhotonNetwork.room.IsOpen = false;
		photonView.RPC("ContinueToLevelselect", PhotonTargets.All);
	}

	void OnCreatedRoom()
	{
		_roomNameText.text = PhotonNetwork.room.Name + " As Host";
	}

	void OnJoinedRoom()
	{
		if (!PhotonNetwork.isMasterClient)
		   _roomNameText.text = PhotonNetwork.room.Name + " As Client";
				
		_playerInfo.photonView.RPC("ClaimUIBox", PhotonTargets.AllBufferedViaServer, PhotonNetwork.player.ID, "SteamNick", "??????????");
	}
	
	[PunRPC]
	void ContinueToLevelselect()
	{
		MainMenuSystem.instance.SetToPage("LevelSelectScreen");
	}

	public override void OnPageEnter()
	{
		_continueButton.interactable = false;
	}

	public override void UpdatePage()
	{
		if (PhotonNetwork.room == null)
			return;

		// when more then two players in room the host can chose to continue to next screen
		if (PhotonNetwork.isMasterClient && PhotonNetwork.room.PlayerCount > 1)
			_continueButton.interactable = true;
		else
			_continueButton.interactable = false;

	}

	public override void OnPageExit()
	{
		_roomNameText.text = "Not Connected";
	}

	public override void OnPlayerLeftRoom(PhotonPlayer player)
	{
		_playerInfo.DisableUIOfPlayer(player.ID);
	}

	public void LeaveRoom()
	{
		// if yet not in room just return to main menu
		if(PhotonNetwork.room == null)
		{
			MainMenuSystem.instance.SetToPage("StartScreen");
			return;
		}

		// if connected to room, unclaim UI and leave room before we return to main menu
		_playerInfo.DisableAllPlayerUI();
		PhotonNetwork.LeaveRoom();
		MainMenuSystem.instance.SetToPage("StartScreen");
	}
}
