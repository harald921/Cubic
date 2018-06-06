﻿using System.Collections;
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
    public struct Data           // Hmpf, cannot be readonly due to Unity's serialization system...
    {
        public bool walkable;    // Can a player ever enter this tile?
        public int  health;      // How many times can a player step on this tile?
        public bool deadly;      // Will a player die if it steps on this tile?
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

    static TileDatabase _tileDB;


    static Tile()
    {
        _tileDB = TileDatabase.instance;
    }

    public Tile(Vector2DInt inPosition, string inTileName)
    {
        model = _tileDB.GetTile(inTileName);

        data  = new Data(model.data, inPosition);
        _view = new View(model.view, inPosition);
    }


    public void Delete()
    {
        Object.Destroy(_view.mainGO);
    }


    public class Data
    {
        public readonly Vector2DInt position;

        static TileMap _tileMap;
        TileMap tileMap => _tileMap ?? (_tileMap = Level.instance.tileMap);

        int _currentHealth = 0;

        Character _character;


        public Data(TileModel.Data inDataModel, Vector2DInt inPosition)
        {
            position = inPosition;
            _currentHealth = inDataModel.health;
        }


        public void SetPlayer(Character inCharacter) =>
            _character = inCharacter;

        public void RemovePlayer() =>
            _character = null;

        public void DamageTile()
        {
            _currentHealth--;

            if (_currentHealth == 0)
                tileMap.SetTile(position, new Tile(position, "empty"));
        }


        public Tile GetRelativeTile(Vector2DInt inOffset) =>
            tileMap.GetTile(position + inOffset);
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


