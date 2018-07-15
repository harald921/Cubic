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
		[Header("BASIC SETTINGS"),Space(3)]
        public bool walkable;    // Can a player ever enter this tile?
        public int  health;      // How many times can a player step on this tile?
        public bool deadly;      // Will a player die if it steps on this tile?
        public bool unbreakable; // tile cant break 

		[Header("SOUNDS"), Space(3)]
		public AudioClip landSound;
		public AudioClip breakSound;

		[Header("PARTICLES"), Space(3)]
		public GameObject landParticle;
		public GameObject breakParticle;
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

    TileDatabase _tileDB;

	AudioSource[] _sounds;


    public Tile(Vector2DInt position, string tileName, float yRotation, float tintStrength, Transform tilesFolder)
    {
		_tileDB = TileDatabase.instance;

		model = _tileDB.GetTile(tileName);

        data  = new Data(model.data, position, this);
        view = new View(model.view, position, yRotation, tintStrength, tilesFolder);

		_sounds = new AudioSource[(int)TileSounds.Count];
		CreateSounds();
    }

	public void Delete()
    {
        Object.Destroy(view.mainGO, 1.0f); // hardcoded to same lenght as animation for now, dont like this too much
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

	void PlaySound(TileSounds type)
	{
		if (view.mainGO == null)
			return;

		if (_sounds[(int)type] != null)
			_sounds[(int)type].Play();
	}

	public void OnPlayerLand()
	{
		PlaySound(TileSounds.Land);

		if (model.data.landParticle != null)
		{
			GameObject p = Object.Instantiate(model.data.landParticle, new Vector3(data.position.x, 0, data.position.y), model.data.landParticle.transform.rotation);
			Object.Destroy(p, 8);
		}
	}

	public static void TintTile(GameObject tile, float strength)
	{
		Renderer renderer = tile.GetComponent<Renderer>();
		if (renderer != null)
			renderer.material.color = Color.white * strength;

		for (int i = 0; i < tile.transform.childCount; i++)
		{
			renderer = tile.transform.GetChild(i).GetComponent<Renderer>();
			if (renderer != null)
				renderer.material.color = Color.white * strength;
		}
	}

	public class Data
    {
        public readonly Vector2DInt position;

        public int currentHealth { get; private set; } = 0;

        Character _character;

		Tile _myTile;

        public Data(TileModel.Data dataModel, Vector2DInt position, Tile myTile)
        {
            this.position = position;
            currentHealth = dataModel.health;
			_myTile = myTile;
        }		

        public void SetCharacter(Character character) =>
            _character = character;

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

			if (_myTile.model.data.breakParticle != null)
			{
				GameObject p = Object.Instantiate(_myTile.model.data.breakParticle, new Vector3(position.x, 0, position.y), _myTile.model.data.breakParticle.transform.rotation);
				Object.Destroy(p, 8);
			}

            if (currentHealth == 0)
				Match.instance.level.tileMap.SetTile(position, new Tile(position, "empty", 0.0f, 0.0f, null));
        }

        public Tile GetRelativeTile(Vector2DInt offset) =>
			Match.instance.level.tileMap.GetTile(position + offset);
    }

    public class View
    {
        public readonly GameObject mainGO;

        public View(TileModel.View viewModel, Vector2DInt position, float yRotation, float tintStrength, Transform tilesFolder)
        {
            if (viewModel.mainGO == null)
                return;

            mainGO = Object.Instantiate(viewModel.mainGO, tilesFolder);
			mainGO.transform.rotation = mainGO.transform.rotation * Quaternion.Euler(new Vector3(0, yRotation, 0));
            mainGO.transform.position = new Vector3(position.x, 0, position.y);

			TintTile(mainGO, tintStrength);
        }
    }

	
}



