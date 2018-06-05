using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Holds the *model data* of a tile, which defines this type of tile 
[System.Serializable]
public class TileModel
{
    [SerializeField] string _typeName; public string typeName => _typeName;

    [SerializeField] Data _data; public Data data => _data;
    [SerializeField] View _view; public View view => _view;


    [System.Serializable]
    public struct Data        // Hmpf, cannot be readonly due to Unity's serialization system...
    {
        public bool walkable; // Can a player ever enter this tile?
        public int  health;   // How many times can a player step on this tile?
        public bool deadly;   // Will a player die if it steps on this tile?
		public bool unBreakable; // tile cant break 
    }

    [System.Serializable]
    public class View
    {
        [SerializeField] GameObject _mainGO; public GameObject mainGO => _mainGO; // Main visual representation
    }
}


// Holds the instance data of a Tile, which separates different instances of the same type of tile
public class Tile
{
    public readonly TileModel model;

    public readonly Data data;
    readonly View _view;

    readonly TileMap _tileMap;

    static TileDatabase _tileDB;


    static Tile()
    {
        _tileDB = TileDatabase.instance;
    }

    public Tile(Vector2DInt inPosition, string inTileName, TileMap inTileMap)
    {
        model = _tileDB.GetTile(inTileName);

        data = new Data(model.data, inPosition);
        _view = new View(model.view, inPosition);

        _tileMap = inTileMap;
    }


    public void Delete()
    {
        Object.Destroy(_view.mainGO);
    }




    public Tile GetRelativeTile(Vector2DInt inOffset) =>
        _tileMap.GetTile(data.position + inOffset);
        

    public class Data
    {
        public readonly Vector2DInt position;

        Player _player;
        int _currentHealth = 0;


        public void SetPlayer(Player inPlayer) =>
            _player = inPlayer;

        public void RemovePlayer() =>
            _player = null;


        public Data(TileModel.Data inDataModel, Vector2DInt inPosition)
        {
            position = inPosition;

            _currentHealth = inDataModel.health;
        }
    }

    public class View
    {
        GameObject _mainGO;
        public GameObject mainGO => _mainGO; 


        public View(TileModel.View inViewModel, Vector2DInt inPosition)
        {
			if (inViewModel.mainGO == null)
				return;

            _mainGO = Object.Instantiate(inViewModel.mainGO);
            _mainGO.transform.position = new Vector3(inPosition.x, 0, inPosition.y);
        }
    }
}


