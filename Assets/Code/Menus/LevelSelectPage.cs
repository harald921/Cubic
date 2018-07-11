using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectPage : Photon.MonoBehaviour
{

	[SerializeField] Button[] _levelButtons;

	[SerializeField] GameObject _characterScreen;
	[SerializeField] GameObject _content;

	public void OnEnterPage()
	{
		if (!PhotonNetwork.isMasterClient)
			for (int i = 0; i < _levelButtons.Length; i++)
				_levelButtons[i].interactable = false;
	}

	public void OnLevelSelected(string level)
	{
		photonView.RPC("SetLevelAndGoToCharacter", PhotonTargets.All, level);
	}

	[PunRPC]
	void SetLevelAndGoToCharacter(string level)
	{
		FindObjectOfType<PlayerData>().level = level;

		_content.SetActive(false);
		_characterScreen.SetActive(true);
	}
}
