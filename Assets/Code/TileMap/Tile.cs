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

<<<<<<< HEAD
        [SerializeField] TileSettings _tileSettings;
        public TileSettings tileSettings => _tileSettings;

        public Data(string inTileName)
        {
            
        }
    }
=======
        [SerializeField] TileSettings _tileSettings; public TileSettings tileSettings { get { return _tileSettings; } }
        

        public Data(string inTileName)
        {
			Tile tileData = TileDatabase.instance.GetTileFromName(inTileName);
>>>>>>> 37f28b50d48add1e4412983f19526b4832a43911

			_tileSettings.walkable         = tileData.data.tileSettings.walkable;
			_tileSettings.walksBeforeBreak = tileData.data.tileSettings.walksBeforeBreak;
			_tileSettings.deadly           = tileData.data.tileSettings.deadly;
		}
    }

	// Holds all the gameobjects a tile will have. Its own class in case a tile will have several gameobject, eg. separate particle emitters and such
	[System.Serializable]
	public class View
	{
		[SerializeField] private GameObject mainGO; public GameObject MainGo { get { return mainGO; } }

        public View(Vector2DInt inPosition, string inTileName)
        {
			if(inTileName == "Death")
			{
				mainGO = null;
				return;
			}

			mainGO = GameObject.Instantiate(TileDatabase.instance.GetTileFromName(inTileName).view.MainGo);
			mainGO.transform.position = new Vector3(inPosition.x, 0, inPosition.y);
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