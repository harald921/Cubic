using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateComponent : MonoBehaviour
{
    Character _character;

    public CharacterState currentState { get; private set; } = CharacterState.Idle;

    public void ManualAwake()
    {
        _character = GetComponent<Character>();
    }


    public void SetState(CharacterState state) => currentState = state;
}

public enum CharacterState
{
    Idle,
    Walking,
    Charging,
    Dashing,
    Dashed,
	Frozen,

    Dead,
}