using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static Level instance { get; private set; }

    public TileMap tileMap { get; private set; }

    Character debugPlayer;


    void Start()
    {
        instance = this;

        tileMap = new TileMap("SavedFromInputField");

        debugPlayer = new Character(tileMap.GetTile(Vector2DInt.One), CharacterDatabase.instance.standardModel, tileMap);
    }

    void Update()
    {
		if (Input.GetKey(KeyCode.Space))
			debugPlayer.TryCharge();

		if (Input.GetKey(KeyCode.W))
            debugPlayer.Move(Vector2DInt.Up);
        if (Input.GetKey(KeyCode.S))
            debugPlayer.Move(Vector2DInt.Down);
        if (Input.GetKey(KeyCode.A))
            debugPlayer.Move(Vector2DInt.Left);
        if (Input.GetKey(KeyCode.D))
            debugPlayer.Move(Vector2DInt.Right);		
    }

}
