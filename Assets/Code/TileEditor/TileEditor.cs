using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;



public class TileInfo
{
	public Vector2DInt position;
	public string name = "";

	public TileInfo(Vector2DInt inPosition, string inName)
	{
		position = inPosition;
		name = inName;
	}
}

public class TileEditor : MonoBehaviour
{	
	enum EDIT_MODE
	{
		PLACE_SINGLE,
		PAINT,
		DELETE,

		NUM_MODES
	}

	EDIT_MODE _currentEditMode;
	Camera _camera;

	// grid
	[Header("Grid")]
	[SerializeField] Material _gridmaterial;
	[SerializeField] Transform _tileFolder;
	Vector2DInt _gridDefaultSize = new Vector2DInt(10, 10);
	Vector2DInt _gridSize;
	GameObject _grid;

	// tileinfo
	TileInfo[,] _tileProperties;

	// current
	GameObject _selectedTile;
	string _selectedTileType;

	TileDatabase _tileDB;

	//UI
	[Header("UI")]
	[SerializeField] InputField _inputLoad;
	[SerializeField] InputField _inputSave;
	[SerializeField] Dropdown   _dropDownTiles;
	[SerializeField] Text		_gridSizeText;
	[SerializeField] Slider		_sliderSizeX;
	[SerializeField] Slider		_sliderSizeY;
	[SerializeField] Text       _editModeText;
	[SerializeField] MessagePromt _promt;
	[SerializeField] Sprite _emptySprite;

	void Start()
	{
		// get references
		_tileDB = TileDatabase.instance;
		_camera = Camera.main;

		// freeze camera
		_camera.GetComponent<SimpleCameraController>().Freeze();

		// generate standard 10x10 grid
		GenerateGrid(_gridDefaultSize.x, _gridDefaultSize.y);

		// add all existing tiletypes to dropdown menu
		_dropDownTiles.options.Clear();
		for(int i =0; i < _tileDB.tileCount; i++)
		{							
			_dropDownTiles.options.Add(new Dropdown.OptionData(_tileDB.GetTile(i).typeName));
			if (_tileDB.GetTile(i).typeName == "empty")			
				_dropDownTiles.options[i].image = _emptySprite;							
		}		
		
		// create tile of first type in typearray
		_selectedTile = Instantiate(_tileDB.GetTile(0).view.mainGO, _tileFolder);
		_selectedTileType = _tileDB.GetTile(0).typeName;
		_currentEditMode = EDIT_MODE.PLACE_SINGLE;

		// add collider and set layer of tile (need this to be able to select alredy placed tiles)
		AddcolliderToSelectedTile();
	}

	void Update()
	{
		// check if scrollwheel has moved
		float scrollDelta = Input.GetAxisRaw("Mouse ScrollWheel");
		if(scrollDelta != 0)
		{
			int currentType = _tileDB.GetTileTypeIndex(_selectedTileType); // get index of current tiletyp
			
			// check if there is a tile available at previous or next index of current tile 
			if (scrollDelta > 0 && currentType < _tileDB.tileCount - 1) // scroll up
			{
				if (_tileDB.GetTile(currentType + 1).typeName == "empty") // if the next tile is the empty tile skip it and move to next
				{
					if (currentType + 1 < _tileDB.tileCount - 1) // check so a next tile exist
						currentType += 2;
				}
				else
					currentType += 1;
			} 
			else if (scrollDelta < 0 && currentType > 0)
			{
				if (_tileDB.GetTile(currentType - 1).typeName == "empty")
				{
					if (currentType - 2 >= 0)
						currentType -= 2;
				}
				else
					currentType -= 1;
			}

			ChangeTile(currentType);
		}
		
		// change editmode
		if (Input.GetMouseButtonDown(1))
		{
			if ((int)_currentEditMode < (int)EDIT_MODE.NUM_MODES -1)
				_currentEditMode++;
			else
				_currentEditMode = 0;

			_editModeText.text = string.Format("EDIT MODE: {0}", _currentEditMode);

			// if in place tile mode, delete tile to go into select mode
			if (_currentEditMode == EDIT_MODE.DELETE)
			{
				if (_selectedTile)
				{
					Destroy(_selectedTile);
					_selectedTile = null;
				}
			}
			else
			{
				if (!_selectedTile)
				{
					_selectedTile = Instantiate(_tileDB.GetTile(_selectedTileType).view.mainGO, _tileFolder);
					AddcolliderToSelectedTile();
				}				
			}
		}

		// in placetile mode
		if (_currentEditMode == EDIT_MODE.PLACE_SINGLE || _currentEditMode == EDIT_MODE.PAINT)
		{
			// create plane at zero to raycast at from camera
			Plane targetPlane = new Plane(Vector3.up, Vector3.zero);
			Ray hitRay = _camera.ScreenPointToRay(Input.mousePosition);
			float dst = 0;
			targetPlane.Raycast(hitRay, out dst);

			// get hitpoint from ray and convert to 2d coordinates
			Vector3 hitPoint = hitRay.GetPoint(dst);
			int CoordY = Mathf.CeilToInt(hitPoint.z);
			int CoordX = Mathf.CeilToInt(hitPoint.x);

			// check so coordinates is inside grid
			if(CoordX >= 0 && CoordX < _gridSize.x && CoordY >= 0 && CoordY < _gridSize.y)
			{
				// check if tile is already placed on coords (all tiles is set to "Death" by defualt meaning they are empty)
				bool occupied = _tileProperties[CoordY, CoordX].name != "Death";
				float yPos = 0.0f;
				if (occupied)
					yPos = 1.0f;

				_selectedTile.transform.position = new Vector3(Mathf.CeilToInt(hitPoint.x), yPos, Mathf.CeilToInt(hitPoint.z));

				// set the tiletype and position of this coord in the grid
				if (!occupied)
				{
					if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
						return;

					if (_currentEditMode == EDIT_MODE.PLACE_SINGLE && Input.GetMouseButtonDown(0))
						PlaceTile(CoordX, CoordY);
					else if(_currentEditMode == EDIT_MODE.PAINT && Input.GetMouseButton(0))
						PlaceTile(CoordX, CoordY);
				}
			}
		}
		else // in select placed tile mode
		{
			if (Input.GetMouseButtonDown(0))
			{
				if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
					return;

				// raycast and se if we hit a already placed tile and then we can move it
				Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if(Physics.Raycast(ray, out hit))
					if(hit.collider.gameObject.layer == 9)
					{						
						_tileProperties[(int)hit.transform.position.z, (int)hit.transform.position.x].name = "Death";
						Destroy(hit.collider.gameObject);						
					}
			}
		}

	}

	void PlaceTile(int x, int y)
	{
		_selectedTile = null;
		_tileProperties[y, x].name = _selectedTileType;
		_tileProperties[y, x].position = new Vector2DInt(x, y);

		// spawn new tile
		_selectedTile = Instantiate(_tileDB.GetTile(_selectedTileType).view.mainGO, _tileFolder);
		AddcolliderToSelectedTile();
	}

	void ChangeTile(int index)
	{
		// if a tile is already selected, delete it and create new
		if (_selectedTile)
			Destroy(_selectedTile);

		// set current selected type to new type
		_selectedTileType = _tileDB.GetTile(index).typeName;

		// instantiate tile and add collider
		if(_currentEditMode != EDIT_MODE.DELETE)
		{
			_selectedTile = Instantiate(_tileDB.GetTile(index).view.mainGO, _tileFolder);
			AddcolliderToSelectedTile();
		}

		// change dropdownmenu if tile was changed from scrollwheel
		_dropDownTiles.value = index;
	}

	void AddcolliderToSelectedTile()
	{
		_selectedTile.AddComponent<BoxCollider>();
		_selectedTile.layer = 9;
	}

	public void GenerateGrid(int sizeX, int sizeY)
	{
		if (_grid)
			Destroy(_grid);

		// create gameobject and mesh
		_grid = new GameObject("grid");
		Mesh mesh = new Mesh();

		float half = 0.5f;

		// add mesh filter and meshrenderer and assign them
		_grid.AddComponent<MeshFilter>().mesh = mesh;
		_grid.AddComponent<MeshRenderer>().material = _gridmaterial;

		// create vertex and index array
		Vector3[] vertices = new Vector3[(sizeX * sizeY) * 4];
		int[] indices = new int[(sizeX * sizeY) * 6];

		int column = 0;
		int tileCount = 0;
		int indexVertex = 0;
		int indexIndice = 0;

		// loop over and set all vertices and indices
		for (int i = 0; i < sizeY * sizeX; i++)
		{
			vertices[indexVertex + 0] = new Vector3(tileCount - half, -half, column + half); // top left
			vertices[indexVertex + 1] = new Vector3(tileCount + half, -half, column + half); // top right
			vertices[indexVertex + 2] = new Vector3(tileCount - half, -half, column - half); // bottom left
			vertices[indexVertex + 3] = new Vector3(tileCount + half, -half, column - half); // bottom right

			indices[indexIndice + 0] = indexVertex;
			indices[indexIndice + 1] = indexVertex + 1;
			indices[indexIndice + 2] = indexVertex + 2;
			indices[indexIndice + 3] = indexVertex + 2;
			indices[indexIndice + 4] = indexVertex + 1;
			indices[indexIndice + 5] = indexVertex + 3;

			tileCount++;
			indexVertex += 4;
			indexIndice += 6;
			if (tileCount == sizeX)
			{
				column++;
				tileCount = 0;
			}
		}

		// assign vertices and indices
		mesh.vertices = vertices;
		mesh.triangles = indices;

		_gridSize = new Vector2DInt(sizeX, sizeY);

		GenerateTileData(sizeX, sizeY);
	}

	void GenerateTileData(int sizeX, int sizeY)
	{
		_tileProperties = new TileInfo[sizeY, sizeX];
		for (int y = 0; y < sizeY; y++)
			for (int x = 0; x < sizeX; x++)
				_tileProperties[y, x] = new TileInfo(new Vector2DInt(x, y), "Death"); // set all tiles to start as deathtiles and set position according to coords
	}

	public void OnTileChanged(int index)
	{
		if (_tileDB.GetTile(index).typeName == "empty")
		{
			_dropDownTiles.value = _tileDB.GetTileTypeIndex(_selectedTileType);
			return;
		}

		ChangeTile(index);
	}

	public void SaveLoadButtonPressed(bool save)
	{
		if (save)
			BinarySave(_inputSave.text);
		else
			BinaryLoad(_inputLoad.text);

	}

	public void ClearAllTiles()
	{
		int numTiles = _tileFolder.childCount;
		for (int i = 0; i < numTiles; i++)
			if (_selectedTile && _tileFolder.GetChild(0).gameObject != _selectedTile || !_selectedTile)
				DestroyImmediate(_tileFolder.GetChild(0).gameObject);
	}

	public void GridChanged()
	{
		_gridSizeText.text = string.Format("{0} X {1}", _sliderSizeY.value, _sliderSizeX.value);
		GenerateGrid((int)_sliderSizeX.value, (int)_sliderSizeY.value);

		ClearAllTiles();		
	}

	public void FillGrid()
	{
		ClearAllTiles();

		for (int y = 0; y < _gridSize.y; y++)
			for (int x = 0; x < _gridSize.x; x++)
			{
				_tileProperties[y, x] = new TileInfo(new Vector2DInt(x, y), _selectedTileType);
				GameObject tile = Instantiate(_tileDB.GetTile(_selectedTileType).view.mainGO, new Vector3(x,0,y), Quaternion.identity, _tileFolder);
				tile.AddComponent<BoxCollider>();
				tile.layer = 9;
			}
	}
	
	public void BinarySave(string levelName)
	{
		Directory.CreateDirectory(Constants.TILEMAP_SAVE_FOLDER);

		using (FileStream stream = new FileStream(Path.Combine(Constants.TILEMAP_SAVE_FOLDER, levelName), FileMode.OpenOrCreate, FileAccess.Write))
		using (BinaryWriter writer = new BinaryWriter(stream))
		{
			// write gridsize
			writer.Write(_gridSize.x * _gridSize.y);

			// loop over and write down the properties of each tile in grid
			for (int y = 0; y < _gridSize.y; y++)
				for (int x = 0; x < _gridSize.x; x++)
				{
					_tileProperties[y, x].position.BinarySave(writer);
					writer.Write(_tileProperties[y, x].name);
				}

			_promt.SetAndShow(string.Format("Level {0} Was successfully saved", levelName), () => print("Ok button pressed, seems to work"));
		}
	}

	public void BinaryLoad( string levelname)
	{
		//using (FileStream stream = new FileStream(Path.Combine(Constants.TILEMAP_SAVE_FOLDER, name), FileMode.Open, FileAccess.Read))
		//using (BinaryReader reader = new BinaryReader(stream))
		//{
		//	int tileCount = reader.ReadInt32();        // Read: Num tiles
		//	for (int i = 0; i < tileCount; i++)
		//	{
		//		Vector2DInt tilePosition = Vector2DInt.Zero;
		//		tilePosition.BinaryLoad(reader);       // Read: Position

		//		string typeName = reader.ReadString(); // Read: Tile type name  

		//		tiles.Add(tilePosition, new Tile(tilePosition, typeName)); // TODO: Add serialization / Deserialization for tiles
		//	}
		//}
	}
}
