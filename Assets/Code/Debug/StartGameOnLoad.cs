using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameOnLoad : MonoBehaviour
{
	void Start()
	{
		Level.instance.StartOnAllLoaded(); // will call an rpc that all should start the game when all players are loaded into this scene
	}

}
