using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Local game manager. In this example, game starts when all players join,
/// expecting two. The server shuts down when any player disconnects,
/// as it is presuming a two player game and the game can't continue.
/// This can be changed so that the server only shuts down when all
/// players disconnect, if your game handles that.
/// </summary>
public class LocalGameManager : MonoBehaviour {

	public GameObject GameSpawner = null;
	public UnityNetworkManager NetworkManager = null;
	
	private GameObject _GameSpawner = null;
	private int _NumConnections = 0;
	
	void OnGUI()
	{
		if (!Network.isClient && !Network.isServer && NetworkManager != null)
		{
			if (GUI.Button(new Rect(0, 0.33f*Screen.height, Screen.width, 0.33f*Screen.height), "PLAY"))
				NetworkManager.Play();
		}
	}
	
	void Start ()
	{
		NetworkManager.OnMaxPlayers += BeginGame;
	}
	
	void OnDestroy()
	{
		NetworkManager.OnMaxPlayers -= BeginGame;
	}
	
	void Update()
	{
		if(Network.connections.Length != _NumConnections && _NumConnections != 0)
		{
			Destroy (_GameSpawner);
			Debug.Log("Opponent fled.");

			Network.Disconnect();
			_NumConnections = 0;
		}
	}
	
	private void BeginGame()
	{
		_GameSpawner = Network.Instantiate(GameSpawner, Vector3.zero, Quaternion.identity, 0) as GameObject;
		_NumConnections = Network.connections.Length;
	}
}
