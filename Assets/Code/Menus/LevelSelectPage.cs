using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LevelSelectPage : MenuPage
{
	[SerializeField] Button[] _levelButtons;
	
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
