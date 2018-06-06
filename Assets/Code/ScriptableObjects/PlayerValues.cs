using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Value Files/Player Values", order =1)]
public class PlayerValues : ScriptableObject
{
	public float moveSpeed = 2.0f;
	public AnimationCurve moveCurve;
	public int numDashTiles = 4;
	public float dashChargeSpeed = 2.0f;
	
}
