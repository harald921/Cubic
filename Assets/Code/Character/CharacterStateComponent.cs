using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateComponent : MonoBehaviour
{
    NewCharacter _character;

    public CharacterState currentState { get; private set; } = CharacterState.Idle;

    public void ManualAwake()
    {
        _character = GetComponent<NewCharacter>();
    }


    public void SetState(CharacterState inState) => currentState = inState;
}

public enum CharacterState
{
    Idle,
    Walking,
    Charging,
    Dashing,
    Dashed,

    Dead,
}