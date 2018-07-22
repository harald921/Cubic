﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : Photon.MonoBehaviour
{
    
    public TileMap tileMap { get; private set; }
	
    [SerializeField] GameObject _characterPrefab; // The character gameobject that Photon automagically creates 
	[SerializeField] string _mapToLoad;
	[SerializeField] Transform _tilesFolder;

    Character _character;

	int _spawnID;

	public void ManualStart()
	{
		// get player properties saved in photonplayer
		string characterName = PhotonNetwork.player.CustomProperties[Constants.CHARACTER_NAME].ToString();
		int skinID           = (int)PhotonNetwork.player.CustomProperties[Constants.SKIN_ID];

		_character = PhotonNetwork.Instantiate("Character", Vector3.zero, Quaternion.identity, 0).GetComponent<Character>();
		_character.Initialize(characterName, PhotonNetwork.player.ID, PhotonNetwork.player.NickName, skinID);

		tileMap = new TileMap(_mapToLoad, _tilesFolder);

		_spawnID = (int)PhotonNetwork.player.CustomProperties[Constants.SPAWN_ID];

		_character.Spawn(tileMap.GetSpawnPointFromSpawnID(_spawnID));		
	}
		
	public void ResetRound()
	{
		photonView.RPC("NetworkResetRound", PhotonTargets.All);
	}

	[PunRPC]
	void NetworkResetRound()
	{
		tileMap.ClearTileViews();
		tileMap.ResetMap();		
		_character.Spawn(tileMap.GetSpawnPointFromSpawnID(_spawnID));
	}

	public void BreakTile(int x, int y)
	{
		photonView.RPC("NetworkBreakTile", PhotonTargets.All, x, y);
	}

	[PunRPC]
	void NetworkBreakTile(int x, int y)
	{
		tileMap.GetTile(new Vector2DInt(x, y)).data.DamageTile();
	}
}
