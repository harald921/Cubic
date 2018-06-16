//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using MEC;



//public class Character 
//{
//    public Tile         currentTile        { get; private set; }
//    public PLAYER_STATE currentState       { get; private set; } = PLAYER_STATE.IDLE;
//    public int          currentDashCharges { get; private set; } 

//    CharacterModel _model;
//    GameObject     _view;

//    TileMap _tileMap;

//    CoroutineHandle _moveHandle;
//	CoroutineHandle _chargeHandle;

//	CoroutineHandle _moveCooldownHandle;
//	CoroutineHandle _chargeCooldownHandle;

//	Vector3 _lastMoveDirection = Vector3.forward;


//    public Character(Tile inSpawnTile, CharacterModel inModel, TileMap inTileMap)
//    {
//        currentTile = inSpawnTile;

//		_tileMap = inTileMap;

//        _view = GameObject.CreatePrimitive(PrimitiveType.Cube);

//		_model = inModel;

//        _view.transform.position = new Vector3(currentTile.data.position.x, 1, currentTile.data.position.y);

//        GameObject.FindObjectOfType<PlayerPage>().Initialize(this);
//	}


//    public void Move(Vector2DInt inDirection)
//    {
//		if (currentState != PLAYER_STATE.IDLE || _moveCooldownHandle.IsRunning)
//			return;

//        Tile targetTile = currentTile.data.GetRelativeTile(inDirection);

//        if (!targetTile.model.data.walkable)
//            return;

//        _moveHandle = Timing.RunCoroutineSingleton(_Move(targetTile), _moveHandle, SingletonBehavior.Abort);
//    }

//	public void TryCharge()
//	{
//		if (currentState != PLAYER_STATE.IDLE || _chargeCooldownHandle.IsRunning)
//			return;

//		_chargeHandle = Timing.RunCoroutineSingleton(_Charge(), _chargeHandle, SingletonBehavior.Abort);
//	}


//    public IEnumerator<float> _Move(Tile inTargetTile)
//    {
//        currentState = PLAYER_STATE.MOVING;

//        if (!currentTile.model.data.unbreakable)
//            currentTile.data.DamageTile();

//        yield return Timing.WaitUntilDone(_WalkInterpolate(inTargetTile));

//        // Kill player if it stands on an edge tile
//        if (currentTile.model.typeName == Constants.EDGE_TYPE) // TODO: It's probably unnecessary to have both an edge tile and an empty tile since they are the same
//        {													   // EDIT: (Johan) dash need to know the difference between edge tiles and empty tiles, empty tiles can be dashed over, edge tiles can not, meaning that player should die directly even if it have more dash charges left if traversing an edgetile
//            Debug.Log("Character dead!");                      // EDIT: (Harald) The player would stop on the edge "air" tile if it tried to move outside of the map, with the same result I believe. 
//            currentState = PLAYER_STATE.DEAD;
//            yield break;
//        }

//        currentState = PLAYER_STATE.IDLE;

//		_moveCooldownHandle = Timing.RunCoroutineSingleton(_moveCooldown(), _moveCooldownHandle, SingletonBehavior.Abort);
//	}

//    public IEnumerator<float> _WalkInterpolate(Tile inTargetTile)
//    {
//        // Calculate lerp positions
//        Vector3 fromPosition   = new Vector3(currentTile.data.position.x, 1, currentTile.data.position.y);
//        Vector3 targetPosition = new Vector3(inTargetTile.data.position.x, 1, inTargetTile.data.position.y);

//        // Calculate lerp rotations
//        Vector3 movementDirection = (targetPosition - fromPosition).normalized;
//        Vector3 movementDirectionRight = new Vector3(movementDirection.z, movementDirection.y, -movementDirection.x);

//        Quaternion fromRotation = _view.transform.rotation;
//        Quaternion targetRotation = Quaternion.Euler(movementDirectionRight * 90) * _view.transform.rotation;

//        // Save last move direction if we would do dash and not give any direction during chargeup
//        _lastMoveDirection = movementDirection;

//        float movementProgress = 0;
//        bool tileRefSet = false;
//        while (movementProgress < 1)
//        {
//            movementProgress += _model.walkSpeed * Time.deltaTime;
//            movementProgress = Mathf.Clamp01(movementProgress);

//            _view.transform.position = Vector3.Lerp(fromPosition, targetPosition, movementProgress);
//            _view.transform.position = new Vector3(_view.transform.position.x, 1 + Mathf.Sin(movementProgress * (float)Math.PI), _view.transform.position.z);

//            _view.transform.rotation = Quaternion.Lerp(fromRotation, targetRotation, movementProgress);

//            if (movementProgress > 0.5f && !tileRefSet)
//            {
//                // Set player tile references
//                Tile previousTile = currentTile;
//                currentTile = inTargetTile;

//                // Update tile player references
//                previousTile.data.RemovePlayer();
//                inTargetTile.data.SetPlayer(this);

//                tileRefSet = true;
//            }

//            yield return Timing.WaitForOneFrame;
//        }
//    }


//    public IEnumerator<float> _Charge()
//    {
//        Debug.Log("Begin charge");
//        currentState = PLAYER_STATE.CHARGING;

//        // Set next tile to direction of last movement
//        Vector2DInt dashDirection = new Vector2DInt((int)_lastMoveDirection.x, (int)_lastMoveDirection.z);

//        // Charge dash while holding button
//        float chargeAmount = 2; // TODO: Expose "min dash distance" variable
//        while (Input.GetKey(KeyCode.Space))
//        {
//            // Add dashes to count
//            chargeAmount = Mathf.Clamp(chargeAmount += _model.dashChargeRate * Time.deltaTime, 2, _model.dashMaxDistance);

//            // while charging direction can be changed
//            if (Input.GetKey(KeyCode.W))
//                dashDirection = Vector2DInt.Up;
//            if (Input.GetKey(KeyCode.S))
//                dashDirection = Vector2DInt.Down;
//            if (Input.GetKey(KeyCode.A))
//                dashDirection = Vector2DInt.Left;
//            if (Input.GetKey(KeyCode.D))
//                dashDirection = Vector2DInt.Right;

//            currentDashCharges = (int)chargeAmount;

//            yield return Timing.WaitForOneFrame;
//        }

//        Timing.RunCoroutine(_Dash((int)chargeAmount, dashDirection));
//    }

//	public IEnumerator<float> _Dash(int inDashStrength, Vector2DInt inDirection)
//	{
//		currentState = PLAYER_STATE.DASHING;

//        // Move over all dashtiles
//        for (int i = 0; i < inDashStrength; i++)
//		{
//			if (!currentTile.model.data.unbreakable) 
//				currentTile.data.DamageTile();

//            Tile targetTile = _tileMap.GetTile(currentTile.data.position + inDirection);


//            yield return Timing.WaitUntilDone(_DashInterpolate(targetTile));

//			// Kill player if it dashed into the edge
//			if (currentTile.model.typeName == Constants.EDGE_TYPE)
//			{
//				// killMeHere();
//				currentState = PLAYER_STATE.DEAD;
//				yield break;
//			}
//		}

//        if (currentTile.model.data.deadly)
//        {
//            Debug.Log("Character died!");
//            currentState = PLAYER_STATE.DEAD;
//            yield break;
//        }
				
//        currentState = PLAYER_STATE.IDLE;

//		_moveCooldownHandle   = Timing.RunCoroutineSingleton(_moveCooldown(), _moveCooldownHandle, SingletonBehavior.Abort);
//		_chargeCooldownHandle = Timing.RunCoroutineSingleton(_chargeCooldown(), _chargeCooldownHandle, SingletonBehavior.Abort);

//	}

//    public IEnumerator<float> _DashInterpolate(Tile inTargetTile)
//    {
//        Vector3 fromPosition   = new Vector3(currentTile.data.position.x, 1, currentTile.data.position.y);
//        Vector3 targetPosition = new Vector3(inTargetTile.data.position.x, 1, inTargetTile.data.position.y);

//        Vector3 moveDirection = (targetPosition - fromPosition).normalized;

//        Quaternion fromRotation = _view.transform.rotation;
//        Quaternion targetRotation = Quaternion.Euler(moveDirection * (90 * _model.numBarrelRollsPerDashTile)) * _view.transform.rotation;

//        float movementProgress = 0; 
//        bool tileRefSet = false;
//        while (movementProgress < 1)
//        {
//            movementProgress += _model.dashSpeed * Time.deltaTime;
//            movementProgress = Mathf.Clamp01(movementProgress);

//            _view.transform.position = Vector3.Lerp(fromPosition, targetPosition, movementProgress);
//            _view.transform.rotation = Quaternion.Lerp(fromRotation, targetRotation, movementProgress);

//            if (movementProgress > 0.5f && !tileRefSet)
//            {
//                // Set player tile references
//                Tile previousTile = currentTile;
//                currentTile = _tileMap.GetTile(new Vector2DInt((int)targetPosition.x, (int)targetPosition.z));

//                // Update tile player references
//                previousTile.data.RemovePlayer();
//                currentTile.data.SetPlayer(this);

//                tileRefSet = true;
//            }

//            yield return Timing.WaitForOneFrame;
//        }
//    }

//	IEnumerator<float> _moveCooldown()
//	{
//		yield return Timing.WaitForSeconds(_model.walkCooldown);
//	}

//	IEnumerator<float> _chargeCooldown()
//	{
//		yield return Timing.WaitForSeconds(_model.dashCooldownTime);
//	}
//}
