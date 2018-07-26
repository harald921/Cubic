using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectPage : MenuPage
{
	[SerializeField] MenuPlayerInfoUI _playerInfo;
	[SerializeField] MessagePromt _promt;
	[SerializeField] Button[] _levelButtons;

	// called from button on server only (this will be changed to a nominate system where everyone gets to pick 1 level and then a random of all picked levels will be chosen)
	public void OnLevelSelected(string level)
	{
		photonView.RPC("SetLevelAndGoToCharacter", PhotonTargets.All, level);
	}

	public override void OnPageEnter()
	{
		if (!PhotonNetwork.isMasterClient)
			ChangeAllButtonsState(false);
	}

	public override void OnPageExit()
	{
		ChangeAllButtonsState(true);
	}

	public override void OnPlayerLeftRoom(PhotonPlayer player)
	{
		_playerInfo.DisableUIOfPlayer(player.ID);
		if (PhotonNetwork.room.PlayerCount == 1)
			_promt.SetAndShow("All other players have left the room!!\n\n Returning to menu!!!",
				() => {
					PhotonNetwork.RemovePlayerCustomProperties(null);
					_playerInfo.DisableUIOfPlayer(PhotonNetwork.player.ID);
					PhotonNetwork.LeaveRoom();
					MainMenuSystem.instance.SetToPage("StartScreen");
				});
	}

	public override void UpdatePage()
	{
		
	}

	[PunRPC]
	void SetLevelAndGoToCharacter(string level)
	{
		// set witch scene to be loaded		
		PhotonHelpers.SetPlayerProperty(PhotonNetwork.player, Constants.LEVEL_NAME, level);
		MainMenuSystem.instance.SetToPage("CharacterSelectScreen");
	}

	public void LeaveRoom()
	{
		PhotonNetwork.RemovePlayerCustomProperties(null);
		_playerInfo.DisableAllPlayerUI();
		PhotonNetwork.LeaveRoom();
		MainMenuSystem.instance.SetToPage("StartScreen");
	}

	void ChangeAllButtonsState(bool enable)
	{
		for (int i = 0; i < _levelButtons.Length; i++)
			_levelButtons[i].interactable = enable;
	}
}
