using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if DEBUG_TOOLS
public class PlayerPage : MonoBehaviour
{
    Character _targetCharacter;

    public void Initialize(Character inCharacter) =>
        _targetCharacter = inCharacter;

	void OnGUI()
	{
        if (_targetCharacter == null)
            return;

        GUILayout.Window(99, new Rect(0, 0, 300, 200), DrawStats, "Debug");
	}

	void DrawStats(int id)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("PLAYER STATE : {0}", _targetCharacter.currentState));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("DASH CHARGED TO : {0}", _targetCharacter.currentDashCharges));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("CURRENT POSITION : X{0} Y{1}", _targetCharacter.currentTile.data.position.x, _targetCharacter.currentTile.data.position.y));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("CURRENT TILETYPE : {0}", _targetCharacter.currentTile.model.typeName.ToUpper()));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("TILE HEALTH : {0}", _targetCharacter.currentTile.data.currentHealth));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("TILE UNBREAKABLE ? : {0}", _targetCharacter.currentTile.model.data.unbreakable.ToString().ToUpper()));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("TILE DEADLY ? : {0}", _targetCharacter.currentTile.model.data.deadly.ToString().ToUpper()));
		GUILayout.EndHorizontal();
	}
}

#endif