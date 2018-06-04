using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetDropdownObjectInactive : MonoBehaviour
{

	void Start()
	{
		if(GetComponentInChildren<Text>().text == "empty")
		{
			GetComponent<Toggle>().interactable = false;
		}
	}

}
