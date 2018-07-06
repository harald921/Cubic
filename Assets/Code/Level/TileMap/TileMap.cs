﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TileMap
{
    readonly string name = "EditorTest";

    Dictionary<Vector2DInt, Tile> _tiles = new Dictionary<Vector2DInt, Tile>();

	Vector2DInt _gridSize;
	Transform _tilesFolder;

    public TileMap(string inMapName, Transform inTilesFolder)
    {
        name = inMapName;
		_tilesFolder = inTilesFolder;
        BinaryLoad();
    }


    public Tile GetTile(Vector2DInt inPosition) => _tiles[inPosition];
    public void SetTile(Vector2DInt inPosition, Tile inTile)
    {
        _tiles[inPosition].Delete();
        _tiles[inPosition] = inTile;
    }

	public Vector2DInt GetRandomTileCoords() =>	
		new Vector2DInt(Random.Range(0, _gridSize.x), Random.Range(0, _gridSize.y));
	
    #region Serialization
    public void BinarySave()
    {
        Directory.CreateDirectory(Constants.TILEMAP_SAVE_FOLDER); 

        using (FileStream stream = new FileStream(Path.Combine(Constants.TILEMAP_SAVE_FOLDER, name), FileMode.OpenOrCreate, FileAccess.Write))
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write(_tiles.Count);                            // Write: Num tiles
            foreach (var tilesKVP in _tiles)
            {
                tilesKVP.Key.BinarySave(writer);                  // Write: Position
                writer.Write(tilesKVP.Value.model.typeName);  // Write: Tile type name
            }
        }
    }

    public void BinaryLoad()
    {
        using (FileStream stream = new FileStream(Path.Combine(Constants.TILEMAP_SAVE_FOLDER, name), FileMode.Open, FileAccess.Read))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            int gridSizeY = reader.ReadInt32();        // Read: Num tiles Vertical
			int gridSizeX = reader.ReadInt32();        // Read: Num tiles Horizontal

			int tileCount = gridSizeY * gridSizeX;        // Num tiles in total

			_gridSize = new Vector2DInt(gridSizeX, gridSizeY); // save gridsize if we need it later

			for (int i = 0; i < tileCount; i++)
            {
                Vector2DInt tilePosition = Vector2DInt.Zero;
                tilePosition.BinaryLoad(reader);       // Read: Position

                string typeName = reader.ReadString(); // Read: Tile type name  

                _tiles.Add(tilePosition, new Tile(tilePosition, typeName, _tilesFolder));
            }

			AddEdgeTiles(gridSizeX, gridSizeY);
        }
    }
    #endregion

	public void AddEdgeTiles(int sizeX, int sizeY)
	{
		// left edges
		for (int i = 0; i < sizeY; i++)
			_tiles.Add(new Vector2DInt(-1, i), new Tile(new Vector2DInt(-1, i), Constants.EDGE_TYPE, null));

		// right edges
		for (int i = 0; i < sizeY; i++)
			_tiles.Add(new Vector2DInt(sizeX, i), new Tile(new Vector2DInt(sizeX, i), Constants.EDGE_TYPE, null));

		// top edges
		for (int i = 0; i < sizeX; i++)
			_tiles.Add(new Vector2DInt(i, sizeY), new Tile(new Vector2DInt(i, sizeY), Constants.EDGE_TYPE, null));

		// bottom edges
		for (int i = 0; i < sizeX; i++)
			_tiles.Add(new Vector2DInt(i, -1), new Tile(new Vector2DInt(i, -1), Constants.EDGE_TYPE, null));
	}

	public void ClearTileViews()
	{
		for (int i = 0; i < _tilesFolder.childCount; i++)
			Object.Destroy(_tilesFolder.GetChild(i).gameObject);
	}

	public void ResetMap()
	{
		_tiles.Clear();
		BinaryLoad();
	}
}



   


