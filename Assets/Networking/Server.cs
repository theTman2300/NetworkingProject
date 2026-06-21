using NetworkConnections;
using Newtonsoft.Json.Linq;
using OSCTools;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// The Server is the class that manages network connections with all clients, and 
/// communicates with the game code (Model classes).
/// </summary>
public class Server : MonoBehaviour
{
	// ----- General server code:
	TcpListener listener;
	List<TcpNetworkConnection> connections;
	OSCDispatcher dispatcher;

	Crazy8sModel model = new();
	Dictionary<TcpNetworkConnection, int> playerIDs = new Dictionary<TcpNetworkConnection, int>();
	int playerCount = 4;

    #region server code
    // server code
    void Start()
    {
		// This server starts with a listener:
		int port = 50006;
		Debug.Log("Starting server at " + port);
		listener = new TcpListener(IPAddress.Any, port);
		listener.Start();

		connections = new List<TcpNetworkConnection>();

		// Initialize the dispatcher and callbacks for incoming OSC messages:
		dispatcher = new OSCDispatcher();
		dispatcher.ShowIncomingMessages = true;
		Initialize();
    }

    void Update()
    {
		AcceptNewConnections();
		UpdateConnections();
		CleanupConnections();
    }

	void AcceptNewConnections() {
		if (listener.Pending()) {
			TcpClient client = listener.AcceptTcpClient();
			TcpNetworkConnection connection = new TcpNetworkConnection(client);
			connections.Add(connection);
			Debug.Log("Server: Adding new connection from " + connection.Remote);
			ClientJoined(connection);
		}
	}
	void ClientJoined(TcpNetworkConnection newClient) {
		//IMPORTANT: setup up things with playercount here
		if (playerIDs.Count < playerCount) {
			// We had fewer than 2 players, so this new client will be a player.
			playerIDs[newClient] = playerIDs.Count + 1;
			Debug.Log($"Registering new player: {newClient.Remote} = player {playerIDs[newClient]}");
			if (playerIDs.Count == playerCount) { // start game
				Debug.Log("Server: starting game");
				foreach (var pid in playerIDs.Keys) {
					SendPrivateInformationCommand(playerIDs[pid], pid);
				}
				StartGame();
			}
		} else {
			Debug.Log($"Sorry - already have {playerCount} players");
			// Note: this client is still allowed to join as spectator, but not as player!
			// TODO: Send a message to this client
		}
	}

	void UpdateConnections() {
		foreach (TcpNetworkConnection conn in connections) {
			// The connection will call HandlePacket when a packet is available:
			while (conn.Available()>0) {
				HandlePacket(conn.GetPacket(), conn.Remote);
			}
		}
	}

	void HandlePacket(byte[] packet, IPEndPoint remote) {
		OSCMessageIn mess = new OSCMessageIn(packet);
		Debug.Log("Message arrives on server: " + mess);

		dispatcher.HandlePacket(packet, remote);
	}

	void CleanupConnections() {
		// TODO
		// remove disconnected clients here for example
	}

    //end server code
#endregion

    void Initialize() {
		model.Reset();

		//Subscribe to game model events:
		model.OnSetCardsInHand += SetCardsInHand;

		//model.OnNextRound += OnNextRoundRpc;
		//model.OnWin += OnWinRpc;
		//model.OnChoiceReveal += OnChoiceRevealRpc;
		//model.OnMove += OnMoveRpc;



		// (Note: we try to keep the game code independent from networking details.)

		// (Note: no unsubscribe needed in OnDestroy, since the server owns the private board variable.)

		// Subscribe listeners for incoming messages:
		// The (optional) list of parameter types (OSCUtil.INT) lets the dispatcher filter
		//  messages that do not satisfy the expected signature (=parameter list):
		dispatcher.AddListener("/ChooseSteps", ChooseStepsRpc, OSCUtil.INT, OSCUtil.INT);
	}

	// ----- Handle incoming RPCs(called by dispatcher) :

	void ChooseStepsRpc(OSCMessageIn message, IPEndPoint remote)
	{
		int player = message.ReadInt();
		int choice = message.ReadInt();
		//model.ChooseSteps(player, choice);
	}

	// ----- Outgoing RPCs:
	// This RPC is called when a client joins who is a player:
	void SendPrivateInformationCommand(int playerID, TcpNetworkConnection connection) {
		OSCMessageOut message = new OSCMessageOut("/PlayerInfo").AddInt(playerID);
		connection.Send(message.GetBytes()); // private message
	}

	void StartGame()
	{
		model.StartGame();
	}

    void Broadcast(byte[] packet)
    {
        foreach (var conn in connections)
        {
            conn.Send(packet);
        }
    }

    // These RPCs are called by game model events:
	public void SetCardsInHand(int player, string cards)
	{
		OSCMessageOut message = new OSCMessageOut("/OnSetCardsInHand").AddString(cards);
		TcpNetworkConnection playerConnection =  playerIDs.FirstOrDefault(x => x.Value == player + 1).Key;
		playerConnection.Send(message.GetBytes());
    }


 //   public void OnNextRoundRpc()
	//{
	//	OSCMessageOut message = new OSCMessageOut("/OnNextround");
	//	Broadcast(message.GetBytes());
 //   }
	//public void OnWinRpc(int winner)
	//{
 //       OSCMessageOut message = new OSCMessageOut("/OnWin").AddInt(winner);
 //       Broadcast(message.GetBytes());
 //   }
 //   public void OnChoiceRevealRpc(int player, int choice)
 //   {
	//	//something with bundles may be able to be done here
	//	OSCMessageOut message = new OSCMessageOut("/OnChoiceReveal").AddInt(player).AddInt(choice);
 //       Broadcast(message.GetBytes());
 //   }

}
