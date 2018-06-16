using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static Level instance { get; private set; }

    public TileMap tileMap { get; private set; }


    [SerializeField] GameObject _characterPrefab; // The character gameobject that Photon automagically creates 
    Character _character;

    void Start()
    {
        instance = this;

        GameObject spawnedCharacterGO = Instantiate(_characterPrefab);
        _character = spawnedCharacterGO.GetComponent<Character>();

        _character.Initialize(CharacterDatabase.instance.standardModel, "ExampleCharacterView");
        tileMap = new TileMap("SavedFromInputField");

        _character.Spawn(tileMap.GetTile(Vector2DInt.One));
    }

 
}
