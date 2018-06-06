using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class Character 
{
    Tile _currentTile;

    GameObject _view;

	CharacterModel _model;

    CoroutineHandle _moveHandle;


    public Character(Tile inSpawnTile, CharacterModel inModel)
    {
        _currentTile = inSpawnTile;

        _view = GameObject.CreatePrimitive(PrimitiveType.Cube);

		_model = inModel;

        _view.transform.position = new Vector3(_currentTile.data.position.x, 1, _currentTile.data.position.y);
    }


    public void Move(Vector2DInt inDirection)
    {
        Tile targetTile = _currentTile.data.GetRelativeTile(inDirection);

        if (!targetTile.model.data.walkable)
            return;

        _moveHandle = Timing.RunCoroutineSingleton(_Move(targetTile), _moveHandle, SingletonBehavior.Abort);
    }

    public IEnumerator<float> _Move(Tile inTargetTile)
    {
		if(!_currentTile.model.data.unBreakable)
		    _currentTile.data.DamageTile();

		Vector3 fromPosition   = new Vector3(_currentTile.data.position.x, 1, _currentTile.data.position.y);
        Vector3 targetPosition = new Vector3(inTargetTile.data.position.x, 1, inTargetTile.data.position.y);

        Vector3 movementDirection = (targetPosition - fromPosition).normalized;
        Vector3 movementDirectionRight = new Vector3(movementDirection.z, movementDirection.y, -movementDirection.x);

        Quaternion fromRotation   = _view.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(movementDirectionRight * 90) * _view.transform.rotation;

        float movementProgress = 0;
		bool tileRefSet = false;
        while (movementProgress < 1)
        {
            movementProgress += _model.moveSpeed * Time.deltaTime;
			movementProgress = Mathf.Clamp01(movementProgress);

            _view.transform.position = Vector3.Lerp(fromPosition, targetPosition, movementProgress);
            _view.transform.position = new Vector3(_view.transform.position.x, 1 + Mathf.Sin(movementProgress * (float)Math.PI), _view.transform.position.z);

            _view.transform.rotation = Quaternion.Lerp(fromRotation, targetRotation, movementProgress);

            if (movementProgress > 0.5f && !tileRefSet)
            {
                // Set player tile references
                Tile previousTile = _currentTile;
                _currentTile = inTargetTile;

                // Update tile player references
                previousTile.data.RemovePlayer();
                inTargetTile.data.SetPlayer(this);

				tileRefSet = true;
            }

            yield return Timing.WaitForOneFrame;
        }

        
       
    }
}
