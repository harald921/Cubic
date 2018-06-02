using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDatabase : MonoBehaviour
{
    public static TileDatabase instance { get; private set; }

    [SerializeField] List<Tile> tiles = new List<Tile>();
    public Tile GetTile(int inID) => tiles[inID];


    public void Awake()
    {
        instance = this;
    }
}
