using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LevelSelectPage : MenuPage
{
	[SerializeField] Button[] _levelButtons;
	[SerializeField] Text _numJoinedPlayersText;

	// called from button on server only (this will be changed to a nominate system where everyone gets to pick 1 level and then a random of all picked levels will be chosen)
	public void OnLevelSelected(string level)
	{
		photonView.RPC("SetLevelAndGoToCharacter", PhotonTargets.All, level);
	}

	public override void OnPageEnter()
	{
		if (!PhotonNetwork.isMasterClient)
			for (int i = 0; i < _levelButtons.Length; i++)
				_levelButtons[i].interactable = false;
	}

	public override void OnPageExit()
	{		
	}

	public override void OnPlayerLeftRoom(PhotonPlayer player)
	{
	}

	public override void UpdatePage()
	{
		if (PhotonNetwork.room == null)
			return;

		_numJoinedPlayersText.text = string.Format("{0}/4", PhotonNetwork.room.PlayerCount.ToString());
	}

	[PunRPC]
	void SetLevelAndGoToCharacter(string level)
	{
		// set witch scene to be loaded
		Hashtable p = PhotonNetwork.player.CustomProperties;
		p.Add(Constants.LEVEL_NAME, level);
		PhotonNetwork.player.SetCustomProperties(p);

		MainMenuSystem.instance.SetToPage("CharacterSelectScreen");
	}
}
