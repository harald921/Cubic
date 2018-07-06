using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovementComponent))]
[RequireComponent(typeof(CharacterFlagComponent))]
[RequireComponent(typeof(CharacterStateComponent))]
public class Character : Photon.MonoBehaviour
{
    // Will be renamed to "Character" when it's fully transfered
	public Color color          {get; private set;}
    public CharacterModel model {get; private set;}
    public GameObject     view  {get; private set;} 
	public bool isMasterClient  {get; private set;}

    public CharacterMovementComponent movementComponent {get; private set;}
    public CharacterFlagComponent     flagComponent     {get; private set;}
    public CharacterStateComponent    stateComponent    {get; private set;}

    public event Action<Vector2DInt> OnCharacterSpawned;

	public void Initialize(string inViewName)
    {
		isMasterClient = PhotonNetwork.isMasterClient;
		photonView.RPC("NetworkInitialize", PhotonTargets.AllBuffered, inViewName); // wont need be buffered later when level loading is synced
	}

	public void Spawn(Vector2DInt inSpawnTile)
	{
		photonView.RPC("NetworkSpawn", PhotonTargets.AllBuffered, inSpawnTile.x, inSpawnTile.y); // wont need be buffered later when level loading is synced
	}

	[PunRPC]
	void NetworkInitialize(string inViewName)
	{
		model = CharacterDatabase.instance.standardModel;
		movementComponent = GetComponent<CharacterMovementComponent>();
		flagComponent     = GetComponent<CharacterFlagComponent>();
		stateComponent    = GetComponent<CharacterStateComponent>();

		movementComponent.ManualAwake();
		flagComponent.ManualAwake();
		stateComponent.ManualAwake();

		// Setup the correct view, probably in a view component
		view = Instantiate(CharacterDatabase.instance.GetViewFromName(inViewName));
		view.transform.SetParent(transform, false);

		color = view.GetComponent<Renderer>().material.color;

#if DEBUG_TOOLS
		if (photonView.isMine)
			FindObjectOfType<PlayerPage>().Initialize(this);
#endif
	}

	[PunRPC]
	void NetworkSpawn(int inSpawnTileX, int inSpawnTileY)
	{
		transform.position = new Vector3(inSpawnTileX, 1, inSpawnTileY);
		OnCharacterSpawned?.Invoke(new Vector2DInt(inSpawnTileX, inSpawnTileY));
		stateComponent.SetState(CharacterState.Idle);

	}
	
	void Update()
	{
		if (!photonView.isMine)
			return;

		if (Input.GetButton(Constants.BUTTON_CHARGE))
			movementComponent.TryCharge();

		if (Input.GetAxisRaw(Constants.AXIS_VERTICAL) > 0)
			movementComponent.TryWalk(Vector2DInt.Up);
		if (Input.GetAxisRaw(Constants.AXIS_VERTICAL) < 0)
			movementComponent.TryWalk(Vector2DInt.Down);
		if (Input.GetAxisRaw(Constants.AXIS_HORIZONTAL) < 0)
			movementComponent.TryWalk(Vector2DInt.Left);
		if (Input.GetAxisRaw(Constants.AXIS_HORIZONTAL) > 0)
			movementComponent.TryWalk(Vector2DInt.Right);

#if DEBUG_TOOLS

		if (Input.GetKeyDown(KeyCode.P))
			movementComponent.InfiniteDash();
#endif
	}
}
