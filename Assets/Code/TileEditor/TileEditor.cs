using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


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
	string levelName = "EditorTest";
	Camera _camera;

	// grid
	[Header("Grid")]
	[SerializeField] Material _gridmaterial;
	Vector2DInt _gridDefaultSize = new Vector2DInt(10, 10);
	Vector2DInt _gridSize;
	GameObject _grid;

	// tileinfo
	TileInfo[,] _tileProperties;

	// current
	GameObject _selectedTile;
	string _selectedTileType;

	TileDatabase TB;

	void Start()
	{
		// get references
		TB = TileDatabase.instance;
		_camera = Camera.main;

		// freeze camera
		_camera.GetComponent<UnityTemplateProjects.SimpleCameraController>().frozen = true;

		// generate standard 10x10 grid
		GenerateGrid(_gridDefaultSize.x, _gridDefaultSize.y);

		// create tile of first type in typearray
		_selectedTile = Instantiate(TB.GetTile(0).view.MainGo);
		_selectedTileType = TB.GetTileType(0);

		// add collider and set layer of tile (need this to be able to select alredy placed tiles)
		AddcolliderToSelectedTile();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.S)) // temp way of saving
			BinarySave();

		// go to next tiletype in tiletype array
		if(Input.GetAxisRaw("Mouse ScrollWheel") > 0)
		{
			int currentType = TB.GetTileTypeIndex(_selectedTileType);
			if (currentType != -1 && currentType < TB.GetTileCount -1)
			{
				_selectedTileType = TB.GetTileType(currentType +1);

				// if a tile is already selected, delete it and create new
				if (_selectedTile)
					Destroy(_selectedTile);

				_selectedTile = Instantiate(TB.GetTile(currentType +1).view.MainGo);
				AddcolliderToSelectedTile();
			}
		}

		// go to previous tiletype in tiletype array
		if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
		{
			int currentType = TB.GetTileTypeIndex(_selectedTileType);
			if (currentType > 0)
			{
				_selectedTileType = TB.GetTileType(currentType -1);
				if (_selectedTile)
					Destroy(_selectedTile);

				_selectedTile = Instantiate(TB.GetTile(currentType - 1).view.MainGo);
				AddcolliderToSelectedTile();
			}
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
				_selectedTile = Instantiate(TB.GetTile(_selectedTileType).view.MainGo);
				AddcolliderToSelectedTile();
			}
		}

		// in placetile mode
		if (_selectedTile)
		{
			Plane targetPlane = new Plane(Vector3.up, Vector3.zero);
			Ray hitRay = _camera.ScreenPointToRay(Input.mousePosition);
			float dst = 0;
			targetPlane.Raycast(hitRay, out dst);
			Vector3 hitPoint = hitRay.GetPoint(dst);
			int CoordY = Mathf.CeilToInt(hitPoint.z);
			int CoordX = Mathf.CeilToInt(hitPoint.x);

			if(CoordX >= 0 && CoordX < _gridSize.x && CoordY >= 0 && CoordY < _gridSize.y)
			{
				bool occupied = _tileProperties[CoordY, CoordX].name != "Death";
				float yPos = 0.0f;
				if (occupied)
					yPos = 1.0f;

				_selectedTile.transform.position = new Vector3(Mathf.CeilToInt(hitPoint.x), yPos, Mathf.CeilToInt(hitPoint.z));

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
				Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if(Physics.Raycast(ray, out hit))
					if(hit.collider.gameObject.layer == 9)
					{
						_selectedTileType = _tileProperties[(int)hit.transform.position.z, (int)hit.transform.position.x].name;
						_tileProperties[(int)hit.transform.position.z, (int)hit.transform.position.x].name = "Death";
						_selectedTile = hit.collider.gameObject;
					}
			}
		}

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

		_grid = new GameObject("grid");
		Mesh mesh = new Mesh();

		float half = 0.5f;

		_grid.AddComponent<MeshFilter>().mesh = mesh;
		_grid.AddComponent<MeshRenderer>().material = _gridmaterial;

		Vector3[] vertices = new Vector3[(sizeX * sizeY) * 4];
		int[] indices = new int[(sizeX * sizeY) * 6];

		int column = 0;
		int tileCount = 0;
		int indexVertex = 0;
		int indexIndice = 0;

		for(int i =0; i < sizeY * sizeX; i++)
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
			if(tileCount == sizeX)
			{
				column++;
				tileCount = 0;
			}
		}

		mesh.vertices = vertices;
		mesh.triangles = indices;

		_gridSize = new Vector2DInt(sizeX, sizeY);

		GenerateTileData(sizeX, sizeY);
	}

	void GenerateTileData(int sizeX, int sizeY)
	{
		_tileProperties = new TileInfo[sizeY,sizeX];
		for (int y = 0; y < sizeY; y++)
			for (int x = 0; x < sizeX; x++)
				_tileProperties[y, x] = new TileInfo(new Vector2DInt(x,y),"Death");
	}


	public void BinarySave()
	{
		Directory.CreateDirectory(Constants.TILEMAP_SAVE_FOLDER);

		using (FileStream stream = new FileStream(Path.Combine(Constants.TILEMAP_SAVE_FOLDER, levelName), FileMode.OpenOrCreate, FileAccess.Write))
		using (BinaryWriter writer = new BinaryWriter(stream))
		{
			writer.Write(_gridSize.x * _gridSize.y);
			for (int y = 0; y < _gridSize.y; y++)
				for (int x = 0; x < _gridSize.x; x++)
				{
					_tileProperties[y, x].position.BinarySave(writer);
					writer.Write(_tileProperties[y, x].name);
				}
		}
	}

	public void BinaryLoad()
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
