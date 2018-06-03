using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    TileModel currentTile;

    public event Action<TileModel> OnTileExit;
    public event Action<TileModel> OnTileEnter;

    void Move(Vector2DInt inDirection)
    {

    }
}
