using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static Level instance { get; private set; }

    public TileMap tileMap { get; private set; }

    void Start()
    {
        instance = this;

        tileMap = new TileMap("SavedFromInputField");


        new Player(tileMap.GetTile(Vector2DInt.One));
    }
}
