using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using Photon;

public class CharacterMovementComponent : Photon.MonoBehaviour
{
	public Tile currentTile       { get; private set; }
	public int currentDashCharges { get; private set; }

	Character _character;
	CharacterModel _model;

	CharacterStateComponent _stateComponent;
	CharacterFlagComponent _flagComponent;

	Vector2DInt _lastMoveDirection = Vector2DInt.Up;

	CollisionTracker _collisionTracker;

	Quaternion _lastTargetRotation; // used to keep track of the rotation the player last was targeting when getting interupted by getting hit

	public void ManualAwake()
	{
		_character = GetComponent<Character>();
		_model = _character.model;

		_stateComponent = _character.stateComponent;
		_flagComponent = _character.flagComponent;

		_collisionTracker = FindObjectOfType<CollisionTracker>();

		_lastTargetRotation = transform.rotation;

		_character.OnCharacterSpawned += (Vector2DInt inSpawnTile) =>
		{
			currentTile = Match.instance.level.tileMap.GetTile(inSpawnTile);
			currentTile.data.SetCharacter(_character);
		};
	}

	void OnDestroy()
	{
		Timing.KillCoroutines(gameObject.GetInstanceID());
	}

	public void TryWalk(Vector2DInt direction)
	{
		if (_stateComponent.currentState != CharacterState.Idle || _flagComponent.GetFlag(CharacterFlag.Cooldown_Walk))
			return;

		// cant walk to tile if occupied by other player or if not walkable tile
		Tile targetTile = currentTile.data.GetRelativeTile(direction);
		if (targetTile.data.IsOccupied() || !targetTile.model.data.walkable)
			return;

		photonView.RPC("NetworkWalk", PhotonTargets.All, currentTile.data.position.x, currentTile.data.position.y, targetTile.data.position.x, targetTile.data.position.y);
	}

	public void TryCharge()
	{
		if (_stateComponent.currentState != CharacterState.Idle || _flagComponent.GetFlag(CharacterFlag.Cooldown_Dash))
			return;

		photonView.RPC("NetworkCharge", PhotonTargets.Others); // send to all other then me, we start coroutine instead

		Timing.RunCoroutineSingleton(_Charge(), gameObject.GetInstanceID(), SingletonBehavior.Overwrite);
	}

	public void OnGettingDashed(Vector2DInt startTile, Vector2DInt direction, int hitPower)
	{
		photonView.RPC("NetworkOnGettingDashed", PhotonTargets.All, startTile.x, startTile.y, direction.x, direction.y, hitPower);
	}

	public void OnDashingOther(Vector2DInt lastTile, Quaternion rot, Vector2DInt targetTile)
	{
		photonView.RPC("NetworkOnDashingOther", PhotonTargets.All, lastTile.x, lastTile.y, targetTile.x, targetTile.y, rot.x, rot.y, rot.z, rot.w);
	}

	[PunRPC]
	void NetworkWalk(int fromX, int fromY, int toX, int toY)
	{
		Timing.RunCoroutineSingleton(_Walk(new Vector2DInt(fromX, fromY), new Vector2DInt(toX, toY)), gameObject.GetInstanceID(), SingletonBehavior.Overwrite);
	}

	[PunRPC]
	void NetworkCharge()
	{
		_stateComponent.SetState(CharacterState.Charging);
		_character.ParticleComponent.EmitCharge(true);
		_character.soundComponent.PlaySound(CharacterSoundComponent.CharacterSound.Charge);
		ChangeColor(Color.red, _character.view); // temp for feedback when charging
	}

	[PunRPC]
	void NetworkDash(int fromX, int fromY, int directionX, int directionY, int dashCharges)
	{
		// set new current tile if desynced, should never happen becuase of chargetime before dash (maybe on quickdash?)
		SetNewTileReferences(new Vector2DInt(fromX, fromY));

		Timing.RunCoroutineSingleton(_Dash(new Vector2DInt(directionX, directionY), dashCharges), gameObject.GetInstanceID(), SingletonBehavior.Overwrite);
	}

	[PunRPC]
	void NetworkOnGettingDashed(int fromX, int fromY, int directionX, int directionY, int numDashtiles)
	{
		// kill all coroutines on this layer
		Timing.KillCoroutines(gameObject.GetInstanceID());

		// set new current tile if desynced
		SetNewTileReferences(new Vector2DInt(fromX, fromY));

		Timing.RunCoroutineSingleton(_Dash(new Vector2DInt(directionX, directionY), numDashtiles, true), gameObject.GetInstanceID(), SingletonBehavior.Overwrite); // takes over the dashPower from the player that dashed into us
	}

	[PunRPC]
	void NetworkOnDashingOther(int fromX, int fromY, int targetX, int targetY, float rotX, float rotY, float rotZ, float rotW)
	{
		// kill all coroutines on this layer
		Timing.KillCoroutines(gameObject.GetInstanceID());

		// play hit sound and spawn effect
		_character.soundComponent.PlaySound(CharacterSoundComponent.CharacterSound.Punch);
		_character.ParticleComponent.SpawnHitEffect(new Vector2DInt(fromX, fromY), new Vector2DInt(targetX, targetY));

		// set new current tile if desynced
		SetNewTileReferences(new Vector2DInt(fromX, fromY));

		// this should not have to be interpolated becuase everyone stops locally aswell
		// used as safety if we only detects collision on server and not locally, should be very rare and be a small teleport if happens
		transform.position = new Vector3(fromX, 1, fromY);
		transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);

		// set last target rotation to current rotation(we never started lerping towards target)
		_lastTargetRotation = transform.rotation;

		StopMovementAndAddCooldowns();
		_character.ParticleComponent.EmitTrail(false, Vector3.zero);

		// check if we got stopped on deadly tile
		DeadlyTile();
	}

	[PunRPC]
	public void FinishCancelledDash(int fromX, int fromY, int directionX, int directionY, int dashCharges)
	{
		// kill all coroutines on this layer
		Timing.KillCoroutines(gameObject.GetInstanceID());

		// set back tilereferences to the tile where we stopped
		SetNewTileReferences(new Vector2DInt(fromX, fromY));

		Timing.RunCoroutineSingleton(_Dash(new Vector2DInt(directionX, directionY), dashCharges, true), gameObject.GetInstanceID(), SingletonBehavior.Overwrite);
	}

	IEnumerator<float> _Walk(Vector2DInt fromTilePos, Vector2DInt toTilePos)
	{
		_stateComponent.SetState(CharacterState.Walking);

		_character.soundComponent.PlaySound(CharacterSoundComponent.CharacterSound.Walk);

		if (!currentTile.model.data.unbreakable)
			currentTile.data.DamageTile();

		TileMap tileMap = Match.instance.level.tileMap;

		Tile fromTile = tileMap.GetTile(fromTilePos);
		Tile targetTile = tileMap.GetTile(toTilePos);

		if (targetTile == null)
			throw new Exception("Tried to walk onto a tile that is null.");

		// Calculate lerp positions
		Vector3 fromPosition = new Vector3(fromTile.data.position.x, 1, fromTile.data.position.y);
		Vector3 targetPosition = new Vector3(targetTile.data.position.x, 1, targetTile.data.position.y);

		// Calculate lerp rotations
		Vector3 movementDirection = (targetPosition - fromPosition).normalized;
		Vector3 movementDirectionRight = new Vector3(movementDirection.z, movementDirection.y, -movementDirection.x);

		// do lerp from where we are even if desynced(will catch up)
		// target is calculated using the target rotation we had during last movement if we are lagging behind
		Quaternion fromRotation = transform.rotation;
		Quaternion targetRotation = Quaternion.Euler(movementDirectionRight * 90) * _lastTargetRotation;

		_lastTargetRotation = targetRotation;

		// Save last move direction if we would do dash and not give any direction during chargeup
		_lastMoveDirection = new Vector2DInt((int)movementDirection.x, (int)movementDirection.z);

		// Update tile player references NOTE: this is done right when a player starts moving to avoid players being able to move to the same tile (lerping is used when getting hit when not physiclly att target tile)
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
		if (DeadlyTile())
			yield break;

		currentTile.OnPlayerLand();

		// reset state and add cooldown
		_stateComponent.SetState(CharacterState.Idle);
		_flagComponent.SetFlag(CharacterFlag.Cooldown_Walk, true, _model.walkCooldown, SingletonBehavior.Overwrite);
	}

	IEnumerator<float> _Charge()
	{
		_stateComponent.SetState(CharacterState.Charging);

		_character.ParticleComponent.EmitCharge(true);
		_character.soundComponent.PlaySound(CharacterSoundComponent.CharacterSound.Charge);
		ChangeColor(Color.red, _character.view);		

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

		Vector2DInt currentPos = currentTile.data.position;

		photonView.RPC("NetworkDash", PhotonTargets.All, currentPos.x, currentPos.y, _lastMoveDirection.x, _lastMoveDirection.y, currentDashCharges);
	}

	public IEnumerator<float> _Dash(Vector2DInt direction, int dashStrength, bool fromCollision = false)
	{
		_stateComponent.SetState(CharacterState.Dashing); // set state to dashing

		// only play dash sound if this was a volentary dash
		if (!fromCollision)
		{
			Vector2DInt currentPos = currentTile.data.position;
			Vector2DInt targetPos  = currentTile.data.GetRelativeTile(direction).data.position;

			_character.soundComponent.PlaySound(CharacterSoundComponent.CharacterSound.Dash);
			_character.ParticleComponent.EmitTrail(true, (new Vector3(targetPos.x, 1, targetPos.y) - new Vector3(currentPos.x, 1, currentPos.y)).normalized);
		}

		ChangeColor(_character.color, _character.view);
		_character.ParticleComponent.EmitCharge(false);
		_character.soundComponent.StopSound(CharacterSoundComponent.CharacterSound.Charge);

		// loop over all dash charges
		for (int i = 0; i < dashStrength; i++)
		{
			// get next tile in dash path			
			// current tile is corrected before coroutine if lagging so this should be safe
			Tile targetTile = currentTile.data.GetRelativeTile(direction);
			if (targetTile == null)
				throw new Exception("Tried to dash onto a tile that is null.");

			// abort dash if running into non walkable tile
			if (!targetTile.model.data.walkable)
				yield break;

			// Calculate lerp positions
			Vector3 fromPosition = transform.position; // interpolate from current position to avoid teleporting if lagging
			Vector3 targetPosition = new Vector3(targetTile.data.position.x, 1, targetTile.data.position.y);

			Vector3 currentTilePos = new Vector3(currentTile.data.position.x, 1, currentTile.data.position.y);
			// Calculate lerp rotations
			// note: use last target rotation as base if we was in middle of movement when this dash started from getting hit from other player
			// this will make the rotation that was left from last movement to be caught up
			Vector3 movementDirection = (targetPosition - currentTilePos).normalized;
			Quaternion fromRotation = transform.rotation;
			Quaternion targetRotation = Quaternion.Euler(movementDirection * (90 * _model.dashRotationSpeed)) * _lastTargetRotation;

			// if we will hit someone we need the target rotation we had last time becuase we wont start moving towards the future last target
			Quaternion previousLastTargetRotation = _lastTargetRotation;

			_lastTargetRotation = targetRotation;


			if (PhotonNetwork.isMasterClient) // do collision on master client
			{
				if (targetTile.data.IsOccupied())
				{
					// get occupying player and tell it to send an rpc that it got dashed
					Character playerToDash = targetTile.data.GetOccupyingPlayer();

					_collisionTracker.AddCollision(playerToDash.photonView.viewID, targetTile.data.position.x, targetTile.data.position.y);

					playerToDash.movementComponent.OnGettingDashed(targetTile.data.position, direction, dashStrength - i);

					// send rpc that we hit other player and cancel all our current movement
					OnDashingOther(currentTile.data.position, previousLastTargetRotation, targetTile.data.position);

					yield break;
				}
			}

			// stop locally aswell and dubblecheck so we stopped on server aswell, if not the server will restart our dashroutine with the charges that was left
			if (targetTile.data.IsOccupied())
			{
				StopMovementAndAddCooldowns();
				_character.ParticleComponent.EmitTrail(false, Vector3.zero);
				_collisionTracker.photonView.RPC("CheckServerCollision", PhotonTargets.MasterClient,
												targetTile.data.GetOccupyingPlayer().photonView.viewID,
												photonView.viewID, currentTile.data.position.x, currentTile.data.position.y,
												targetTile.data.position.x, targetTile.data.position.y,
												direction.x, direction.y, dashStrength - i);
				yield break;
			}

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
			if (DeadlyEdge())
				yield break;
		}

		// check if we ended up on deadly tile
		if (DeadlyTile())
			yield break;

		_character.ParticleComponent.EmitTrail(false, Vector3.zero);
		StopMovementAndAddCooldowns();
	}

	public IEnumerator<float> _sink()
	{
		while(_stateComponent.currentState == CharacterState.Dead)
		{
			transform.position += Vector3.down * _model.sinkSpeed * Time.deltaTime;
			yield return Timing.WaitForOneFrame;
		}
	}

	public void ResetAll()
	{
		Timing.KillCoroutines(gameObject.GetInstanceID());
		_stateComponent.SetState(CharacterState.Idle);
		
		transform.rotation = Quaternion.Euler(Vector3.zero);
		_lastTargetRotation = transform.rotation;
	}
		
	void StopMovementAndAddCooldowns()
	{
		// reset state and add cooldowns
		_stateComponent.SetState(CharacterState.Idle);
		_flagComponent.SetFlag(CharacterFlag.Cooldown_Walk, true, _model.walkCooldown, SingletonBehavior.Overwrite);
		_flagComponent.SetFlag(CharacterFlag.Cooldown_Dash, true, _model.dashCooldown, SingletonBehavior.Overwrite);
	}

	void SetNewTileReferences(Vector2DInt tile)
	{
		// remove old reference and set to new
		currentTile.data.RemovePlayer();                                      
		currentTile = Match.instance.level.tileMap.GetTile(tile);                 
		currentTile.data.SetCharacter(_character);                            																																																																																																																																																																																																																																																																																																																																				   
	}

	bool DeadlyTile()
	{
		if (currentTile.model.data.deadly)
			Die();

		return currentTile.model.data.deadly;
	}

	bool DeadlyEdge()
	{
		if (currentTile.model.typeName == Constants.EDGE_TYPE)
			Die();

		return currentTile.model.typeName == Constants.EDGE_TYPE;
	}

	void Die()
	{
		// remove reference and set state
		currentTile.data.RemovePlayer();
		_stateComponent.SetState(CharacterState.Dead);

		_character.soundComponent.PlaySound(CharacterSoundComponent.CharacterSound.Death);
		_character.ParticleComponent.EmitTrail(false, Vector3.zero);

		// sink to bottom
		Timing.RunCoroutineSingleton(_sink(), gameObject.GetInstanceID(), SingletonBehavior.Overwrite);

		if (PhotonNetwork.isMasterClient)		
			Match.instance.OnPlayerDie(_character.playerID);					
	}

	void ChangeColor(Color color, GameObject view)
	{
		// loops over and change color of all meshes of a gameobject
		Renderer r = view.GetComponent<Renderer>();
		if (r != null)
			r.material.color = color;
		
		for (int i = 0; i < view.transform.childCount; i++)
		{
			Renderer meshRenderer = view.transform.GetChild(i).GetComponent<Renderer>();
			if (meshRenderer)
				meshRenderer.material.color = color;
		}		
	}

#if DEBUG_TOOLS
	public void InfiniteDash()
	{
		photonView.RPC("NetworkDash", PhotonTargets.All, currentTile.data.position.x, currentTile.data.position.y, _lastMoveDirection.x, _lastMoveDirection.y, 100);
	}
#endif

}
