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
        _model = _character.model;

        _stateComponent = _character.stateComponent;
        _flagComponent  = _character.flagComponent;

        _character.OnCharacterSpawned += (Vector2DInt inSpawnTile) =>
            currentTile = Level.instance.tileMap.GetTile(inSpawnTile);
    }


    public void TryWalk(Vector2DInt inDirection)
    {
        if (_stateComponent.currentState != CharacterState.Idle || _flagComponent.GetFlag(CharacterFlag.Cooldown_Walk))
            return;

		photonView.RPC("NetworkWalk", PhotonTargets.All, inDirection.x, inDirection.y);
    }

	[PunRPC]
	void NetworkWalk(int inDirectionX, int inDirectionY)
	{
		Timing.RunCoroutineSingleton(_Walk(new Vector2DInt(inDirectionX, inDirectionY)), gameObject.GetInstanceID() + 0, SingletonBehavior.Abort);
	}

    IEnumerator<float> _Walk(Vector2DInt inDirection)
    {
        _stateComponent.SetState(CharacterState.Walking);

        if (!currentTile.model.data.unbreakable)
            currentTile.data.DamageTile();

        yield return Timing.WaitUntilDone(_WalkInterpolation(inDirection));

        if (currentTile.model.typeName == Constants.EDGE_TYPE || currentTile.model.data.deadly)
        {           
            _stateComponent.SetState(CharacterState.Dead);
            yield break;
        }

        _stateComponent.SetState(CharacterState.Idle);
        _flagComponent.SetFlag(CharacterFlag.Cooldown_Walk, true, _model.walkCooldown, SingletonBehavior.Overwrite);
    }

    IEnumerator<float> _WalkInterpolation(Vector2DInt inDirection)
    {
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
    }

    public void TryCharge()
    {
        if (_stateComponent.currentState != CharacterState.Idle || _flagComponent.GetFlag(CharacterFlag.Cooldown_Dash))
            return;

        Timing.RunCoroutineSingleton(_Charge(), gameObject.GetInstanceID() + 1, SingletonBehavior.Abort);
    }

    IEnumerator<float> _Charge()
    {
        _stateComponent.SetState(CharacterState.Charging); // TODO: Make this happen via events instead (I can fix that -Harald)

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

        Timing.RunCoroutine(_Dash(_lastMoveDirection, (int)chargeAmount));
    }

    public IEnumerator<float> _Dash(Vector2DInt inDirection, int inDashStrength)
    {
        _stateComponent.SetState(CharacterState.Dashing);

        for (int i = 0; i < inDashStrength; i++)
		{
			// hurt tile if it is destructible
			if (!currentTile.model.data.unbreakable)
				currentTile.data.DamageTile();

			// do dash movement for one tile
			yield return Timing.WaitUntilDone(_DashInterpolation(inDirection));

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

    public IEnumerator<float> _DashInterpolation(Vector2DInt inDirection)
    {
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
    }
}
