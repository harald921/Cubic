using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LevelSelectPage : MenuPage
{
	[SerializeField] Button[] _levelButtons;
	[SerializeField] Text _numJoinedPlayersText;

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

	public override void UpdatePage()
	{
		if (PhotonNetwork.room == null)
			return;

		_numJoinedPlayersText.text = string.Format("{0}/4", PhotonNetwork.room.PlayerCount.ToString());
	}

	[PunRPC]
	void SetLevelAndGoToCharacter(string level)
	{
		Hashtable p = PhotonNetwork.player.CustomProperties;
		p.Add(Constants.LEVEL_NAME, level);
		PhotonNetwork.player.SetCustomProperties(p);

		MainMenuSystem.instance.SetToPage("CharacterSelectScreen");
	}
}
