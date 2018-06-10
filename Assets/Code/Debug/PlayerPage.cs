using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if DEBUG_TOOLS
public class PlayerPage : MonoBehaviour
{
	static PlayerPage _instance; public static PlayerPage instance { get { return _instance; } }

	Character _player; public Character player { set { _player = value; } }

	void Awake()
	{
		_instance = this;	
	}

	void OnGUI()
	{		
		GUILayout.Window(99, new Rect(0, 0, 300, 200), DrawStats, "Debug");
	}

	void DrawStats(int id)
	{
		if (_player == null)
			return;

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("PLAYER STATE : {0}", _player.currentState));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("DASH CHARGED TO : {0}", _player.currentDashCharges));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("CURRENT POSITION : X{0} Y{1}", _player.currentTile.data.position.x, _player.currentTile.data.position.y));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("CURRENT TILETYPE : {0}", _player.currentTile.model.typeName.ToUpper()));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("TILE HEALTH : {0}", _player.currentTile.data.currentHealth));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("TILE UNBREAKABLE ? : {0}", _player.currentTile.model.data.unBreakable.ToString().ToUpper()));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("TILE DEADLY ? : {0}", _player.currentTile.model.data.deadly.ToString().ToUpper()));
		GUILayout.EndHorizontal();
	}
}

#endif