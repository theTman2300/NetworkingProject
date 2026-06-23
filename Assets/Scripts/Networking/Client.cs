using NetworkConnections;
using OSCTools;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// The client is the class that lets game code (Controller and View classes) communicate with 
/// the server, and handles network connections.
/// </summary>
public class Client : MonoBehaviour
{
	public IPAddress ServerIP = IPAddress.Loopback;
	TcpNetworkConnection connection;
	OSCDispatcher dispatcher;
	LocalPlayer localPlayer;

	int playerID;

    // Views subscribe here, on any client:
    public event Action OnNextRound;
    public event Action<int> OnWin;
    public event Action<int, int> OnChoiceReveal; // player, choice
    public event Action<int, int> OnMove; // player, actual steps made

    void Start()
    {
		localPlayer = FindFirstObjectByType<LocalPlayer>();
        TcpClient client = new TcpClient();
		client.Connect(new IPEndPoint(ServerIP, 50006));
		connection = new TcpNetworkConnection(client);
		// TODO: error handling
		Debug.Log("Starting client, connecting to " + ServerIP);

		// Initialize the dispatcher and callbacks for incoming OSC messages:
		dispatcher = new OSCDispatcher();
		dispatcher.ShowIncomingMessages = true;
		Initialize();
    }

	/// <summary>
	/// Called from NetworkConnection callback (connection.Update), when a packet arrives:
	/// </summary>
	void HandlePacket(byte[] packet, IPEndPoint remote) {
		OSCMessageIn mess = new OSCMessageIn(packet);
		Debug.Log("Message arrives on client: " + mess);
		dispatcher.HandlePacket(packet, remote);
	}

	void Update()
    {
		// Check for incoming packets, and deal with them:
		while (connection.Available()>0) {
			HandlePacket(connection.GetPacket(), connection.Remote);
		}
		// TODO: disconnect handling
    }

	void Initialize() {
		// The (optional) list of parameter types (OSCUtil.INT) lets the dispatcher filter
		//  messages that do not satisfy the expected signature (=parameter list):
		dispatcher.AddListener("/PlayerInfo", RecievePrivateInformationCommandRpc, OSCUtil.INT);
		dispatcher.AddListener("/OnSetCardsInHand", SetCardsInHandRpc, OSCUtil.STRING);
		dispatcher.AddListener("/OnSetFirstCard", SetFirstCardRpc, OSCUtil.STRING);

    }

	// ----- Incoming RPCs (events are triggered, and View classes subscribe):
	void RecievePrivateInformationCommandRpc(OSCMessageIn message, IPEndPoint remote)
	{
		int id = message.ReadInt() - 1;
		playerID = id;
		Debug.Log("Connected to server with player index " + playerID);
	}

	void SetCardsInHandRpc(OSCMessageIn message, IPEndPoint remote)
	{
		string[] cards = message.ReadString().Trim().Split(' ');
		StartCoroutine(localPlayer.DrawCards(cards));
	}

    void SetFirstCardRpc(OSCMessageIn message, IPEndPoint remote)
    {
        StartCoroutine(localPlayer.SetFirstCard(message.ReadString()));
    }


    // ----- Outgoing RPCs (called from Controller):
	public void PlayCard(int cardIndex)
	{
		OSCMessageOut message = new OSCMessageOut("/PlayCard").AddInt(cardIndex);
        connection.Send(message.GetBytes());
    }

	public void ResetRpc()
	{
		OSCMessageOut message = new OSCMessageOut("/Reset");
        connection.Send(message.GetBytes());
    }
}
