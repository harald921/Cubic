using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class Match : Photon.MonoBehaviour
{
	public static Match instance { get; private set; }

	public Level level           { get; private set; }	
	public bool matchStarted     { get; private set; }

	[SerializeField] ScoreUI _scoreUI;
	[SerializeField] StartCounterUI _counterUI;

	const int _numRoundsToWin = 3;

	public class PlayerTracker
	{
		public int score;
		public bool dead;
		public bool disconnected;
	}

	Dictionary<int,PlayerTracker> _players;

	int _numPlayers;
	bool _winnerSet;

	void Awake()
	{
		instance = this;		
	}

	void Start()
	{		
		level = FindObjectOfType<Level>();

		SetupMatch();

		// check if we start from menu or the simple network starter firectly from level scen
		if (FindObjectOfType<SimpleNetworkStarter>() == null)
			StartOnAllLoaded();
	}

	void OnDestroy()
	{
		instance = null;	
	}

	void SetupMatch()
	{
		// init data structures
		_numPlayers = PhotonNetwork.room.PlayerCount;
		_players = new Dictionary<int, PlayerTracker>();

		// tell the ui how many players we are
		_scoreUI.Setup(_numPlayers);
	}

	void StartOnAllLoaded()
	{
		// this rpc will execute when all players are loaded in the scene
		if (PhotonNetwork.isMasterClient)
			photonView.RPC("AllIsLoaded", PhotonTargets.MasterClient);
	}

	public void OnPlayerDie(int playerId)
	{
		if (_winnerSet) // if last player dies after winning we dont want to do nothing
			return;

		_players[playerId].dead = true;
		
		int numAlive = 0;
		int idLastAlive = 0;

		// get how many players is left alive
		foreach(var p in _players)		
			if(!p.Value.dead && !p.Value.disconnected)
			{
				numAlive++;
				idLastAlive = p.Key;
			}
		
		// round over
		if (numAlive <= 1)
			RoundOver(idLastAlive);
	}
	
	void RoundOver(int winnerId)
	{
		_winnerSet = true;

		// increment score and check if the match is over or if we should start next round
		_players[winnerId].score++;
		if (_players[winnerId].score == _numRoundsToWin)
			MatchOver(winnerId);
		else
			StartNewRound(winnerId, _players[winnerId].score);
	}

	void MatchOver(int winnerId)
	{


	}

	public void StartNewRound(int winner, int score)
	{
		// tell all clients who won, the clients need to keep track of score in case of server migration
		photonView.RPC("RoundWinner", PhotonTargets.Others, winner);

		// untag dead players
		foreach (var p in _players)
			p.Value.dead = false;

		_winnerSet = false;

		// do small delay before we reset to new round
		Timing.RunCoroutine(_resetDelay(2, winner, score));		
	}
	
	// callback from when countdown is done
	// matchStarted will enable input for the players
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

	// callback from character when someone disconnects, called locally on all clients
	public void OnPlayerLeft(int id)
	{		
		_players[id].disconnected = true;
		_scoreUI.DisableUIOfDisconnectedPlayer(id);
	}

	[PunRPC]
	void AllIsLoaded()
	{
		// tell everyone to start the match, send the timestamp so the countdown timer will be the same for all players no matter lag
		photonView.RPC("NetworkStartGame", PhotonTargets.All, PhotonNetwork.time);
	}

	[PunRPC]
	void NetworkStartGame(double delta)
	{
		// init master class that have last say in all collisions(will only be called on the server)
		FindObjectOfType<CollisionTracker>().ManualStart();

		// create level (player creation is here for now aswell)
		level.ManualStart();

		// start countdown
		_counterUI.StartCount(delta);
	}

	[PunRPC]
	void RoundWinner(int id)
	{
		_players[id].score++;
	}

	[PunRPC]
	void NetworkStartNewRound(int lastWinner, int newScore, double delta)
	{
		matchStarted = false;

		// reset level(character resapwn is here aswell for now)
		level.ResetRound();

		// update score ui and restart timer
		_scoreUI.UpdateScore(lastWinner, newScore);
		_counterUI.StartCount(delta);
	}

	[PunRPC]
	void RegisterPlayer(int id, string name)
	{
		// register a player by id for scorekepping and ui
		_players.Add(id, new PlayerTracker());
		_scoreUI.RegisterPlayer(id, name);
	}
}
