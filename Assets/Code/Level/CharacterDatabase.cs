using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDatabase : MonoBehaviour
{
    static CharacterDatabase _instance;
    public static CharacterDatabase instance => _instance ?? (_instance = FindObjectOfType<CharacterDatabase>()); 

    [SerializeField] CharacterModel _standardModel; public CharacterModel standardModel => _standardModel;
}
