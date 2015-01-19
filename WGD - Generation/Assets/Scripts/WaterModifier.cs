using UnityEngine;
using System.Collections;

public class WaterModifier : MonoBehaviour {

	private Mesh sourceMesh;
	private Mesh unsharedVertexMesh;
	private MeshRenderer meshRenderer;

	private int [] Triangles ;
	private int[] Indices;
	private Vector2 [] UVs;
	private Vector3 [] Vertices ; 

	float PsuedoRand2 (float a, float b){
		float cx = Mathf.Abs(Mathf.Cos(Mathf.Log(  Mathf.Pow(Mathf.Tan(a)- Mathf.Tan(b) + Mathf.PI,4  ))));
		float cy = Mathf.Abs(Mathf.Sin((Mathf.Log( Mathf.Abs( Mathf.Tan(a)+ Mathf.Tan(b) + Mathf.Exp(1)) ))));
		float CANTOR = 0;
		float output;
		for(int p = 1 ; p < 4 ; p++){
			CANTOR += Mathf.RoundToInt((Mathf.Pow(10, 2*p - 1) *cx )- 0.5f) * Mathf.Pow(10,-(2*p -1)) + Mathf.RoundToInt((Mathf.Pow(10,2*p) *cy ) -0.5f) * Mathf.Pow(10,-(2*p));
		}
		output = Mathf.Sin (CANTOR);
		return output;
	}


	void Start () {

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

		Mesh unsharedVertexMesh = new Mesh();
		sourceMesh = GetComponent<MeshFilter> ().mesh;
		unsharedVertexMesh = GetComponent<MeshFilter> ().mesh;

		//Take the sourceMesh and make all vertices independent
		int[] sourceIndices = sourceMesh.GetTriangles(0);
		Vector3[] sourceVerts = sourceMesh.vertices;
		Vector2[] sourceUVs = sourceMesh.uv;
		
		Indices = new int[sourceIndices.Length];
		Vertices = new Vector3[sourceIndices.Length];
		UVs = new Vector2[sourceIndices.Length];
		
		// Create a unique vertex for every index in the original Mesh:
		for(int i = 0; i < sourceIndices.Length; i++)
		{
			Indices[i] = i;
			Vertices[i] = sourceVerts[sourceIndices[i]];
			UVs[i] = sourceUVs[sourceIndices[i]];
		}

		unsharedVertexMesh.vertices = Vertices;
		unsharedVertexMesh.uv = UVs;
		unsharedVertexMesh.SetTriangles(Indices, 0);

		Triangles = unsharedVertexMesh.triangles;

		// Assigns each triangle a colour from the Pallette/Material
		for (int k = 0; k< unsharedVertexMesh.triangles.Length; k += 3) {
			float RandPosx = 0.25f + 0.5f * (Mathf.Round (2f * Random.value));
			float RandPosy = 0.25f + 0.5f * (Mathf.Round (2f * Random.value));
			UVs [Triangles [k]] = UVs [Triangles [k + 1]] = UVs [Triangles [k + 2]] = new Vector2 (RandPosx, RandPosy);
		}

		unsharedVertexMesh.uv = UVs;

		for (int l = 0; l< Vertices.Length; l++) {
			Vertices [l].y += PsuedoRand2(Mathf.Round(Vertices [l].x + transform.position.x),Mathf.Round(Vertices [l].z + transform.position.z));
				}
		unsharedVertexMesh.vertices = Vertices; 

		unsharedVertexMesh.RecalculateBounds ();
		unsharedVertexMesh.RecalculateNormals ();

	}
	
	float ripple = 0;
	void Update () {
		unsharedVertexMesh = GetComponent<MeshFilter> ().mesh;

		for (int l = 0; l< Vertices.Length; l++) {
			//Vertices [l].y = PsuedoRand2(Vertices [l].x + transform.position.x + ripple,Vertices [l].z + transform.position.z + ripple);
			Vertices [l].y = 0.1f * PsuedoRand2(Vertices [l].x + transform.position.x,Vertices [l].z + transform.position.z ) + 0.05f * Mathf.Sin(0.2f* Mathf.PI * Vertices [l].x + ripple) - 0.2f;
		}
		ripple += 0.01f ;
		print (ripple);
		unsharedVertexMesh.vertices = Vertices; 
		unsharedVertexMesh.RecalculateBounds ();
		unsharedVertexMesh.RecalculateNormals ();
	}
}
