using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Grid : MonoBehaviour {

	public int xWidth, zWidth;
	public Vector3 startingPoint;

	private Mesh mesh;
	private Vector3[] vertices;


	private void Awake () {
		Generate();
	}

	private void Generate () {
		GeneratePoints();
		meshFromVertices();
		moveVertices();
		stretchMesh();
		GetComponent<MeshCollider>().sharedMesh = mesh;
	}

	private void GeneratePoints(){
		vertices = new Vector3[(xWidth + 1) * (zWidth + 1)];
		for (int i = 0, z = 0; z <= zWidth; z++) {
			for (int x = 0; x <= xWidth; x++, i++) {
				vertices[i] = new Vector3(x, 0, z);
			}
		}
	}

	private void meshFromVertices(){
		//generate a mesh from the vertices
		mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = GenerateTriangles();
		mesh.RecalculateNormals();
		GetComponent<MeshFilter>().mesh = mesh;
	}

	private int[] GenerateTriangles(){
		int[] triangles = new int[xWidth * zWidth * 6];
		for (int ti = 0, vi = 0, z = 0; z < zWidth; z++, vi++) {
			for (int x = 0; x < xWidth; x++, ti += 6, vi++) {
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + xWidth + 1;
				triangles[ti + 5] = vi + xWidth + 2;
			}
		}
		return triangles;
	}
	public void moveVertices(){
		for (int i = 0, z = 0; z <= zWidth; z++) {
			for (int x = 0; x <= xWidth; x++, i++) {
				vertices[i].y = Mathf.PerlinNoise((vertices[i].x) * 0.3f, (vertices[i].z) * 0.3f) * 300f - 150f;
			}
		}
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
	}

	public void stretchMesh(){
		for (int i = 0, z = 0; z <= zWidth; z++) {
			for (int x = 0; x <= xWidth; x++, i++) {
				vertices[i].x *= 300;
				vertices[i].z *= 300;
				vertices[i].x += startingPoint.x;
				vertices[i].y += startingPoint.y;
				vertices[i].z += startingPoint.z;
			}
		}
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}
}