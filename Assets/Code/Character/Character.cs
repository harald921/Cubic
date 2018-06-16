using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovementComponent))]
[RequireComponent(typeof(CharacterFlagComponent))]
[RequireComponent(typeof(CharacterStateComponent))]
public class Character : MonoBehaviour
{
    // Will be renamed to "Character" when it's fully transfered

    public CharacterModel model {get; private set;}
    public GameObject     view  {get; private set;} 

    public CharacterMovementComponent movementComponent {get; private set;}
    public CharacterFlagComponent     flagComponent     {get; private set;}
    public CharacterStateComponent    stateComponent    {get; private set;}

    public event Action<Tile> OnCharacterSpawned;


    public void Initialize(CharacterModel inModel, string inViewName)
    {
        model = inModel;


        movementComponent = GetComponent<CharacterMovementComponent>();
        flagComponent     = GetComponent<CharacterFlagComponent>();
        stateComponent    = GetComponent<CharacterStateComponent>();

        movementComponent.ManualAwake();
        flagComponent.ManualAwake();
        stateComponent.ManualAwake();

        // Setup the correct view, probably in a view component
        view = GameObject.CreatePrimitive(PrimitiveType.Cube);
        view.transform.SetParent(transform, false);

#if DEBUG_TOOLS
		FindObjectOfType<PlayerPage>().Initialize(this);
#endif
	}

    public void Spawn(Tile inSpawnTile)
    {
        transform.position = new Vector3(inSpawnTile.data.position.x, 1, inSpawnTile.data.position.y);
        OnCharacterSpawned?.Invoke(inSpawnTile);
    }

	void Update()
	{
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
