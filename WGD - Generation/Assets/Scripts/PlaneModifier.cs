using UnityEngine;
using System.Collections;

public class PlaneModifier : MonoBehaviour {

	private Mesh sourceMesh;
	private Mesh unsharedVertexMesh;
	private MeshRenderer meshRenderer;
	private MeshCollider meshCollider;

	private int [] triangles ;
	private int[] indices;
	private Vector2 [] uvs;
	private Vector3 [] vertices;

	void Start () {
		Generate (1);
	}

	public void Generate (int seed) {

		//Layout of vertices of the plane:
		//
		// Each corner at (5,5) , (-5,5) , (5,-5) , (-5,-5)
		//														The UV coordinate system
		//(-5,5)->...10-9-8-7-6-5-4-3-2-1-0..<-(5,5)............(1,0)->...10-9-8-7-6-5-4-3-2-1-0..<-(0,0)...........
		//...........21-.-.-.-.-.-.-.-.-.-11..............................21-.-.-.-.-.-.-.-.-.-11...................
		//...........32-.-.-.-.-.-.-.-.-.-22..............................32-.-.-.-.-.-.-.-.-.-22...................
		//...........43-.-.-.-.-.-.-.-.-.-33..............................43-.-.-.-.-.-.-.-.-.-33...................
		//...........54-.-.-.-.-.-.-.-.-.-44..............................54-.-.-.-.-.-.-.-.-.-44...................
		//...........65-.-.-.-.-.-.-.-.-.-55..............................65-.-.-.-.-.-.-.-.-.-55...................
		//...........76-.-.-.-.-.-.-.-.-.-66..............................76-.-.-.-.-.-.-.-.-.-66...................
		//...........87-.-.-.-.-.-.-.-.-.-77..............................87-.-.-.-.-.-.-.-.-.-77...................
		//...........98-.-.-.-.-.-.-.-.-.-88..............................98-.-.-.-.-.-.-.-.-.-88...................
		//..........109-.-.-.-.-.-.-.-.-.-99.............................109-.-.-.-.-.-.-.-.-.-99...................
		//(-5,-5)->.120-.-.-.-.-.-.-.-.-.-110.<-(5,-5).........(1,1)->...120-.-.-.-.-.-.-.-.-.-110.<-(0,1)..........

		unsharedVertexMesh = new Mesh();
		sourceMesh = GetComponent<MeshFilter> ().mesh;
		unsharedVertexMesh = GetComponent<MeshFilter> ().mesh;
		meshCollider = GetComponent<MeshCollider> ();

		//Take the sourceMesh and make all vertices independent
		int[] sourceIndices = sourceMesh.GetTriangles(0);
		Vector3[] sourceVerts = sourceMesh.vertices;
		Vector2[] sourceUVs = sourceMesh.uv;
		
		indices = new int[sourceIndices.Length];
		vertices = new Vector3[sourceIndices.Length];
		uvs = new Vector2[sourceIndices.Length];
		
		// Create a unique vertex for every index in the original Mesh:
		for(int i = 0; i < sourceIndices.Length; i++)
		{
			indices[i] = i;
			vertices[i] = sourceVerts[sourceIndices[i]];
			uvs[i] = sourceUVs[sourceIndices[i]];
		}

		unsharedVertexMesh.vertices = vertices;
		unsharedVertexMesh.uv = uvs;
		unsharedVertexMesh.SetTriangles(indices, 0);

		triangles = unsharedVertexMesh.triangles;

		// Assigns each triangle a colour from the Pallette/Material
		//NOTE: USE THIS SECTION IF USING A PALLETTE TEXTURE
//		for (int k = 0; k< unsharedVertexMesh.triangles.Length; k += 3) {
//			float RandPosx = 0.25f + 0.5f * (Mathf.Round (2f * Random.value));
//			float RandPosy = 0.25f + 0.5f * (Mathf.Round (2f * Random.value));
//			UVs [Triangles [k]] = UVs [Triangles [k + 1]] = UVs [Triangles [k + 2]] = new Vector2 (RandPosx, RandPosy);
//		}

		// NOTE USE THIS IF USING A TEXTURE
		for (int i = 0; i < unsharedVertexMesh.triangles.Length; i += 3) {
			float RandPosx = 0.25f * (Random.value);
			float RandPosy = 0.25f * (Random.value);
			float sideOfTriangle01 = (vertices[triangles [i + 1]] - vertices[triangles [i]]).magnitude;
			float sideOfTriangle02 = (vertices[triangles [i + 2]] - vertices[triangles [i]]).magnitude;
			float sideOfTriangle12 = (vertices[triangles [i + 1]] - vertices[triangles [i + 2]]).magnitude;
			float angle = Mathf.Acos((Mathf.Pow(sideOfTriangle01, 2) + Mathf.Pow(sideOfTriangle02, 2) - Mathf.Pow(sideOfTriangle12, 2)) / (2f * sideOfTriangle01 * sideOfTriangle02)); //Cosine Rule
			uvs [triangles [i]] = new Vector2(0,0); 
			uvs [triangles [i + 1]] = new Vector2 (1f,0); 
			uvs [triangles [i + 2]] = new Vector2 ((sideOfTriangle02 * Mathf.Cos(angle)) / sideOfTriangle01, (sideOfTriangle02 * Mathf.Sin(angle)) / sideOfTriangle01); 

			// Arranges UVs same as the triangle in the mesh with same distances.
		}

		unsharedVertexMesh.uv = uvs;
		RecalculateVertices();
	}

	public void RecalculateVertices () {
		for (int i = 0; i < vertices.Length; i++) {
			float absoluteX = 0.25f + vertices[i].x + transform.position.x;
			float absoluteZ = 0.25f + vertices[i].z + transform.position.z;
			float shift = 10000f;

			vertices[i].y = (-30f) + PerlinCalculate (5f, 50f, absoluteX, absoluteZ, shift);
			vertices[i].y += PerlinCalculate (25f, 30f, absoluteX, absoluteZ, shift);
			vertices[i].y += PerlinCalculate (50f, 50f, absoluteX, absoluteZ, shift);
			vertices[i].y += PerlinCalculate (2f, 3f, absoluteX, absoluteZ, shift);
			vertices[i].y += 0.25f * (PerlinCalculate (2f, 1f, absoluteX, absoluteZ, shift) - 1);

			//vertices[i].y = 20f * Mathf.PerlinNoise((absoluteX + 10000f) / 50f, (absoluteZ + 10000f) / 50f);
			//vertices[i].y += 0.25f * ((-1) + 2f * Mathf.PerlinNoise(absoluteX, absoluteZ));
		}
		unsharedVertexMesh.vertices = vertices; 
		RecalculateCollider();
	}

	private float PerlinCalculate (float h, float var, float absX, float absZ, float pShift) {
		return h * Mathf.PerlinNoise ((absX + pShift) / var, (absZ + pShift) / var);
	}

	public void RecalculateCollider () {
		if(GetComponent<MeshCollider>())
			Destroy(meshCollider);
		meshCollider = gameObject.AddComponent<MeshCollider>();
		this.meshCollider.sharedMesh = unsharedVertexMesh;

		unsharedVertexMesh.RecalculateBounds ();
		unsharedVertexMesh.RecalculateNormals ();
	}
}
