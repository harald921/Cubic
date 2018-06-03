using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDatabase : MonoBehaviour
{
    public static TileDatabase instance { get; private set; }

    public List<NameTileKVP> _tilesToSerialize = new List<NameTileKVP>();

    Dictionary<string, TileModel> tiles = new Dictionary<string, TileModel>();
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


    public int tileCount => 
        tiles.Count;

    public TileModel GetTile(string inTileName) =>
        tiles[inTileName];

    public TileModel GetTile(int inID) =>
        tiles[tileTypes[inID]];


	public int GetTileTypeIndex(string inName)
	{
		for (int i = 0; i < tileTypes.Count; i++)
			if (inName == tileTypes[i])
				return i;
        
        throw new KeyNotFoundException("Tile type '" + inName + "' not found");
	}


    [System.Serializable]
    public struct NameTileKVP
    {
        public string tileName;
        public TileModel tile;
    }
}
