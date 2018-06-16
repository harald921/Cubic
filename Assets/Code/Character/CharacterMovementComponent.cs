using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class CharacterMovementComponent : MonoBehaviour
{
    public Tile currentTile        {get; private set;}
    public int  currentDashCharges {get; private set;}

    NewCharacter _character;

    CharacterStateComponent _stateComponent;
    CharacterFlagComponent  _flagComponent;

    Vector3 _lastMoveDirection = Vector3.forward;


    public void ManualAwake()
    {
        _character = GetComponent<NewCharacter>();
        _stateComponent = _character.stateComponent;
        _flagComponent = _character.flagComponent;

        _character.OnCharacterSpawned += (Tile inSpawnTile) =>
            currentTile = inSpawnTile;
    }


    public void TryWalk(Vector2DInt inDirection)
    {
        if (_stateComponent.currentState != CharacterState.Idle || _flagComponent.GetFlag(CharacterFlag.Cooldown_Dash))
            return;
    }

    IEnumerator<float> _Walk(Vector2DInt inDirection)
    {
        _stateComponent.SetState(CharacterState.Walking);

        if (!currentTile.model.data.unbreakable)
            currentTile.data.DamageTile();

        yield return Timing.WaitUntilDone(_WalkInterpolation(inDirection));

        if (currentTile.model.typeName == Constants.EDGE_TYPE)
        {
            Debug.Log("Character dead!");
            _stateComponent.SetState(CharacterState.Dead);
            yield break;
        }

        _stateComponent.SetState(CharacterState.Idle);
        _flagComponent.SetFlag(CharacterFlag.Cooldown_Walk, true, _character.model.walkCooldown, SingletonBehavior.Overwrite);
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
        _lastMoveDirection = movementDirection;

        float movementProgress = 0;
        bool tileRefSet = false;
        while (movementProgress < 1)
        {
            movementProgress += _character.model.walkSpeed * Time.deltaTime;
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
    }

    public void Dash(Vector2DInt inVelocity)
    {
    }
}
