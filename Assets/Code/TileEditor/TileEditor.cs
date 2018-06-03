﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;


public class TileInfo
{
	public Vector2DInt position;
	public string name = "";

	public TileInfo(Vector2DInt pos, string n)
	{
		position = pos;
		name = n;
	}
}

public class TileEditor : MonoBehaviour
{	
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

	TileDatabase TB;

	//UI
	[Header("UI")]
	[SerializeField] InputField _inputLoad;
	[SerializeField] InputField _inputSave;
	[SerializeField] Dropdown   _dropDownTiles;
	[SerializeField] Text		_gridSizeText;
	[SerializeField] Slider		_sliderSizeX;
	[SerializeField] Slider		_sliderSizeY;

	void Start()
	{
		// get references
		TB = TileDatabase.instance;
		_camera = Camera.main;

		// freeze camera
		_camera.GetComponent<UnityTemplateProjects.SimpleCameraController>().frozen = true;

		// generate standard 10x10 grid
		GenerateGrid(_gridDefaultSize.x, _gridDefaultSize.y);

		// add all existing tiletypes to dropdown menu
		_dropDownTiles.options.Clear();
		for(int i =0; i < TB.GetTileCount; i++)		
			_dropDownTiles.options.Add(new Dropdown.OptionData(TB.GetTileType(i)));
		
		// create tile of first type in typearray
		_selectedTile = Instantiate(TB.GetTile(0).view.MainGo, _tileFolder);
		_selectedTileType = TB.GetTileType(0);

		// add collider and set layer of tile (need this to be able to select alredy placed tiles)
		AddcolliderToSelectedTile();
	}

	void Update()
	{
		// check if scrollwheel has moved
		float scrollDelta = Input.GetAxisRaw("Mouse ScrollWheel");
		if(scrollDelta != 0)
		{
			int currentType = TB.GetTileTypeIndex(_selectedTileType); // get index of current tiletyp

			// check if there is a tile available at previous or next index of current tile 
			if (scrollDelta > 0 && currentType != -1 && currentType < TB.GetTileCount - 1) // scroll up
				currentType++;
			else if (scrollDelta < 0 && currentType > 0) // scroll down
				currentType--;

			ChangeTile(currentType);
		}
		
		if (Input.GetMouseButtonDown(1))
		{
			// if in place tile mode, delete tile to go into select mode
			if (_selectedTile)
			{
				Destroy(_selectedTile);
				_selectedTile = null;
			}
			else
			{
				_selectedTile = Instantiate(TB.GetTile(_selectedTileType).view.MainGo, _tileFolder);
				AddcolliderToSelectedTile();
			}
		}

		// in placetile mode
		if (_selectedTile)
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
				if (Input.GetMouseButtonDown(0) && !occupied)
				{
					_selectedTile = null;
					_tileProperties[CoordY, CoordX].name = _selectedTileType;
					_tileProperties[CoordY, CoordX].position = new Vector2DInt(CoordX, CoordY);

				}
			}
		}
		else // in select placed tile mode
		{
			if (Input.GetMouseButtonDown(0))
			{
				// raycast and se if we hit a already placed tile and then we can move it
				Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if(Physics.Raycast(ray, out hit))
					if(hit.collider.gameObject.layer == 9)
					{
						_selectedTileType = _tileProperties[(int)hit.transform.position.z, (int)hit.transform.position.x].name;
						_tileProperties[(int)hit.transform.position.z, (int)hit.transform.position.x].name = "Death";
						_selectedTile = hit.collider.gameObject;

						_dropDownTiles.value = TB.GetTileTypeIndex(_selectedTileType);
					}
			}
		}

	}

	void ChangeTile(int index)
	{
		// if a tile is already selected, delete it and create new
		if (_selectedTile)
			Destroy(_selectedTile);

		// set current selected type to new type
		_selectedTileType = TB.GetTileType(index);

		// instantiate tile and add collider
		_selectedTile = Instantiate(TB.GetTile(index).view.MainGo, _tileFolder);
		AddcolliderToSelectedTile();

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
		ChangeTile(index);
	}

	public void SaveLoadButtonPressed(bool save)
	{
		if (save)
			BinarySave(_inputSave.text);
		else
			BinaryLoad(_inputLoad.text);

	}

	public void GridChanged()
	{
		_gridSizeText.text = string.Format("{0} X {1}", _sliderSizeY.value, _sliderSizeX.value);
		GenerateGrid((int)_sliderSizeX.value, (int)_sliderSizeY.value);

		int numTiles = _tileFolder.childCount;
		for(int i =0; i < numTiles; i++)		
			DestroyImmediate(_tileFolder.GetChild(0).gameObject);				
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
