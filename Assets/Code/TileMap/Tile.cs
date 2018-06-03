using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Holds the *model data* of a tile, which defines this type of tile 
[System.Serializable]
public class TileModel
{
    string _typeName; public string typeName => _typeName;

    [SerializeField] Data _data; public Data data => _data;
    [SerializeField] View _view; public View view => _view;


    [System.Serializable]
    public struct Data        // Hmpf, cannot be readonly due to Unity's serialization system...
    {
        public bool walkable; // Can a player ever enter this tile?
        public int  health;   // How many times can a player step on this tile?
        public bool deadly;   // Will a player die if it steps on this tile?
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
    public readonly TileModel tileModel;

    readonly Data _data;
    readonly View _view;


    public Tile(Vector2DInt inPosition, string inTileName)
    {
        tileModel = TileDatabase.instance.GetTile(inTileName);

        _data = new Data(tileModel.data, inPosition);
        _view = new View(tileModel.view, inPosition);
    }

    public void Delete()
    {
        Object.Destroy(_view.mainGO);
    }


    public class Data
    {
        public readonly Vector2DInt position;

        public Player _player { get; private set; }
        int _currentHealth = 0;


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
            if (inViewModel.mainGO)
                _mainGO = Object.Instantiate(inViewModel.mainGO);

            _mainGO.transform.position = new Vector3(inPosition.x, 0, inPosition.y);
        }
    }
}


