using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
	[SerializeField] PlayerValues _playerValues;

    public static Level instance { get; private set; }

    public TileMap tileMap { get; private set; }

    Player debugPlayer;


    void Start()
    {
        instance = this;

        tileMap = new TileMap("SavedFromInputField");


        debugPlayer = new Player(tileMap.GetTile(Vector2DInt.One), _playerValues);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
            debugPlayer.Move(Vector2DInt.Up);
        if (Input.GetKeyDown(KeyCode.S))
            debugPlayer.Move(Vector2DInt.Down);
        if (Input.GetKeyDown(KeyCode.A))
            debugPlayer.Move(Vector2DInt.Left);
        if (Input.GetKeyDown(KeyCode.D))
            debugPlayer.Move(Vector2DInt.Right);
    }
}
