using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovementComponent))]
[RequireComponent(typeof(CharacterFlagComponent))]
[RequireComponent(typeof(CharacterStateComponent))]
[RequireComponent(typeof(CharacterSoundComponent))]
public class Character : Photon.MonoBehaviour
{
    // Will be renamed to "Character" when it's fully transfered
	public Color color          {get; private set;}
    public CharacterModel model {get; private set;}
    public GameObject     view  {get; private set;} 
	public bool isMasterClient  {get; private set;}
	public int playerID         {get; private set;}

    public CharacterMovementComponent movementComponent {get; private set;}
    public CharacterFlagComponent     flagComponent     {get; private set;}
    public CharacterStateComponent    stateComponent    {get; private set;}
	public CharacterSoundComponent    soundComponent    {get; private set;}

	public event Action<Vector2DInt> OnCharacterSpawned;

	public void Initialize(string inViewName, int inPlayerId)
    {
		isMasterClient = PhotonNetwork.isMasterClient;
		photonView.RPC("NetworkInitialize", PhotonTargets.AllBuffered, inViewName, inPlayerId); // wont need be buffered later when level loading is synced
	}

	public void Spawn(Vector2DInt inSpawnTile)
	{
		photonView.RPC("NetworkSpawn", PhotonTargets.AllBuffered, inSpawnTile.x, inSpawnTile.y); // wont need be buffered later when level loading is synced
	}

	[PunRPC]
	void NetworkInitialize(string inViewName, int inPlayerId)
	{
		playerID = inPlayerId;

		model = CharacterDatabase.instance.standardModel;
		CharacterDatabase.ViewData vData = CharacterDatabase.instance.GetViewFromName(inViewName);

		// Setup the correct view, probably in a view component	
		view = Instantiate(vData.prefab);
		view.transform.SetParent(transform, false);

		// gat components
		movementComponent = GetComponent<CharacterMovementComponent>();
		flagComponent     = GetComponent<CharacterFlagComponent>();
		stateComponent    = GetComponent<CharacterStateComponent>();
		soundComponent    = GetComponent<CharacterSoundComponent>();

		// initialize components
		movementComponent.ManualAwake();
		flagComponent.ManualAwake();
		stateComponent.ManualAwake();
		soundComponent.ManualAwake(vData, view.transform);

		GetOriginalColor();

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

	void GetOriginalColor()
	{
		// temp way of getting original material color, wont be used later at all
		Renderer r = view.GetComponent<Renderer>();
		if (r == null)
			r = view.transform.GetChild(0).GetComponent<Renderer>();

		color = r.material.color;
	}
	
	void Update()
	{
		if (!photonView.isMine || !Match.instance.matchStarted)
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

		if (Input.GetKeyDown(KeyCode.L))
			soundComponent.PlaySound(CharacterSoundComponent.CharacterSound.Dash);

#endif
	}
}
