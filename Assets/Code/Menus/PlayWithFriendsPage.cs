using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayWithFriendsPage : MenuPage
{		
	[SerializeField] Text _roomNameText;
	[SerializeField] Text _joinRoomInput;
	[SerializeField] Text _numJoinedPlayersText;

	[SerializeField] Button _continueButton;
	
	public void HostRoom()
	{
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.IsVisible = false;
		roomOptions.MaxPlayers = 4;

		// generate a random name, if we let photon create a name for us its about 100 characters long, works untill we intergrate steam
		string roomName = Random.Range(1000, 50000).ToString();

		PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
	}

	public void JoinRoom()
	{
		if (!PhotonNetwork.isMasterClient)
		   PhotonNetwork.JoinRoom(_joinRoomInput.text);
	}

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
		
		PhotonNetwork.player.SetCustomProperties(new Hashtable(3));
	}

	void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
	{
		// send notification if anyone disconnects
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

		_numJoinedPlayersText.text = string.Format("{0}/4", PhotonNetwork.room.PlayerCount.ToString());

		if (PhotonNetwork.isMasterClient && PhotonNetwork.room.PlayerCount > 1)
			_continueButton.interactable = true;
		else
			_continueButton.interactable = false;

	}

	public override void OnPageExit()
	{
		
	}
}
