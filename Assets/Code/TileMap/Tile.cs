using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile
{
    [SerializeField] string _typeName = "debug";
    public string typeName => _typeName;

    public Data data;
    public View view;


    public Tile(Vector2DInt inPosition, string inTypeName)
    {
        _typeName = inTypeName;

        data = new Data(inTypeName);
        view = new View(inPosition, inTypeName);
    }

    [System.Serializable]
    public class Data
    {
        public Player player { get; private set; }

        [SerializeField] TileSettings _tileSettings;
        public TileSettings tileSettings => _tileSettings;

        public Data(string inTileName)
        {
            
        }
    }


    // Holds all the gameobjects a tile will have. Its own class in case a tile will have several gameobject, eg. separate particle emitters and such
    [System.Serializable]
    public class View
    {
        [SerializeField] private GameObject mainGO;

        public View(Vector2DInt inPosition, string inTileName)
        {
            // Load tile view from file... (E.g, "lava" loads the "lava" prefab from resources)
        }

        public View(Vector2DInt inPosition)
        {
            // Debug way of creating a border around the map
            if (inPosition.x == 0 || inPosition.x == 11 ||
                inPosition.y == 0 || inPosition.x == 11)
                return;

            mainGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainGO.transform.localScale = Vector3.one * 0.95f;
            mainGO.transform.position = new Vector3(inPosition.x, 0, inPosition.y) + new Vector3(0.5f, -0.5f, 0.5f);

            mainGO.transform.SetParent(Level.instance.transform);
        }
    }
}

[System.Serializable]
public struct TileSettings
{
    public bool walkable;
    public int  walksBeforeBreak;
    public bool deadly;
}