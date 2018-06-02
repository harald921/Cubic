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

	public string GetTileType(int id) =>
		tileTypes[id];

	public int GetTileTypeIndex(string name)
	{
		for (int i = 0; i < tileTypes.Count; i++)
			if (name == tileTypes[i])
				return i;

		Debug.LogErrorFormat("could not find tile {0} in tiletypes", name);
		return -1;
	}

    [System.Serializable]
    public struct NameTileKVP
    {
        public string tileName;
        public Tile tile;
    }
}
