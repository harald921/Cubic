﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDatabase : MonoBehaviour
{
    public static TileDatabase instance { get; private set; }

    public List<TileModel> tilesToSerialize = new List<TileModel>();        // Collection gathering data from the inspector to insert into "_tiles" and "_tileTypes" upon program start

    Dictionary<string, TileModel> _tiles = new Dictionary<string, TileModel>();
    List<string> _tileTypes = new List<string>();


    public void Awake()
    {
        instance = this;

        foreach (TileModel tileModelToSerialize in tilesToSerialize)
        {
            _tileTypes.Add(tileModelToSerialize.typeName.ToLower());
            _tiles.Add(tileModelToSerialize.typeName.ToLower(), tileModelToSerialize);
        }
    }


    public int tileCount => 
        _tiles.Count;

    public TileModel GetTile(string inTileName) =>
        _tiles[inTileName.ToLower()];


    public TileModel GetTile(int inID) =>
        _tiles[_tileTypes[inID]];

    public int GetTileTypeIndex(string inName) =>
        _tileTypes.IndexOf(inName);
}