using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChunkLoader : MonoBehaviour {
	
	public Transform groundChunk;
	public Transform player;

	private PlaneModifier planeMod;
	private Vector3 chunkPos;
	private Vector3 playerPos;

	private int chunksMovedX = 0;
	private int chunksMovedZ = 0;

	private List<PlaneModifier> chunkettes;

	void Start () {
		chunkPos = transform.position;
		chunkettes = new List<PlaneModifier>();
		for(int i = 0; i < transform.childCount; i++) {
			chunkettes.Add (transform.GetChild(i).GetComponent<PlaneModifier>());
		}
	}

	void Update () {
		playerPos = player.position;
		chunkPos = transform.position;
		float xOffset = playerPos.x - chunkPos.x;
		float zOffset = playerPos.z - chunkPos.z;

		//chunksMovedX = Mathf.RoundToInt(((xOffset + 133f * IsNegative(xOffset)) / 270f) - 0.5f);
		//chunksMovedZ = Mathf.RoundToInt(((zOffset + 133f * IsNegative(zOffset)) / 270f) - 0.5f);

//		if(Input.GetKeyDown(KeyCode.E))
//			print (chunksMovedX);
//
//		if(chunksMovedX != 0) {
//			chunkPos += 270f * chunksMovedX * Vector3.right;
//			RecalculateChunkettes ();
//		}
//		else if(chunksMovedZ != 0) {
//			chunkPos += 270f * chunksMovedZ * Vector3.forward;
//			RecalculateChunkettes ();
//		}


		/*	number in if statement is (no. of chunks * 15)
		 * 	number in chunkPos += statement is 
		 */
		if (xOffset > 137f){
			transform.position += Mathf.FloorToInt((xOffset + 133f) / 270f) * 270f * Vector3.right;
			//print (true);
			//transform.position = chunkPos;
			foreach(PlaneModifier chunkette in chunkettes) {
				chunkette.RecalculateVertices();
			}
		}
		else if (xOffset < -137f){
			transform.position += Mathf.CeilToInt((xOffset - 133f) / 270f) * 270f * Vector3.right;
			//print (false);
			//transform.position = chunkPos; 
			foreach(PlaneModifier chunkette in chunkettes) {
				chunkette.RecalculateVertices();
			}
		}
		else if (zOffset > 137f){
			transform.position += Mathf.FloorToInt((zOffset + 133f) / 270f) * 270f * Vector3.forward;
			//transform.position = chunkPos; 
			foreach(PlaneModifier chunkette in chunkettes) {
				chunkette.RecalculateVertices();
			}
		}
		else if (zOffset < -137f){
			transform.position += Mathf.CeilToInt((zOffset - 133f) / 270f) * 270f * Vector3.forward;
			//transform.position = chunkPos; 
			foreach(PlaneModifier chunkette in chunkettes) {
				chunkette.RecalculateVertices();
			}
		}
	}

	private void RecalculateChunkettes () {
		foreach(PlaneModifier chunkette in chunkettes) {
			chunkette.RecalculateVertices();
		}
	}

	private int IsNegative (float input) {
		if (input >= 0) return 1;
		else return -1;
	}
}
