using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDatabase : MonoBehaviour
{
    public static TileDatabase instance { get; private set; }

    [SerializeField] List<Tile> tiles = new List<Tile>();
    public Tile GetTile(int inID) => tiles[inID];
	public int GetNumTiles => tiles.Count;

	public int GetIndexFromName(string name)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			if (tiles[i].typeName == name)
				return i;
		}

		return -1; // magic number for not found
	}

	public Tile GetTileFromName(string name)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			if (tiles[i].typeName == name)
				return tiles[i];
		}

		return null; 
	}


    public void Awake()
    {
        instance = this;

    }
}
