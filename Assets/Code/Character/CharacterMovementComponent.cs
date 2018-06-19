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

		photonView.RPC("NetworkWalk", PhotonTargets.All, inDirection.x, inDirection.y);
    }
	
    public void TryCharge()
    {
        if (_stateComponent.currentState != CharacterState.Idle || _flagComponent.GetFlag(CharacterFlag.Cooldown_Dash))
            return;

		photonView.RPC("NetworkCharge", PhotonTargets.Others); // send to all other then me, we start coroutine instead

        Timing.RunCoroutineSingleton(_Charge(), gameObject.GetInstanceID(), SingletonBehavior.Abort);
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
	void NetworkWalk(int inDirectionX, int inDirectionY)
	{
		Timing.RunCoroutineSingleton(_Walk(new Vector2DInt(inDirectionX, inDirectionY)), gameObject.GetInstanceID(), SingletonBehavior.Abort);
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
		Timing.RunCoroutine(_Dash(new Vector2DInt(inDirectionX, inDirectionY), inDashCharges), gameObject.GetInstanceID());
	}

	[PunRPC]
	void NetworkOnGettingDashed(int pX, int pY, int dX, int dY, int numDashtiles, float rX, float rY, float rZ)
	{
		Timing.KillCoroutines(gameObject.GetInstanceID());

		currentTile.data.RemovePlayer();
		currentTile = Level.instance.tileMap.GetTile(new Vector2DInt(pX, pY));
		currentTile.data.SetCharacter(_character);
		transform.position = new Vector3(pX, 1, pY);
		transform.rotation = Quaternion.Euler(new Vector3(rX, rY, rZ));

		Timing.RunCoroutine(_Dash(new Vector2DInt(dX, dY), numDashtiles), gameObject.GetInstanceID());
	}

	[PunRPC]
	void NetworkOnDashingOther(int x, int y, float rX, float rY, float rZ)
	{
		Timing.KillCoroutines(gameObject.GetInstanceID());

		currentTile.data.RemovePlayer();
		currentTile = Level.instance.tileMap.GetTile(new Vector2DInt(x, y));
		currentTile.data.SetCharacter(_character);
		transform.position = new Vector3(x, 1, y);
		transform.rotation = Quaternion.Euler(new Vector3(rX, rY, rZ));

		_stateComponent.SetState(CharacterState.Idle);
		_flagComponent.SetFlag(CharacterFlag.Cooldown_Walk, true, _model.walkCooldown, SingletonBehavior.Overwrite);
		_flagComponent.SetFlag(CharacterFlag.Cooldown_Dash, true, _model.dashCooldown, SingletonBehavior.Overwrite);
	}
		
	IEnumerator<float> _Walk(Vector2DInt inDirection)
	{
		_stateComponent.SetState(CharacterState.Walking);

		if (!currentTile.model.data.unbreakable)
			currentTile.data.DamageTile();

		Tile targetTile = currentTile.data.GetRelativeTile(inDirection);
		if (targetTile == null)
			throw new Exception("Tried to walk onto a tile that is null.");

		// Calculate lerp positions
		Vector3 fromPosition = new Vector3(currentTile.data.position.x, 1, currentTile.data.position.y);
		Vector3 targetPosition = new Vector3(targetTile.data.position.x, 1, targetTile.data.position.y);

		// Calculate lerp rotations
		Vector3 movementDirection = (targetPosition - fromPosition).normalized;
		Vector3 movementDirectionRight = new Vector3(movementDirection.z, movementDirection.y, -movementDirection.x);

		Quaternion fromRotation = transform.rotation;
		Quaternion targetRotation = Quaternion.Euler(movementDirectionRight * 90) * transform.rotation;

		// Save last move direction if we would do dash and not give any direction during chargeup
		_lastMoveDirection = new Vector2DInt((int)movementDirection.x, (int)movementDirection.z);

		float movementProgress = 0;
		bool tileRefSet = false;
		while (movementProgress < 1)
		{
			movementProgress += _model.walkSpeed * Time.deltaTime;
			movementProgress = Mathf.Clamp01(movementProgress);

			transform.position = Vector3.Lerp(fromPosition, targetPosition, movementProgress);
			transform.position = new Vector3(transform.position.x, 1 + Mathf.Sin(movementProgress * (float)Math.PI), transform.position.z);

			transform.rotation = Quaternion.Lerp(fromRotation, targetRotation, movementProgress);

			if (movementProgress > 0.5f && !tileRefSet)
			{
				// Set player tile references
				Tile previousTile = currentTile;
				currentTile = targetTile;

				// Update tile player references
				previousTile.data.RemovePlayer();
				targetTile.data.SetCharacter(_character);

				tileRefSet = true;
			}

			yield return Timing.WaitForOneFrame;
		}

		if (currentTile.model.typeName == Constants.EDGE_TYPE || currentTile.model.data.deadly)
		{
			_stateComponent.SetState(CharacterState.Dead);
			yield break;
		}

		_stateComponent.SetState(CharacterState.Idle);
		_flagComponent.SetFlag(CharacterFlag.Cooldown_Walk, true, _model.walkCooldown, SingletonBehavior.Overwrite);
	}

	IEnumerator<float> _Charge()
    {
        _stateComponent.SetState(CharacterState.Charging); // TODO: Make this happen via events instead (I can fix that -Harald)

		_character.view.GetComponent<Renderer>().material.color = Color.red; // temp for feedback when charging

		float chargeAmount = _model.dashMinCharge;

        while (Input.GetKey(KeyCode.Space))
        {
            chargeAmount += (_model.dashChargeRate * Time.deltaTime);
            chargeAmount = Mathf.Clamp(chargeAmount, _model.dashMinCharge, _model.dashMaxCharge);

            // while charging direction can be changed
            if (Input.GetKey(KeyCode.W))
                _lastMoveDirection = Vector2DInt.Up;
            if (Input.GetKey(KeyCode.S))
                _lastMoveDirection = Vector2DInt.Down;
            if (Input.GetKey(KeyCode.A))
                _lastMoveDirection = Vector2DInt.Left;
            if (Input.GetKey(KeyCode.D))
                _lastMoveDirection = Vector2DInt.Right;

            currentDashCharges = (int)chargeAmount; // TODO: Send with event (Same here -Harald)

            yield return Timing.WaitForOneFrame;
        }

		photonView.RPC("NetworkDash", PhotonTargets.All, _lastMoveDirection.x, _lastMoveDirection.y, currentDashCharges);        
    }

    public IEnumerator<float> _Dash(Vector2DInt inDirection, int inDashStrength)
    {
        _stateComponent.SetState(CharacterState.Dashing);

		_character.view.GetComponent<Renderer>().material.color = Color.white; // temp for feedback when charging

		for (int i = 0; i < inDashStrength; i++)
		{
			// hurt tile if it is destructible
			if (!currentTile.model.data.unbreakable)
				currentTile.data.DamageTile();

			Tile targetTile = currentTile.data.GetRelativeTile(inDirection);
			if (targetTile == null)
				throw new Exception("Tried to dash onto a tile that is null.");

			// Calculate lerp positions
			Vector3 fromPosition = new Vector3(currentTile.data.position.x, 1, currentTile.data.position.y);
			Vector3 targetPosition = new Vector3(targetTile.data.position.x, 1, targetTile.data.position.y);

			// Calculate lerp rotations
			Vector3 movementDirection = (targetPosition - fromPosition).normalized;

			Quaternion fromRotation = transform.rotation;
			Quaternion targetRotation = Quaternion.Euler(movementDirection * (90 * _model.dashRotationSpeed)) * transform.rotation;

			float movementProgress = 0;
			bool tileRefSet = false;

			if (PhotonNetwork.isMasterClient) // do collision on master client
			{
				if (targetTile.data.IsOccupied())
				{
					Character playerToDash = targetTile.data.GetOccupyingPlayer();
					playerToDash.movementComponent.OnGettingDashed(targetTile.data.position, inDirection, inDashStrength - i, fromRotation.eulerAngles);

					OnDashingOther(currentTile.data.position, fromRotation.eulerAngles);
				}
			}

			if (targetTile.data.IsOccupied()) // stop movement no matter who we are, clients will have time to contiunue movement for several tiles otherwise and be teleported back when the server is done
				yield break;

			while (movementProgress < 1)
			{
				movementProgress += _model.dashSpeed * Time.deltaTime;
				movementProgress = Mathf.Clamp01(movementProgress);

				transform.position = Vector3.Lerp(fromPosition, targetPosition, movementProgress);
				transform.rotation = Quaternion.Lerp(fromRotation, targetRotation, movementProgress);

				if (movementProgress > 0.5f && !tileRefSet)
				{
					// Set player tile references
					Tile previousTile = currentTile;
					currentTile = targetTile;

					// Update tile player references
					previousTile.data.RemovePlayer();
					targetTile.data.SetCharacter(_character);

					tileRefSet = true;
				}

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

		_stateComponent.SetState(CharacterState.Idle);
        _flagComponent.SetFlag(CharacterFlag.Cooldown_Walk, true, _model.walkCooldown, SingletonBehavior.Overwrite);
        _flagComponent.SetFlag(CharacterFlag.Cooldown_Dash, true, _model.dashCooldown, SingletonBehavior.Overwrite);
    }

}
