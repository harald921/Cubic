using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public enum PLAYER_STATE
{
	IDLE,
	MOVING,
	CHARGING,
	DASHING,
	TUMBLING, // BEING DASHED FROM OTHER PLAYER, PLEASE COME UP WITH BETTER NAME!!!
	DEAD,
}

public class Character 
{
    Tile _currentTile; public Tile currentTile => _currentTile;
	TileMap _tileMap;

	PLAYER_STATE _CURRENT_STATE = PLAYER_STATE.IDLE; public PLAYER_STATE currentState => _CURRENT_STATE;
	int _currentDashCharges = 2; public int currentDashCharges => _currentDashCharges;

    GameObject _view;

	CharacterModel _model;

    CoroutineHandle _moveHandle;
	CoroutineHandle _dashHandle;

	Vector3 _lastMoveDirection = Vector3.forward;


    public Character(Tile inSpawnTile, CharacterModel inModel, TileMap inTileMap)
    {
        _currentTile = inSpawnTile;

		_tileMap = inTileMap;

        _view = GameObject.CreatePrimitive(PrimitiveType.Cube);

		_model = inModel;

        _view.transform.position = new Vector3(_currentTile.data.position.x, 1, _currentTile.data.position.y);

#if DEBUG_TOOLS

		PlayerPage.instance.player = this;

#endif
	}


    public void Move(Vector2DInt inDirection)
    {
		if (_CURRENT_STATE != PLAYER_STATE.IDLE)
			return;

        Tile targetTile = _currentTile.data.GetRelativeTile(inDirection);

        if (!targetTile.model.data.walkable)
            return;

        _moveHandle = Timing.RunCoroutineSingleton(_Move(targetTile), _moveHandle, SingletonBehavior.Abort);
    }

	public void InitiateDash()
	{
		if (_CURRENT_STATE != PLAYER_STATE.IDLE)
			return;

		_dashHandle = Timing.RunCoroutineSingleton(_dash(), _dashHandle, SingletonBehavior.Abort);
	}

    public IEnumerator<float> _Move(Tile inTargetTile)
    {
		_CURRENT_STATE = PLAYER_STATE.MOVING;

		if(!_currentTile.model.data.unBreakable)
		    _currentTile.data.DamageTile();

		Vector3 fromPosition   = new Vector3(_currentTile.data.position.x, 1, _currentTile.data.position.y);
        Vector3 targetPosition = new Vector3(inTargetTile.data.position.x, 1, inTargetTile.data.position.y);

        Vector3 movementDirection = (targetPosition - fromPosition).normalized;
        Vector3 movementDirectionRight = new Vector3(movementDirection.z, movementDirection.y, -movementDirection.x);

        Quaternion fromRotation   = _view.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(movementDirectionRight * 90) * _view.transform.rotation;

		_lastMoveDirection = movementDirection; // save last movedirection if we would do dash and not give any direction during chargeup

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

		// here we need a way to check if tile is a edge tile and die
		if (_currentTile.model.typeName == Constants.EDGE_TYPE)
		{
			// killMeHere();
			_CURRENT_STATE = PLAYER_STATE.DEAD;
			yield break;
		}

		_CURRENT_STATE = PLAYER_STATE.IDLE;
    }

	public IEnumerator<float> _dash()
	{
		_CURRENT_STATE = PLAYER_STATE.CHARGING;

		float charger = 0;
		_currentDashCharges = 2; // do a minumum of 2 tiles if quickdashing??

		// set next tile to direction of last movement
		Vector2DInt tileDirection = new Vector2DInt((int)_lastMoveDirection.x, (int)_lastMoveDirection.z);

		// charge while holding button
		while (Input.GetKey(KeyCode.Space))
		{
			// add dashes to count
			charger += Time.deltaTime;
			if(charger >= 1 / _model.dashChargeRate)
			{
				_currentDashCharges = Mathf.Clamp(_currentDashCharges + 1, 2, _model.maxDashDistance);
				charger = 0.0f;
			}

			// while charging direction can be changed
			if (Input.GetKey(KeyCode.W))
				tileDirection = Vector2DInt.Up;
			if (Input.GetKey(KeyCode.S))
				tileDirection = Vector2DInt.Down;
			if (Input.GetKey(KeyCode.A))
				tileDirection = Vector2DInt.Left;
			if (Input.GetKey(KeyCode.D))
				tileDirection = Vector2DInt.Right;

			yield return Timing.WaitForOneFrame;
		}

		_CURRENT_STATE = PLAYER_STATE.DASHING;

		// loop over all dashtiles
		for(int i =0; i < _currentDashCharges; i++)
		{
			if (!_currentTile.model.data.unBreakable) // hurt tile on leaving
				_currentTile.data.DamageTile();

			Vector3 fromPosition = new Vector3(_currentTile.data.position.x, 1, _currentTile.data.position.y);
			Vector3 targetPosition = new Vector3(fromPosition.x + tileDirection.x, 1, fromPosition.z + tileDirection.y);

			Quaternion fromRotation = _view.transform.rotation;
			Quaternion targetRotation = Quaternion.Euler(new Vector3(tileDirection.x, 0, tileDirection.y) * (90 * _model.numBarrelRollsPerDashTile)) * _view.transform.rotation;

			float movementProgress = 0;
			bool tileRefSet = false;
			while (movementProgress < 1)
			{
				movementProgress += _model.dashSpeed * Time.deltaTime;
				movementProgress = Mathf.Clamp01(movementProgress);

				_view.transform.position = Vector3.Lerp(fromPosition, targetPosition, movementProgress);
				_view.transform.rotation = Quaternion.Lerp(fromRotation, targetRotation, movementProgress);

				if (movementProgress > 0.5f && !tileRefSet)
				{
					// Set player tile references
					Tile previousTile = _currentTile;
					_currentTile = _tileMap.GetTile(new Vector2DInt((int)targetPosition.x, (int)targetPosition.z));

					// Update tile player references
					previousTile.data.RemovePlayer();
					_currentTile.data.SetPlayer(this);

					tileRefSet = true;
				}

				yield return Timing.WaitForOneFrame;
			}

			// here we die if we are in last dash and tile is deadly
			if (i +1 == _currentDashCharges && _currentTile.model.data.deadly)
			{
				// killMeHere();
				_CURRENT_STATE = PLAYER_STATE.DEAD;
				yield break;
			}

			// here we need a way to check if tile is a edge tile, then abort more dashes and die
			if (_currentTile.model.typeName == Constants.EDGE_TYPE)
			{
				// killMeHere();
				_CURRENT_STATE = PLAYER_STATE.DEAD;
				yield break;
			}
		}

		_currentDashCharges = 2;

		_CURRENT_STATE = PLAYER_STATE.IDLE;

		// cooldown before naxt available dash
		float cooldown = _model.dashCoolDown;
		while (cooldown > 0)
		{
			cooldown -= Time.deltaTime;
			yield return Timing.WaitForOneFrame;
		}

	}


}
