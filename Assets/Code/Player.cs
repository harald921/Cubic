﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player 
{
    Tile _currentTile;

    GameObject _view;

	PlayerValues _values;


    public Player(Tile inSpawnTile, PlayerValues values)
    {
        _currentTile = inSpawnTile;

        _view = GameObject.CreatePrimitive(PrimitiveType.Cube);

		_values = values;

        UpdateView();
    }


    public void Move(Vector2DInt inDirection)
    {
        Tile targetTile = _currentTile.data.GetRelativeTile(inDirection);

        if (!targetTile.model.data.walkable)
            return;

        // Set player tile references
        Tile previousTile = _currentTile;
        _currentTile      = targetTile;

        // Update tile player references
        previousTile.data.RemovePlayer();
        targetTile.data.SetPlayer(this);

		if (!previousTile.model.data.unBreakable && !previousTile.model.data.deadly)
			 previousTile.data.DamageTile();

        UpdateView();
    }

    void UpdateView() =>
        _view.transform.position = new Vector3(_currentTile.data.position.x, 1, _currentTile.data.position.y);
}
