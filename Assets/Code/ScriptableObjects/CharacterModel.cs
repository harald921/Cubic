using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Data Models/Player Model", order =1)]
public class CharacterModel : ScriptableObject
{
	public float moveSpeed       = 2.0f; // How many tiles per second the player moves
	public int   maxDashDistance = 4;    // Max dash distance in tiles
	public float dashChargeRate  = 2.0f; // Dash tiles per second
}
