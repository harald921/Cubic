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

    public CharacterModel model {get; private set;}
    public GameObject     view  {get; private set;} 

    public CharacterMovementComponent movementComponent {get; private set;}
    public CharacterFlagComponent     flagComponent     {get; private set;}
    public CharacterStateComponent    stateComponent    {get; private set;}

    public event Action<Vector2DInt> OnCharacterSpawned;

	public void Initialize(string inViewName)
    {
		photonView.RPC("NetworkInitialize", PhotonTargets.AllBuffered, inViewName); // wont need be buffered later when level loading is synced
	}

	public void Spawn(Vector2DInt inSpawnTile)
	{
		photonView.RPC("NetworkSpawn", PhotonTargets.AllBuffered, inSpawnTile.x, inSpawnTile.y); // wont need be buffered later when level loading is synced
	}

	[PunRPC]
	void NetworkInitialize(string inViewNam)
	{
		model = CharacterDatabase.instance.standardModel;
		movementComponent = GetComponent<CharacterMovementComponent>();
		flagComponent = GetComponent<CharacterFlagComponent>();
		stateComponent = GetComponent<CharacterStateComponent>();

		movementComponent.ManualAwake();
		flagComponent.ManualAwake();
		stateComponent.ManualAwake();

		// Setup the correct view, probably in a view component
		view = GameObject.CreatePrimitive(PrimitiveType.Cube);
		view.transform.SetParent(transform, false);

#if DEBUG_TOOLS
		if(photonView.isMine)
			FindObjectOfType<PlayerPage>().Initialize(this);
#endif
	}

	[PunRPC]
	void NetworkSpawn(int inSpawnTileX, int inSpawnTileY)
	{
		transform.position = new Vector3(inSpawnTileX, 1, inSpawnTileY);
		OnCharacterSpawned?.Invoke(new Vector2DInt(inSpawnTileX, inSpawnTileY));
	}
	
	void Update()
	{
		if (!photonView.isMine)
			return;

		if (Input.GetKey(KeyCode.Space))
			movementComponent.TryCharge();

		if (Input.GetKey(KeyCode.W))
			movementComponent.TryWalk(Vector2DInt.Up);
		if (Input.GetKey(KeyCode.S))
			movementComponent.TryWalk(Vector2DInt.Down);
		if (Input.GetKey(KeyCode.A))
			movementComponent.TryWalk(Vector2DInt.Left);
		if (Input.GetKey(KeyCode.D))
			movementComponent.TryWalk(Vector2DInt.Right);
	}
}
