using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class UnityNetworkManager : MonoBehaviour
{
	public delegate void OnMaxPlayersEvent();
	public event OnMaxPlayersEvent OnMaxPlayers;

	public int MinConnections = 1;
	public int MaxConnections = 1;
	
	private UdpClient _sender = null;
	private UdpClient _receiver = null;
	private int _remotePort = 19784;
	private int _serverPort = 25000;
	private string _playerIP = "";
	private string _receivedString = "";
	
	private static string _messageLog = "";
	
	void Start()
	{
		_playerIP = Network.player.ipAddress;
		Log ("This IP: " + _playerIP);
		StartReceivingIP();
	}
	
	void OnGUI()
	{
		GUI.TextArea(new Rect(0, 0, Screen.width, 0.33f*Screen.height), _messageLog, Screen.width -1);
	}
	
#region Private Methods
	private IEnumerator SendData ()
	{
		Log ("Sending IP via Udp...");
		while(Network.connections.Length < MinConnections)
		{
			if(_sender != null)
			{
				string customMessage = _playerIP;
				_sender.Send (Encoding.ASCII.GetBytes (customMessage), customMessage.Length);
			}
			else
			{
				yield return 0;
			}
			yield return new WaitForSeconds(1f);
		}
		_sender = null;
		if(OnMaxPlayers != null)
		{
			OnMaxPlayers();
		}
		yield return 0;
	}
	
	private void ReceiveData (IAsyncResult result)
	{
		IPEndPoint _receiveIPGroup = new IPEndPoint (IPAddress.Any, _remotePort);
		byte[] received;
		if (_receiver != null)
		{
			received = _receiver.EndReceive (result, ref _receiveIPGroup);
		}
		else
		{
			return;
		}
		_receivedString = Encoding.ASCII.GetString (received);
		Log ("Received " + _receivedString + " via Udp.");
		Log ("Listening for IP via Udp...");
		_receiver.BeginReceive (new AsyncCallback (ReceiveData), null);
	}
	
	private void StartServer()
	{
		_receiver = null;
		Network.InitializeServer(1, _serverPort, !Network.HavePublicAddress());
		_sender = new UdpClient (AddressFamily.InterNetwork);
		IPEndPoint groupEP = new IPEndPoint (IPAddress.Broadcast, _remotePort);
		_sender.Connect (groupEP);
		StartCoroutine (SendData());
	}
#endregion

#region Public Methods
	public void Play()
	{
		if(_receivedString != "")
		{
			Network.Connect(_receivedString, _serverPort);
			_receiver = null;
		}
		else
		{
			StartServer();
		}
	}
	
	public void StartReceivingIP ()
	{
		try
		{
			if (_receiver == null)
			{
				Log ("Listening for IP via Udp...");
				_receiver = new UdpClient (_remotePort);
				_receiver.BeginReceive (new AsyncCallback (ReceiveData), null);
			}
		}
		catch (SocketException e)
		{
			Log (e.Message);
		}
	}
#endregion

#region Events
	private void OnConnectedToServer()
	{
		Log ("Server Joined");
	}
	private void OnServerInitialized()
	{
		Log ("Server Initializied");
	}
	private void OnPlayerConnected(NetworkPlayer player) {
		Log ("Player " + player.guid + " connected from " + player.ipAddress + ":" + player.port);
	}
	private void OnPlayerDisconnected(NetworkPlayer player) {
		Log ("Player " + player.guid + " disconnected from " + player.ipAddress + ":" + player.port);
	}
#endregion

#region Log
	void Log(string msg)
	{
		Debug.Log(msg);
		_messageLog += "\n" + msg;
	}
#endregion
}