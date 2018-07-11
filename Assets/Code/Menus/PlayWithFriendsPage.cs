using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;

public class PlayWithFriendsPage : Photon.MonoBehaviour
{
	[SerializeField] GameObject _content;
	[SerializeField] GameObject _levelScreen;

	[SerializeField] Text _roomNameText;
	[SerializeField] Text _joinRoomInput;
	[SerializeField] Text _numJoinedPlayersText;

	[SerializeField] Button _continueButton;

	[SerializeField] LevelSelectPage _lvlselect;

	void Awake()
	{
		_continueButton.interactable = false;	
	}

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

		GameObject playerData = new GameObject("PlayerData", typeof(PlayerData));
		playerData.GetComponent<PlayerData>().playerId = PhotonNetwork.room.PlayerCount - 1;
		DontDestroyOnLoad(playerData);

		photonView.RPC("UpdateNumPlayersInRoom", PhotonTargets.All, PhotonNetwork.room.PlayerCount);
	}

	void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
	{
		photonView.RPC("UpdateNumPlayersInRoom", PhotonTargets.All, PhotonNetwork.room.PlayerCount);
	}

	[PunRPC]
	void UpdateNumPlayersInRoom(int numPlayers)
	{
		_numJoinedPlayersText.text = string.Format("{0}/4", PhotonNetwork.room.PlayerCount.ToString());

		if (PhotonNetwork.isMasterClient && PhotonNetwork.room.PlayerCount > 1)
			_continueButton.interactable = true;
	}

	[PunRPC]
	void ContinueToLevelselect()
	{
		_levelScreen.SetActive(true);
		_content.SetActive(false);
		_lvlselect.OnEnterPage();
	}



	

}
