using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static Level instance { get; private set; }

    public TileMap tileMap { get; private set; }


    [SerializeField] GameObject _characterPrefab; // The character gameobject that Photon automagically creates 
    NewCharacter _character;

    void Start()
    {
        instance = this;

        GameObject spawnedCharacterGO = Instantiate(_characterPrefab);
        _character = spawnedCharacterGO.GetComponent<NewCharacter>();

        _character.Initialize(CharacterDatabase.instance.standardModel, "ExampleCharacterView");
        tileMap = new TileMap("SavedFromInputField");

        _character.Spawn(tileMap.GetTile(Vector2DInt.One));
    }

    void Update()
    {
		if (Input.GetKey(KeyCode.Space))
            _character.movementComponent.TryCharge();

		if (Input.GetKey(KeyCode.W))
            _character.movementComponent.TryWalk(Vector2DInt.Up);
        if (Input.GetKey(KeyCode.S))
            _character.movementComponent.TryWalk(Vector2DInt.Down);
        if (Input.GetKey(KeyCode.A))
            _character.movementComponent.TryWalk(Vector2DInt.Left);
        if (Input.GetKey(KeyCode.D))
            _character.movementComponent.TryWalk(Vector2DInt.Right);		
    }
}
