using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Level : Photon.MonoBehaviour
{
    
    public TileMap tileMap { get; private set; }
	
    [SerializeField] GameObject _characterPrefab; // The character gameobject that Photon automagically creates 
	[SerializeField] string _mapToLoad;
	[SerializeField] Transform _tilesFolder;

    Character _character;

	public void ManualStart()
	{
		PlayerData pd = Match.instance.playerData;

		_character = PhotonNetwork.Instantiate("Character", Vector3.zero, Quaternion.identity, 0).GetComponent<Character>();
		_character.Initialize(pd.character, pd.playerId);

		tileMap = new TileMap(_mapToLoad, _tilesFolder);
		
		_character.Spawn(tileMap.GetSpawnPointFromPlayerId(pd.playerId));
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
		_character.Spawn(tileMap.GetSpawnPointFromPlayerId(Match.instance.playerData.playerId));
	}

	
}
