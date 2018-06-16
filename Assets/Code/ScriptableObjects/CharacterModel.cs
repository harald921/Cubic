using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Data Models/Player Model", order =1)]
public class CharacterModel : ScriptableObject
{
	public float walkSpeed                 = 2.0f;      // How many tiles per second the player moves
	public float walkCooldown              = 0.25f;     // cooldown between moves
	public int   dashMaxDistance           = 4;         // Max dash distance in tiles
	public float dashChargeRate            = 2.0f;      // Dash tiles per second
	public float dashCooldownTime          = 0.5f;      // how long before a new dash can be made
	public float dashSpeed                 = 6.0f;      // how many tiles per second the player dashes
	public int   numBarrelRollsPerDashTile = 1;         // how many spins the player will do while dashing

}
