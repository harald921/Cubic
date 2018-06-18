using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Level : Photon.MonoBehaviour
{
    public static Level instance { get; private set; }

    public TileMap tileMap { get; private set; }
	[SerializeField] NetworkManager _networkManager; 

    [SerializeField] GameObject _characterPrefab; // The character gameobject that Photon automagically creates 
    Character _character;

    void Awake()
    {
        instance = this;				
    }

	public void ManualStart()
	{
		_character = PhotonNetwork.Instantiate("Character", Vector3.zero, Quaternion.identity, 0).GetComponent<Character>();
		_character.Initialize("ExampleCharacterView");

		tileMap = new TileMap("SavedFromInputField");

		_character.Spawn(Vector2DInt.One);
	}
	
 
}
