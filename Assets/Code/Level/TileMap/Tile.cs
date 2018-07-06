using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Holds the *model data* of a tile, which defines this type of tile 
[System.Serializable]
public class TileModel
{
    [SerializeField] string _typeName; public string typeName => _typeName;

    [SerializeField] Data _data; public Data data => _data;
    [SerializeField] View _view; public View view => _view;


    [System.Serializable]
    public struct Data           // Hmpf, cannot be readonly due to Unity's serialization system...
    {
        public bool walkable;    // Can a player ever enter this tile?
        public int  health;      // How many times can a player step on this tile?
        public bool deadly;      // Will a player die if it steps on this tile?
        public bool unbreakable; // tile cant break 
    }

    [System.Serializable]
    public class View
    {
        [SerializeField] GameObject _mainGO; public GameObject mainGO => _mainGO; // Main visual representation
    }

	public void MakeEdgeTile()
	{
		_typeName = Constants.EDGE_TYPE;
		_data = new Data();
		_view = new View();

		_data.walkable = true;
		_data.health = 0;
		_data.deadly = true;
		_data.unbreakable = true;

	}
}

// Holds the instance data of a Tile, which separates different instances of the same type of tile
public class Tile
{
    public readonly TileModel model;

    public readonly Data data;
    readonly View _view;

    static TileDatabase _tileDB;


    static Tile()
    {
        _tileDB = TileDatabase.instance;
    }

    public Tile(Vector2DInt inPosition, string inTileName, Transform tilesFolder)
    {
        model = _tileDB.GetTile(inTileName);

        data  = new Data(model.data, inPosition);
        _view = new View(model.view, inPosition, tilesFolder);
    }


    public void Delete()
    {
        Object.Destroy(_view.mainGO, 1.0f); // hardcoded to same lenght as animation for now, dont like this too much
    }

    public class Data
    {
        public readonly Vector2DInt position;

        static TileMap _tileMap;
        TileMap tileMap => _tileMap ?? (_tileMap = Level.instance.tileMap);

        public int currentHealth { get; private set; } = 0;

        Character _character;

        public Data(TileModel.Data inDataModel, Vector2DInt inPosition)
        {
            position = inPosition;
            currentHealth = inDataModel.health;
        }

        public void SetCharacter(Character inCharacter) =>
            _character = inCharacter;

        public void RemovePlayer() =>
            _character = null;

		public bool IsOccupied() =>
			_character != null;

		public Character GetOccupyingPlayer() =>
			_character;

        public void DamageTile()
        {
            currentHealth--;

			tileMap.GetTile(position)._view.mainGO.GetComponent<Animator>().SetInteger("health", currentHealth); // cant get my tile model in a batter way right now, this should be fixed

            if (currentHealth == 0)
                tileMap.SetTile(position, new Tile(position, "empty", null));
        }

        public Tile GetRelativeTile(Vector2DInt inOffset) =>
            tileMap.GetTile(position + inOffset);
    }

    public class View
    {
        public readonly GameObject mainGO;

        public View(TileModel.View inViewModel, Vector2DInt inPosition, Transform tilesFolder)
        {
            if (inViewModel.mainGO == null)
                return;

            mainGO = Object.Instantiate(inViewModel.mainGO, tilesFolder);
            mainGO.transform.position = new Vector3(inPosition.x, 0, inPosition.y);
        }
    }
}



