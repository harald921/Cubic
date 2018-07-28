using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;
using MEC;

public class LevelSelectPage : MenuPage
{
	[Serializable]
	public class LevelData
	{
		public string name;
		public string sceneName;
		public Button button;
		public Sprite sprite;
	}

	[Serializable]
	public class NominatedData
	{
		public GameObject content;
		public Image image;
		public Text name;
	}

	[SerializeField] GameObject _selectScreen;
	[SerializeField] GameObject _nominatedScreen;

	[SerializeField] MenuPlayerInfoUI _playerInfo;
	[SerializeField] MessagePromt _promt;
	[SerializeField] StartCounterUI _counter;
	[SerializeField] LevelData[] _levels;
	[SerializeField] NominatedData[] _nominatedLevelUI;

	[SerializeField] GameObject _border;
	[SerializeField] Text _levelWinnerNameText;

	public void OnLevelSelected(int level)
	{
		PhotonHelpers.SetPlayerProperty(PhotonNetwork.player, Constants.PLAYER_READY, true);
		PhotonHelpers.SetPlayerProperty(PhotonNetwork.player, Constants.NOMINATED_LEVEL, level);
		ChangeAllButtonsState(false);

		// tell server that we are selected and ready
		_playerInfo.photonView.RPC("SetReadyUI", PhotonTargets.All, PhotonNetwork.player.ID, true);
	}

	public override void OnPageEnter()
	{
		// move all player UI boxes to the prefered positions of this page
		_playerInfo.SetPlayerUIByScreen(MenuScreen.LevelSelect);

		InvokeRepeating("AllNominatedLevel", 0, 5);
		_selectScreen.SetActive(true);
		_nominatedScreen.SetActive(false);

		// if masterclient tell everyone to start countdown timer
		if (PhotonNetwork.isMasterClient)
			photonView.RPC("StartCountdown", PhotonTargets.All, PhotonNetwork.time);
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
					PhotonNetwork.LeaveRoom();
					MainMenuSystem.instance.SetToPage("StartScreen");
				});
	}

	public override void UpdatePage()
	{
		
	}

	void AllNominatedLevel()
	{
		if (!PhotonNetwork.isMasterClient || PhotonNetwork.room.PlayerCount < 2)
			return;

		int playersReady = 0;
		foreach (PhotonPlayer p in PhotonNetwork.playerList)
			if (p.CustomProperties.ContainsKey(Constants.PLAYER_READY) && (bool)p.CustomProperties[Constants.PLAYER_READY])
				playersReady++;

		int numPlayers = PhotonNetwork.room.PlayerCount;
		if (playersReady == numPlayers)
		{
			int winner = Random.Range(0, numPlayers);
			string winnerLevel = "";
			string winnerLevelName = "";

			Debug.Log(winner);

			int[] nL = { 0, 0, 0, 0 };
			for(int i =0; i < numPlayers; i++)
			{
				if (i == winner)
				{
					winnerLevel     = _levels[(int)PhotonNetwork.playerList[i].CustomProperties[Constants.NOMINATED_LEVEL]].sceneName;
					winnerLevelName = _levels[(int)PhotonNetwork.playerList[i].CustomProperties[Constants.NOMINATED_LEVEL]].name;
				}

				nL[i] = (int)PhotonNetwork.playerList[i].CustomProperties[Constants.NOMINATED_LEVEL];
			}

			photonView.RPC("LevelToPlay", PhotonTargets.All, winnerLevel, winnerLevelName, winner, nL[0], nL[1], nL[2], nL[3]);
		}
	}

	[PunRPC]
	void LevelToPlay(string level, string levelName, int winnerIndex, int one, int two, int three, int four)
	{
		CancelInvoke("AllNominatedLevel");
		_counter.CancelCount();
		PhotonHelpers.SetPlayerProperty(PhotonNetwork.player, Constants.LEVEL_NAME, level);
		PhotonHelpers.SetPlayerProperty(PhotonNetwork.player, Constants.PLAYER_READY, false);

		Timing.RunCoroutine(_PickRandomLevel(winnerIndex, levelName, new int[]{one, two, three, four}));
	}

	[PunRPC]
	void StartCountdown(double delta)
	{
		_counter.StartCount(delta, 20, () => OnLevelSelected(Random.Range(0, _levels.Length)));
	}

	void GoToCharacter()
	{		
		MainMenuSystem.instance.SetToPage("CharacterSelectScreen");
	}

	public void LeaveRoom()
	{
		_counter.CancelCount();
		_playerInfo.DisableAllPlayerUI();
		PhotonNetwork.RemovePlayerCustomProperties(null);
		PhotonNetwork.LeaveRoom();
		MainMenuSystem.instance.SetToPage("StartScreen");
	}

	void ChangeAllButtonsState(bool enable)
	{
		for (int i = 0; i < _levels.Length; i++)
			_levels[i].button.interactable = enable;
	}

	IEnumerator<float> _PickRandomLevel(int winnerIndex, string levelName, int[] nominatedLevels)
	{
		_nominatedScreen.SetActive(true);
		_selectScreen.SetActive(false);
		_border.SetActive(true);
		_levelWinnerNameText.text = "";

		for (int i = 0; i < 4; i++)
			_nominatedLevelUI[i].content.SetActive(false);


		int numLevels = PhotonNetwork.room.PlayerCount;
		for(int i =0; i< numLevels; i++)
		{
			_nominatedLevelUI[i].content.SetActive(true);
			_nominatedLevelUI[i].image.sprite = _levels[nominatedLevels[i]].sprite;
		}

		int steps = winnerIndex + 1 + (numLevels * 20);

		float timePerStep = 0.04f;
		int loopsWithoutIncrease = 12;
		float timeIncresePerLoop = 0.05f;
		int count = 0;
		int loops = 0;

		while (count < steps )
		{			
			for(int i =0; i < numLevels; i++)
			{												
				count++;

				_border.transform.position = _nominatedLevelUI[i].content.transform.position;

				if (count == steps)
					break;

				yield return Timing.WaitForSeconds(timePerStep);
			}
			loops++;
			if(loops >= loopsWithoutIncrease)
			  timePerStep += timeIncresePerLoop;

		}

		_levelWinnerNameText.text = levelName;

		yield return Timing.WaitForSeconds(3.0f);

		GoToCharacter();
	}

}
