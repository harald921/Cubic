﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TileMap
{
    readonly string name = "EditorTest";

    Dictionary<Vector2DInt, Tile> _tiles = new Dictionary<Vector2DInt, Tile>();


    public TileMap(string inMapName)
    {
        name = inMapName;
        BinaryLoad();
    }


    public Tile GetTile(Vector2DInt inPosition) => _tiles[inPosition];
    public void SetTile(Vector2DInt inPosition, Tile inTile)
    {
        _tiles[inPosition].Delete();
        _tiles[inPosition] = inTile;
    }


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
            int tileCount = reader.ReadInt32();        // Read: Num tiles
            for (int i = 0; i < tileCount; i++)
            {
                Vector2DInt tilePosition = Vector2DInt.Zero;
                tilePosition.BinaryLoad(reader);       // Read: Position

                string typeName = reader.ReadString(); // Read: Tile type name  

                _tiles.Add(tilePosition, new Tile(tilePosition, typeName, this));
            }
        }
    }
    #endregion
}



   


