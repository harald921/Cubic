using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TileMap
{
    readonly string name = "EditorTest";
    Dictionary<Vector2DInt, Tile> tiles = new Dictionary<Vector2DInt, Tile>();

    public Tile GetTile(Vector2DInt inPosition) => tiles[inPosition];
    public void SetTile(Vector2DInt inPosition, Tile inTile)
    {
        tiles[inPosition].Delete();
        tiles[inPosition] = inTile;
    } 


    public TileMap(string inMapName)
    {
        name = inMapName;
        BinaryLoad();
    }


    #region Serialization
    public void BinarySave()
    {
        Directory.CreateDirectory(Constants.TILEMAP_SAVE_FOLDER); 

        using (FileStream stream = new FileStream(Path.Combine(Constants.TILEMAP_SAVE_FOLDER, name), FileMode.OpenOrCreate, FileAccess.Write))
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write(tiles.Count);                       // Write: Num tiles
            foreach (var kvp in tiles)
            {
                kvp.Key.BinarySave(writer);                  // Write: Position
                writer.Write(kvp.Value.tileModel.typeName);  // Write: Tile type name
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

                tiles.Add(tilePosition, new Tile(tilePosition, typeName));
            }
        }
    }
    #endregion
}



   


