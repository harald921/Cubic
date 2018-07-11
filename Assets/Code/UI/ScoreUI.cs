using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ScoreUI : MonoBehaviour
{
	[Serializable]
	public struct PlayerElement
	{
		public GameObject content;
		public Text scoreText;
	}

	[SerializeField] PlayerElement[] _players;	

	public void Setup(int numPlayers)
	{
		for (int i = 0; i < numPlayers; i++)
			_players[i].content.SetActive(true);
	}

	public void UpdateScore(int playerId, int score)
	{
		_players[playerId].scoreText.text = score.ToString();
	}
}
