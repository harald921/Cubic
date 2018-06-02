using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Tile currentTile;

    public event Action<Tile> OnTileExit;
    public event Action<Tile> OnTileEnter;

    void Move(Vector2DInt inDirection)
    {

    }
}
