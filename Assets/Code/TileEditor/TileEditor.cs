using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TILE_TYPE
{
	TYPE_LAVA,
	TYPE_ICE,
	TYPE_DEATH,
	TYPE_UNDIFINED
}

public class TileInfo
{
	public TILE_TYPE type = TILE_TYPE.TYPE_UNDIFINED;
}

public class TileEditor : MonoBehaviour
{
	[Header("TileModels")]
	[SerializeField] GameObject[] _tileModels;

	Camera _camera;

	// grid
	[Header("Grid")]
	[SerializeField] Material _gridmaterial;
	Vector2 _gridDefaultSize = new Vector2(10, 10);
	Vector2 _gridSize;
	GameObject _grid;

	// tileinfo
	TileInfo[,] _tileProperties;

	// current
	GameObject _selectedTile;
	TILE_TYPE _selectedTileType;

	void Start()
	{
		Camera.main.GetComponent<UnityTemplateProjects.SimpleCameraController>().frozen = true;
		GenerateGrid((int)_gridDefaultSize.x, (int)_gridDefaultSize.y);

		_selectedTile = Instantiate(_tileModels[0]);
		_selectedTileType = TILE_TYPE.TYPE_LAVA;
	}

	void Update()
	{

		if(Input.GetAxisRaw("Mouse ScrollWheel") > 0)
		{
			int currentType = (int)_selectedTileType;
			if (currentType < (int)TILE_TYPE.TYPE_UNDIFINED - 1)
			{
				_selectedTileType = (TILE_TYPE)(currentType + 1);
				if (_selectedTile)
					Destroy(_selectedTile);

				_selectedTile = Instantiate(_tileModels[(int)_selectedTileType]);
			}
		}

		if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
		{
			int currentType = (int)_selectedTileType;
			if (currentType > 0)
			{
				_selectedTileType = (TILE_TYPE)(currentType - 1);
				if (_selectedTile)
					Destroy(_selectedTile);

				_selectedTile = Instantiate(_tileModels[(int)_selectedTileType]);
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
				_selectedTile = Instantiate(_tileModels[(int)_selectedTileType]);
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
				bool occupied = _tileProperties[CoordY, CoordX].type != TILE_TYPE.TYPE_UNDIFINED;
				float yPos = 0.0f;
				if (occupied)
					yPos = 1.0f;

				_selectedTile.transform.position = new Vector3(Mathf.CeilToInt(hitPoint.x), yPos, Mathf.CeilToInt(hitPoint.z));

				if (Input.GetMouseButtonDown(0) && !occupied)
				{
					_selectedTile = null;
					_tileProperties[CoordY, CoordX].type = _selectedTileType;
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
						_selectedTileType = _tileProperties[(int)hit.transform.position.z, (int)hit.transform.position.x].type;
						_tileProperties[(int)hit.transform.position.z, (int)hit.transform.position.x].type = TILE_TYPE.TYPE_UNDIFINED;
						_selectedTile = hit.collider.gameObject;
					}
			}
		}

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

		_gridSize = new Vector2(sizeX, sizeY);

		GenerateTileData(sizeX, sizeY);
	}

	void GenerateTileData(int sizeX, int sizeY)
	{
		_tileProperties = new TileInfo[sizeY,sizeX];
		for (int y = 0; y < sizeY; y++)
			for (int x = 0; x < sizeX; x++)
				_tileProperties[y, x] = new TileInfo();
	}

}
