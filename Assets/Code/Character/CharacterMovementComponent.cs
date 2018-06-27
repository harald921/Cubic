using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using Photon;

public class CharacterMovementComponent : Photon.MonoBehaviour
{
    public Tile currentTile        {get; private set;}
    public int  currentDashCharges {get; private set;}

    Character _character;
    CharacterModel _model;

    CharacterStateComponent _stateComponent;
    CharacterFlagComponent  _flagComponent;

    Vector2DInt _lastMoveDirection = Vector2DInt.Up;
	
    public void ManualAwake()
    {
        _character      = GetComponent<Character>();
        _model			= _character.model;

        _stateComponent = _character.stateComponent;
        _flagComponent  = _character.flagComponent;

		_character.OnCharacterSpawned += (Vector2DInt inSpawnTile) =>
		{
			currentTile = Level.instance.tileMap.GetTile(inSpawnTile);
			currentTile.data.SetCharacter(_character);
		};
    }

    public void TryWalk(Vector2DInt inDirection)
    {
        if (_stateComponent.currentState != CharacterState.Idle || _flagComponent.GetFlag(CharacterFlag.Cooldown_Walk))
            return;

		// cant walk to tile if occupied by other player or if not walkable tile
		Tile targetTile = currentTile.data.GetRelativeTile(inDirection);
		if (targetTile.data.IsOccupied() || !targetTile.model.data.walkable)
			return;

		photonView.RPC("NetworkWalk", PhotonTargets.All, currentTile.data.position.x, currentTile.data.position.y, targetTile.data.position.x, targetTile.data.position.y, transform.rotation.x, transform.rotation.y, transform.rotation.z);
    }
	
    public void TryCharge()
    {
        if (_stateComponent.currentState != CharacterState.Idle || _flagComponent.GetFlag(CharacterFlag.Cooldown_Dash))
            return;

		photonView.RPC("NetworkCharge", PhotonTargets.Others); // send to all other then me, we start coroutine instead

        Timing.RunCoroutineSingleton(_Charge(), gameObject.GetInstanceID(), SingletonBehavior.Overwrite);
    }

	public void OnGettingDashed(Vector2DInt startTile, Vector2DInt direction, int hitPower, Vector3 rot)
	{
		photonView.RPC("NetworkOnGettingDashed", PhotonTargets.All, startTile.x, startTile.y, direction.x, direction.y, hitPower, rot.x, rot.y, rot.z);
	}

	public void OnDashingOther(Vector2DInt lastTile, Vector3 rot)
	{
		photonView.RPC("NetworkOnDashingOther", PhotonTargets.All, lastTile.x, lastTile.y, rot.x, rot.y, rot.z);
	}

	[PunRPC]
	void NetworkWalk(int fromX, int fromY, int toX, int toY, float rotX, float rotY, float rotZ)
	{
		transform.rotation = Quaternion.Euler(new Vector3(rotX, rotY, rotZ)); // set rotation to start rotation right away (should be lerped if desync)
		Timing.RunCoroutineSingleton(_Walk(new Vector2DInt(fromX, fromY), new Vector2DInt(toX, toY)), gameObject.GetInstanceID(), SingletonBehavior.Overwrite);
	}

	[PunRPC]
	void NetworkCharge()
	{
		_stateComponent.SetState(CharacterState.Charging);
		_character.view.GetComponent<Renderer>().material.color = Color.red; // temp for feedback when charging
	}

	[PunRPC]
	void NetworkDash(int inDirectionX, int inDirectionY, int inDashCharges)
	{
		Timing.RunCoroutineSingleton(_Dash(new Vector2DInt(inDirectionX, inDirectionY), inDashCharges), gameObject.GetInstanceID(), SingletonBehavior.Overwrite);
	}

	[PunRPC]
	void NetworkOnGettingDashed(int pX, int pY, int dX, int dY, int numDashtiles, float rX, float rY, float rZ)
	{
		Timing.KillCoroutines(gameObject.GetInstanceID());                     // kill all coroutines on this layer

		currentTile.data.RemovePlayer();                                       // remove player from current tile, (can be different then the tile from the server if desynced)
		currentTile = Level.instance.tileMap.GetTile(new Vector2DInt(pX, pY)); // get tile we was on when getting hit by other player
		currentTile.data.SetCharacter(_character);                             // set our reference on this tile

		transform.position = new Vector3(pX, 1, pY);                           // set pos where it should be
		transform.rotation = Quaternion.Euler(new Vector3(rX, rY, rZ));        // just set rotation to where it should be, this could be interpolated to

		Timing.RunCoroutineSingleton(_Dash(new Vector2DInt(dX, dY), numDashtiles), gameObject.GetInstanceID(), SingletonBehavior.Overwrite); // takes over the dashPower from the player that dashed into us
	}

	[PunRPC]
	void NetworkOnDashingOther(int x, int y, float rX, float rY, float rZ)
	{
		Timing.KillCoroutines(gameObject.GetInstanceID());                     // kill all coroutines on this layer

		currentTile.data.RemovePlayer();                                       // remove player from current tile, (can be different then the tile from the server if desynced)
		currentTile = Level.instance.tileMap.GetTile(new Vector2DInt(x, y));   // get tile we was on when getting hit by other player
		currentTile.data.SetCharacter(_character);                             // set our reference on this tile

		transform.position = new Vector3(x, 1, y);                             // this should to be interpolated, should only be able to be off by very little so is not as noticible as the player that gets dashed
		transform.rotation = Quaternion.Euler(new Vector3(rX, rY, rZ)); 

		// reset states and add cooldowns
		_stateComponent.SetState(CharacterState.Idle);
		_flagComponent.SetFlag(CharacterFlag.Cooldown_Walk, true, _model.walkCooldown, SingletonBehavior.Overwrite);
		_flagComponent.SetFlag(CharacterFlag.Cooldown_Dash, true, _model.dashCooldown, SingletonBehavior.Overwrite);
	}
		
	IEnumerator<float> _Walk(Vector2DInt inFromTile, Vector2DInt inToTile)
	{
		_stateComponent.SetState(CharacterState.Walking);

		if (!currentTile.model.data.unbreakable)
			currentTile.data.DamageTile();

		TileMap tileMap = Level.instance.tileMap;

		Tile fromTile   = tileMap.GetTile(inFromTile);
		Tile targetTile = tileMap.GetTile(inToTile);

		if (targetTile == null)
			throw new Exception("Tried to walk onto a tile that is null.");

		// Calculate lerp positions
		Vector3 fromPosition   = new Vector3(fromTile.data.position.x, 1, fromTile.data.position.y);
		Vector3 targetPosition = new Vector3(targetTile.data.position.x, 1, targetTile.data.position.y);

		// Calculate lerp rotations
		Vector3 movementDirection      = (targetPosition - fromPosition).normalized;
		Vector3 movementDirectionRight = new Vector3(movementDirection.z, movementDirection.y, -movementDirection.x);
		Quaternion fromRotation        = transform.rotation;
		Quaternion targetRotation      = Quaternion.Euler(movementDirectionRight * 90) * transform.rotation;

		// Save last move direction if we would do dash and not give any direction during chargeup
		_lastMoveDirection = new Vector2DInt((int)movementDirection.x, (int)movementDirection.z);

		// Update tile player references NOTE: this is done right when a player starts moving to avoid players being able to move to the same tile (gives the same teleport bug as in the original game when being dashed in middle of movement, maybe can be solved with interpolation to look ok?) or we have to solve this in other way
		currentTile.data.RemovePlayer();
		targetTile.data.SetCharacter(_character);
		currentTile = targetTile;

		// do the movement itself
		float movementProgress = 0;		
		while (movementProgress < 1)
		{
			movementProgress += _model.walkSpeed * Time.deltaTime;
			movementProgress = Mathf.Clamp01(movementProgress);

			transform.position = Vector3.Lerp(fromPosition, targetPosition, movementProgress);
			transform.position = new Vector3(transform.position.x, 1 + Mathf.Sin(movementProgress * (float)Math.PI), transform.position.z);

			transform.rotation = Quaternion.Lerp(fromRotation, targetRotation, movementProgress);			

			yield return Timing.WaitForOneFrame;
		}

		// check if we ended up on deadly tile
		if (currentTile.model.typeName == Constants.EDGE_TYPE || currentTile.model.data.deadly)
		{
			_stateComponent.SetState(CharacterState.Dead);
			yield break;
		}

		// reset state and add cooldown
		_stateComponent.SetState(CharacterState.Idle);
		_flagComponent.SetFlag(CharacterFlag.Cooldown_Walk, true, _model.walkCooldown, SingletonBehavior.Overwrite);
	}

	IEnumerator<float> _Charge()
    {
        _stateComponent.SetState(CharacterState.Charging); 

		_character.view.GetComponent<Renderer>().material.color = Color.red; // temp for feedback when charging

		float chargeAmount = _model.dashMinCharge;

        while (Input.GetButton(Constants.BUTTON_CHARGE))
        {
            chargeAmount += (_model.dashChargeRate * Time.deltaTime);
            chargeAmount = Mathf.Clamp(chargeAmount, _model.dashMinCharge, _model.dashMaxCharge);

			// while charging direction can be changed
			if (Input.GetAxisRaw(Constants.AXIS_VERTICAL) > 0)
				_lastMoveDirection = Vector2DInt.Up;
			if (Input.GetAxisRaw(Constants.AXIS_VERTICAL) < 0)
				_lastMoveDirection = Vector2DInt.Down;
			if (Input.GetAxisRaw(Constants.AXIS_HORIZONTAL) < 0)
				_lastMoveDirection = Vector2DInt.Left;
			if (Input.GetAxisRaw(Constants.AXIS_HORIZONTAL) > 0)
				_lastMoveDirection = Vector2DInt.Right;

			currentDashCharges = (int)chargeAmount; 

            yield return Timing.WaitForOneFrame;
        }

		photonView.RPC("NetworkDash", PhotonTargets.All, _lastMoveDirection.x, _lastMoveDirection.y, currentDashCharges);        
    }

    public IEnumerator<float> _Dash(Vector2DInt inDirection, int inDashStrength)
    {
        _stateComponent.SetState(CharacterState.Dashing); // set state to dashing

		_character.view.GetComponent<Renderer>().material.color = _character.color; // temp for feedback when charging

		// loop over all dash charges
		for (int i = 0; i < inDashStrength; i++)
		{			
			// get next tile in dash path
			Tile targetTile = currentTile.data.GetRelativeTile(inDirection);
			if (targetTile == null)
				throw new Exception("Tried to dash onto a tile that is null.");

			// abort dash if running into non walkable tile
			if (!targetTile.model.data.walkable)
				yield break;

			// Calculate lerp positions
			Vector3 fromPosition   = new Vector3(currentTile.data.position.x, 1, currentTile.data.position.y);
			Vector3 targetPosition = new Vector3(targetTile.data.position.x,  1, targetTile.data.position.y);

			// Calculate lerp rotations
			Vector3 movementDirection = (targetPosition - fromPosition).normalized;
			Quaternion fromRotation   = transform.rotation;
			Quaternion targetRotation = Quaternion.Euler(movementDirection * (90 * _model.dashRotationSpeed)) * transform.rotation;						

			if (PhotonNetwork.isMasterClient) // do collision on master client
			{
				if (targetTile.data.IsOccupied())
				{
					// get occupying player and tell it to send an rpc that it got dashed
					Character playerToDash = targetTile.data.GetOccupyingPlayer();
					playerToDash.movementComponent.OnGettingDashed(targetTile.data.position, inDirection, inDashStrength - i, fromRotation.eulerAngles);

					// send rpc that we hit other player and cancel all our current movement
					OnDashingOther(currentTile.data.position, fromRotation.eulerAngles);
					yield break;
				}
			}

			if (targetTile.data.IsOccupied()) // stop movement no matter who we are, clients will have time to contiunue movement for several tiles otherwise and be teleported back when the server is done(don't now how we should handle this)
				yield break;

			// hurt tile if it is destructible
			if (!currentTile.model.data.unbreakable)
				currentTile.data.DamageTile();

			// Update tile player references NOTE: this is done right when a player starts moving to avoid players being able to move to the same tile (gives the same teleport bug as in the original game when being dashed in middle of movement, maybe can be solved with interpolation to look ok?) or we have to solve this in other way
			currentTile.data.RemovePlayer();
			targetTile.data.SetCharacter(_character);
			currentTile = targetTile;			

			// do the movement itself
			float movementProgress = 0;
			while (movementProgress < 1)
			{
				movementProgress += _model.dashSpeed * Time.deltaTime;
				movementProgress = Mathf.Clamp01(movementProgress);

				transform.position = Vector3.Lerp(fromPosition, targetPosition, movementProgress);
				transform.rotation = Quaternion.Lerp(fromRotation, targetRotation, movementProgress);
				
				yield return Timing.WaitForOneFrame;
			}

			// check for edgetile after every tile dashed
			if (currentTile.model.typeName == Constants.EDGE_TYPE)
			{
				_stateComponent.SetState(CharacterState.Dead);
				yield break;
			}
		}

		// check if we ended up on deadly tile
		if (currentTile.model.data.deadly)
		{
			_stateComponent.SetState(CharacterState.Dead);
			yield break;
		}

		// reset state and add cooldowns
		_stateComponent.SetState(CharacterState.Idle);
        _flagComponent.SetFlag(CharacterFlag.Cooldown_Walk, true, _model.walkCooldown, SingletonBehavior.Overwrite);
        _flagComponent.SetFlag(CharacterFlag.Cooldown_Dash, true, _model.dashCooldown, SingletonBehavior.Overwrite);
    }

}
