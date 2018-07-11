using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class Match : Photon.MonoBehaviour
{
	public static Match instance { get; private set; }

	public Level level           { get; private set; }
	public PlayerData playerData { get; private set; }

	public bool matchStarted     { get; private set; }

	[SerializeField] ScoreUI _scoreUI;
	[SerializeField] StartCounterUI _counterUI;

	const int _numRoundsToWin = 3;

	int _numPlayers;
	int[] _score;

	bool[] _deadPlayers;

	bool _winnerSet;

	void Awake()
	{
		instance = this;		
	}

	void Start()
	{
		playerData = FindObjectOfType<PlayerData>();
		level      = FindObjectOfType<Level>();

		SetupMatch();

		if (FindObjectOfType<SimpleNetworkStarter>() == null)
			StartOnAllLoaded();
	}

	void OnDestroy()
	{
		instance = null;	
	}

	void StartOnAllLoaded()
	{
		if (PhotonNetwork.isMasterClient)
			photonView.RPC("AllIsLoaded", PhotonTargets.MasterClient);
	}

	[PunRPC]
	void AllIsLoaded()
	{
		photonView.RPC("NetworkStartGame", PhotonTargets.All, PhotonNetwork.time);
	}

	[PunRPC]
	void NetworkStartGame(double delta)
	{
		FindObjectOfType<CollisionTracker>().ManualStart();
		level.ManualStart();
		_counterUI.StartCount(delta);
	}

	public void OnPlayerDie(int playerId)
	{
		if (_winnerSet) // if last player dies after winning we dont want to do nothing
			return;

		_deadPlayers[playerId] = true;

		int numAlive = 0;
		int idLastAlive = 0;
		for(int i =0; i < _numPlayers; i++)		
			if (!_deadPlayers[i])
			{
				numAlive++;
				idLastAlive = i;
			}

		if (numAlive <= 1)
			RoundOver(idLastAlive);
	}

	void SetupMatch()
	{
		_numPlayers  = PhotonNetwork.room.PlayerCount;
		_score       = new int[_numPlayers];
		_deadPlayers = new bool[_numPlayers];

		_scoreUI.Setup(_numPlayers);
	}

	void RoundOver(int winnerId)
	{
		_winnerSet = true;

		_score[winnerId] ++;
		if (_score[winnerId] == _numRoundsToWin)
			MatchOver(winnerId);
		else
			StartNewRound(winnerId, _score[winnerId]);
	}

	void MatchOver(int winnerId)
	{


	}

	public void StartNewRound(int winner, int score)
	{
		_deadPlayers = new bool[_numPlayers];
		_winnerSet = false;

		Timing.RunCoroutine(_resetDelay(2, winner, score));		
	}

	[PunRPC]
	void NetworkStartNewRound(int lastWinner, int newScore, double delta)
	{
		matchStarted = false;

		level.ResetRound();

		_scoreUI.UpdateScore(lastWinner, newScore);
		_counterUI.StartCount(delta);
	}

	public void OnCounterZero()
	{
		matchStarted = true;
	}

	IEnumerator<float> _resetDelay(float delay, int winner, int score)
	{
		float timer = 0;
		while(timer < delay)
		{
			timer += Time.deltaTime;
			yield return Timing.WaitForOneFrame;
		}

		photonView.RPC("NetworkStartNewRound", PhotonTargets.All, winner, score, PhotonNetwork.time);
	}
}
