using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDatabase : MonoBehaviour
{
    public static TileDatabase instance { get; private set; }

    public List<NameTileKVP> _tilesToSerialize = new List<NameTileKVP>();

    Dictionary<string, Tile> tiles = new Dictionary<string, Tile>();
    List<string> tileTypes = new List<string>();


    public void Awake()
    {
        instance = this;

        foreach (var kvp in _tilesToSerialize)
        {
            tileTypes.Add(kvp.tileName);
            tiles.Add(kvp.tileName, kvp.tile);
        }
    }


    public int GetTileCount => 
        tiles.Count;

    public Tile GetTile(string inTileName) =>
        tiles[inTileName];

    public Tile GetTile(int inID) =>
        tiles[tileTypes[inID]];



    [System.Serializable]
    public struct NameTileKVP
    {
        public string tileName;
        public Tile tile;
    }
}
