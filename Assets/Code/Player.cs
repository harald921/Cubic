using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player 
{
    Tile _currentTile;

    GameObject _view;


    public Player(Tile inSpawnTile)
    {
        _currentTile = inSpawnTile;

        _view = GameObject.CreatePrimitive(PrimitiveType.Cube);

        UpdateView();
    }


    void Move(Vector2DInt inDirection)
    {
        Tile targetTile = _currentTile.GetRelativeTile(inDirection);

        if (!targetTile.model.data.walkable)
            return;

        // Set player tile references
        Tile previousTile = _currentTile;
        _currentTile      = targetTile;

        // Update tile player references
        previousTile.data.RemovePlayer();
        targetTile.data.SetPlayer(this);

        UpdateView();
    }

    void UpdateView() =>
        _view.transform.position = new Vector3(_currentTile.data.position.x, 1, _currentTile.data.position.y);
}
