﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Level : Photon.MonoBehaviour
{
    public static Level instance { get; private set; }

    public TileMap tileMap { get; private set; }

	[SerializeField] NetworkManager _networkManager;
    [SerializeField] GameObject _characterPrefab; // The character gameobject that Photon automagically creates 
	[SerializeField] string _mapToLoad;
	[SerializeField] string _modelView;

	[SerializeField] Transform _tilesFolder;

    Character _character;

    void Awake()
    {
        instance = this;

		print(-5 - -5);
    }

	public void ManualStart()
	{
		_character = PhotonNetwork.Instantiate("Character", Vector3.zero, Quaternion.identity, 0).GetComponent<Character>();
		_character.Initialize(_modelView);

		tileMap = new TileMap(_mapToLoad, _tilesFolder);
		
		_character.Spawn(tileMap.GetRandomTileCoords());

	}
	
	// use for debbuging so you get a different color
	Color GetRandomColor()
	{
		return new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
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
		_character.Spawn(tileMap.GetRandomTileCoords());
	}
}
