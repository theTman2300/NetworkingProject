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

	int playerID;

    // Views subscribe here, on any client:
    public event Action OnNextRound;
    public event Action<int> OnWin;
    public event Action<int, int> OnChoiceReveal; // player, choice
    public event Action<int, int> OnMove; // player, actual steps made

    void Start()
    {
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
		dispatcher.AddListener("/OnSetCardsInHand", SetCardsInHand, OSCUtil.STRING);
		//dispatcher.AddListener("/OnNextround", OnNextRoundRpc);
		//dispatcher.AddListener("/OnWin", OnWinRpc, OSCUtil.INT);
		//dispatcher.AddListener("/OnChoiceReveal", OnChoiceRevealRpc, OSCUtil.INT, OSCUtil.INT);
		//dispatcher.AddListener("/OnMove", OnMoveRpc, OSCUtil.INT, OSCUtil.INT);
    }

	// ----- Incoming RPCs (events are triggered, and View classes subscribe):
	void RecievePrivateInformationCommandRpc(OSCMessageIn message, IPEndPoint remote)
	{
		int id = message.ReadInt() - 1;
		playerID = id;
		Debug.Log("Connected to server with player index " + playerID);
	}

	void SetCardsInHand(OSCMessageIn message, IPEndPoint remote)
	{
		string[] cards = message.ReadString().Split(' ');


		string cardsString = "count: " + cards.Length + "   ";
		foreach (string card in cards)
		{
			cardsString += card + " ";
		}
		Debug.Log(cardsString);
	}

	void OnNextRoundRpc(OSCMessageIn message, IPEndPoint remote)
	{
		OnNextRound.Invoke();
	}
	void OnWinRpc(OSCMessageIn message, IPEndPoint remote)
	{
		int player = message.ReadInt();
		OnWin.Invoke(player);
	}
	void OnChoiceRevealRpc(OSCMessageIn message, IPEndPoint remote)
	{
		int player = message.ReadInt();
		int choice = message.ReadInt();
		OnChoiceReveal.Invoke(player, choice);
	}
	void OnMoveRpc(OSCMessageIn message, IPEndPoint remote)
	{
		int player = message.ReadInt();
		int stepsMade = message.ReadInt();
		OnMove.Invoke(player, stepsMade);
	}

	// ----- Outgoing RPCs (called from Controller):

	public void ChooseStepsRpc(int choice)
	{
        OSCMessageOut message = new OSCMessageOut("/ChooseSteps").AddInt(playerID).AddInt(choice);
		connection.Send(message.GetBytes());
	}

	public void ResetRpc()
	{
		OSCMessageOut message = new OSCMessageOut("/Reset");
        connection.Send(message.GetBytes());
    }
}
