using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour 
{
	[SerializeField] Text connectionText;
	[SerializeField] Transform[] spawnPoints;
	[SerializeField] Camera sceneCamera;
	public List<ChunkLoader> chunks;
	
	[SerializeField] GameObject serverWindow;
	[SerializeField] InputField username;
	//[SerializeField] InputField roomName;
	//[SerializeField] InputField roomList;
	[SerializeField] InputField messageWindow;
	
	GameObject player;
	Queue<string> messages;
	const int messageCount = 6;
	PhotonView photonView;
	
	
	void Start () 
	{
		photonView = GetComponent<PhotonView> ();
		messages = new Queue<string> (messageCount);
		
		PhotonNetwork.logLevel = PhotonLogLevel.Full;
		PhotonNetwork.ConnectUsingSettings ("1.0");
		StartCoroutine ("UpdateConnectionString");
	}
	
	IEnumerator UpdateConnectionString () 
	{
		while(true)
		{
			connectionText.text = PhotonNetwork.connectionStateDetailed.ToString ();
			yield return null;
		}
	}
	
	void OnJoinedLobby()
	{
		serverWindow.SetActive (true);
	}
	
//	void OnReceivedRoomListUpdate()
//	{
//		roomList.text = "";
//		RoomInfo[] rooms = PhotonNetwork.GetRoomList ();
//		foreach(RoomInfo room in rooms)
//			roomList.text += room.name + "\n";
//	}
	
	public void JoinRoom()
	{
		PhotonNetwork.player.name = username.text;
		RoomOptions roomOptions = new RoomOptions(){ isVisible = true, maxPlayers = 20 };
		PhotonNetwork.JoinOrCreateRoom ("DefaultRoom", roomOptions, TypedLobby.Default);
	}
	
	void OnJoinedRoom()
	{
		serverWindow.SetActive (false);
		StopCoroutine ("UpdateConnectionString");
		connectionText.text = "";
		StartSpawnProcess (0f);
	}
	
	void StartSpawnProcess(float respawnTime)
	{
		sceneCamera.enabled = true;
		StartCoroutine ("SpawnPlayer", respawnTime);
	}
	
	IEnumerator SpawnPlayer(float respawnTime)
	{
		yield return new WaitForSeconds(respawnTime);
		
		int index = Random.Range (0, spawnPoints.Length);
		player = PhotonNetwork.Instantiate("FPS_Player", new Vector3(400, 60, 0), spawnPoints[index].rotation, 0);
		foreach(ChunkLoader chunk in chunks)
			chunk.player = player.transform;

		player.GetComponent<PlayerNetworkMover> ().RespawnMe += StartSpawnProcess;
		player.GetComponent<PlayerNetworkMover> ().SendNetworkMessage += AddMessage;
		sceneCamera.enabled = false;

		AddMessage ("Spawned player: " + PhotonNetwork.player.name);
	}
	
	void AddMessage(string message)
	{
		photonView.RPC ("AddMessage_RPC", PhotonTargets.All, message);
	}
	
	[RPC]
	void AddMessage_RPC(string message)
	{
		messages.Enqueue (message);
		if(messages.Count > messageCount)
			messages.Dequeue();
		
		messageWindow.text = "";
		foreach(string m in messages)
			messageWindow.text += m + "\n";
	}
}