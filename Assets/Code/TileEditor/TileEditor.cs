using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEditor : MonoBehaviour
{
	[SerializeField] Material _gridmaterial;
	Vector2 _gridDefaultSize = new Vector2(10, 10);
	GameObject _grid;

	void Start()
	{
		Camera.main.GetComponent<UnityTemplateProjects.SimpleCameraController>().frozen = true;
		GenerateGrid((int)_gridDefaultSize.x, (int)_gridDefaultSize.y);
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
		
	}

}
