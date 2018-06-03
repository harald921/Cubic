using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDatabase : MonoBehaviour
{
    public static TileDatabase instance { get; private set; }

    public List<NameTileKVP> tilesToSerialize = new List<NameTileKVP>();        // Collection gathering data from the inspector to insert into "_tiles" and "_tileTypes" upon program start

    Dictionary<string, TileModel> _tiles = new Dictionary<string, TileModel>();
    List<string> _tileTypes = new List<string>();


    public void Awake()
    {
        instance = this;

        foreach (var kvp in tilesToSerialize)
        {
            _tileTypes.Add(kvp.tileName);
            _tiles.Add(kvp.tileName, kvp.tile);
        }
    }


    public int tileCount => 
        _tiles.Count;

    public TileModel GetTile(string inTileName) =>
        _tiles[inTileName];

    public TileModel GetTile(int inID) =>
        _tiles[_tileTypes[inID]];

    public int GetTileTypeIndex(string inName) =>
        _tileTypes.IndexOf(inName);


    [System.Serializable]
    public struct NameTileKVP
    {
        public string tileName;
        public TileModel tile;
    }
}
