using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Level : Photon.MonoBehaviour
{
    public static Level instance { get; private set; }

    public TileMap tileMap { get; private set; }
	[SerializeField] NetworkManager _networkManager;
	[SerializeField] string _mapToLoad;
    [SerializeField] GameObject _characterPrefab; // The character gameobject that Photon automagically creates 
	[SerializeField] string _modelView;
    Character _character;

    void Awake()
    {
        instance = this;				
    }

	public void ManualStart()
	{
		_character = PhotonNetwork.Instantiate("Character", Vector3.zero, Quaternion.identity, 0).GetComponent<Character>();
		_character.Initialize(_modelView);

		tileMap = new TileMap(_mapToLoad);
		
		_character.Spawn(tileMap.GetRandomTileCoords());

	}
	
	// use for debbuging so you get a different color
	Color GetRandomColor()
	{
		return new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
	}


}
