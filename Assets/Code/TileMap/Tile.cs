using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public readonly string typeName = "debug";

    public readonly Data data;
    public readonly View view;


    public Tile(Vector2DInt inPosition, string inTypeName)
    {
        typeName = inTypeName;

        data = new Data(inTypeName);
        view = new View(inPosition, inTypeName);
    }

    public Tile(Vector2DInt inPosition)
    {
        // DEBUG
        if (inPosition.x == 0 || inPosition.x == 11 ||
            inPosition.y == 0 || inPosition.x == 11)
            data = new Data(true, true);

        else
            data = new Data(true, false);

        view = new View(inPosition);
    }


    public class Data
    {
        public Player player { get; private set; }

        public readonly bool walkable;
        public readonly bool deadly;


        public Data(string inTileName)
        {
            // Load tile data from file...
        }

        public Data(bool inWalkable, bool inDeadly)
        {
            walkable = inWalkable;
            deadly = inDeadly;
        }
    }

    // Holds all the gameobjects a tile will have. Its own class in case a tile will have several gameobject, eg. separate particle emitters and such
    public class View
    {
        public readonly GameObject mainGO;

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