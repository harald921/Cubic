using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static Level instance { get; private set; }

    public TileMap tileMap { get; private set; }

    void Awake()
    {
        instance = this;

        tileMap = new TileMap("SavedFromInputField");
    }
}
