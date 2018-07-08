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

		public AudioClip landSound;
		public AudioClip breakSound;
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
	public enum TileSounds
	{
		Land,
		Break,

		Count,
	}

    public readonly TileModel model;

    public readonly Data data;
    readonly View view;

    static TileDatabase _tileDB;

	AudioSource[] _sounds;

    static Tile()
    {
        _tileDB = TileDatabase.instance;
    }

    public Tile(Vector2DInt inPosition, string inTileName, float inYRotation, Transform tilesFolder)
    {
        model = _tileDB.GetTile(inTileName);

        data  = new Data(model.data, inPosition, this);
        view = new View(model.view, inPosition, inYRotation, tilesFolder);

		_sounds = new AudioSource[(int)TileSounds.Count];
		CreateSounds();
    }

    public void Delete()
    {
        Object.Destroy(view.mainGO, 1.0f); // hardcoded to same lenght as animation for now, dont like this too much
    }

    public class Data
    {
        public readonly Vector2DInt position;

        static TileMap _tileMap;
        TileMap tileMap => _tileMap ?? (_tileMap = Level.instance.tileMap);

        public int currentHealth { get; private set; } = 0;

        Character _character;

		Tile _myTile;

        public Data(TileModel.Data inDataModel, Vector2DInt inPosition, Tile myTile)
        {
            position = inPosition;
            currentHealth = inDataModel.health;
			_myTile = myTile;
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

			_myTile.view.mainGO.GetComponent<Animator>().SetInteger("health", currentHealth);

			_myTile.PlaySound(TileSounds.Break);

            if (currentHealth == 0)
                tileMap.SetTile(position, new Tile(position, "empty", 0.0f, null));
        }

        public Tile GetRelativeTile(Vector2DInt inOffset) =>
            tileMap.GetTile(position + inOffset);
    }

    public class View
    {
        public readonly GameObject mainGO;

        public View(TileModel.View inViewModel, Vector2DInt inPosition, float inYrotation, Transform tilesFolder)
        {
            if (inViewModel.mainGO == null)
                return;

            mainGO = Object.Instantiate(inViewModel.mainGO, tilesFolder);
			mainGO.transform.rotation = mainGO.transform.rotation * Quaternion.Euler(new Vector3(0, inYrotation, 0));
            mainGO.transform.position = new Vector3(inPosition.x, 0, inPosition.y);
        }
    }

	void CreateSounds()
	{
		if (view.mainGO == null)
			return;

		GameObject soundHolderLand = new GameObject("landSound", typeof(AudioSource));
		soundHolderLand.transform.SetParent(view.mainGO.transform);

		GameObject soundHolderBreak = new GameObject("breakSound", typeof(AudioSource));
		soundHolderBreak.transform.SetParent(view.mainGO.transform);

		_sounds[(int)TileSounds.Land] = soundHolderLand.GetComponent<AudioSource>();
		_sounds[(int)TileSounds.Land].clip = model.data.landSound;

		_sounds[(int)TileSounds.Break] = soundHolderBreak.GetComponent<AudioSource>();
		_sounds[(int)TileSounds.Break].clip = model.data.breakSound;
	}

	public void PlaySound(TileSounds type)
	{
		if (view.mainGO == null)
			return;

		if(_sounds[(int)type] != null)
		   _sounds[(int)type].Play();
	}
}



