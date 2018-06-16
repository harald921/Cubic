using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class CharacterFlagComponent : MonoBehaviour
{
    NewCharacter _character;

    Dictionary<CharacterFlag, bool> _flags = new Dictionary<CharacterFlag, bool>(); // Can be replaced with bit enum when I am not lazy
    Dictionary<CharacterFlag, CoroutineHandle> _durationHandles = new Dictionary<CharacterFlag, CoroutineHandle>();


    public void ManualAwake()
    {
        _character = GetComponent<NewCharacter>();

        foreach (CharacterFlag stateValue in System.Enum.GetValues(typeof(CharacterFlag)))
        {
            _flags.Add(stateValue, false);
            _durationHandles.Add(stateValue, new CoroutineHandle());
        }
    }
    
    public bool GetFlag(CharacterFlag inFlag) => _flags[inFlag];
    public void SetFlag(CharacterFlag inFlag, bool inValue)
    {
        // Sets the flag directly

        Timing.KillCoroutines(_durationHandles[inFlag]);

        _flags[inFlag] = inValue;
    }

    public void SetFlag(CharacterFlag inFlag, bool inValue, float inDuration, SingletonBehavior inCollisionBehaviour)
    {
        // Sets the flag and inverts it after 'inDuration'

        if (inDuration <= 0)
            throw new System.Exception("SetFlag parameter 'inDuration' cannot be zero or lower.");

        _durationHandles[inFlag] = Timing.RunCoroutineSingleton(_FlagDuration(inFlag, inValue, inDuration), _durationHandles[inFlag], inCollisionBehaviour);
    }


    IEnumerator<float> _FlagDuration(CharacterFlag inFlag, bool inInitialState, float inDuration)
    {
        _flags[inFlag] = inInitialState;
        yield return Timing.WaitForSeconds(inDuration);
        _flags[inFlag] = !inInitialState;
    }
}

public enum CharacterFlag
{
    Cooldown_Dash,
    Cooldown_Walk,
}
