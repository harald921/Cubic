using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TileMap
{
    readonly string name = "debug";
    Dictionary<Vector2DInt, Tile> tiles = new Dictionary<Vector2DInt, Tile>();

    public Tile GetTile(Vector2DInt inPosition) => tiles[inPosition];
    public void SetTile(Vector2DInt inPosition, Tile inTile)
    {
        // tiles[inPosition].view.Destroy();
        tiles[inPosition] = inTile;
    } 


    public TileMap(string inMapName)
    {
        name = inMapName;
        BinaryLoad();
    }

    public TileMap(int inSize)
    {
        for (int y = 0; y < inSize; y++)
            for (int x = 0; x < inSize; x++)
            {
                Vector2DInt tilePosition = new Vector2DInt(x, y);
                tiles.Add(tilePosition, new Tile(tilePosition));
            }

        BinarySave();
    }


    public void BinarySave()
    {
        Directory.CreateDirectory(Constants.TILEMAP_SAVE_FOLDER); 

        using (FileStream stream = new FileStream(Path.Combine(Constants.TILEMAP_SAVE_FOLDER, name), FileMode.OpenOrCreate, FileAccess.Write))
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write(tiles.Count);             // Write: Num tiles
            foreach (KeyValuePair<Vector2DInt, Tile> item in tiles)
            {
                item.Key.BinarySave(writer);       // Write: Position
                writer.Write(item.Value.typeName); // Write: Tile type name
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

                tiles.Add(tilePosition, new Tile(tilePosition/*, typeName*/)); // TODO: Add serialization / Deserialization for tiles
            }
        }
    }
}

   


