using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if DEBUG_TOOLS
public class PlayerPage : MonoBehaviour
{
    NewCharacter _targetCharacter;

    public void Initialize(NewCharacter inCharacter) =>
        _targetCharacter = inCharacter;

	void OnGUI()
	{
        if (_targetCharacter == null)
            return;

        GUILayout.Window(99, new Rect(0, 0, 300, 200), DrawStats, "Debug");
	}

	void DrawStats(int id)
	{
        Tile currentTile = _targetCharacter.movementComponent.currentTile;

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("PLAYER STATE : {0}", _targetCharacter.stateComponent.currentState));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("DASH CHARGED TO : {0}", _targetCharacter.movementComponent.currentDashCharges));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("CURRENT POSITION : X{0} Y{1}", currentTile.data.position.x, currentTile.data.position.y));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("CURRENT TILETYPE : {0}", _targetCharacter.movementComponent.currentTile.model.typeName.ToUpper()));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("TILE HEALTH : {0}", currentTile.data.currentHealth));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("TILE UNBREAKABLE ? : {0}", currentTile.model.data.unbreakable.ToString().ToUpper()));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("TILE DEADLY ? : {0}", currentTile.model.data.deadly.ToString().ToUpper()));
		GUILayout.EndHorizontal();
	}
}

#endif