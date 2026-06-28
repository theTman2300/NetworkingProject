using NetworkConnections;
using Newtonsoft.Json.Linq;
using OSCTools;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

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
		model.OnSetFirstCard += SetFirstCard;
		model.OnPlayerPlayedCard += PlayerPlayedCard;
		model.OnChangePlayerTurn += ChangePlayerTurn;
		model.OnPlayerDrawCards += PlayerDrawCards;
		model.OnPlayerChooseSuit += PlayerChooseSuit;

        // (Note: we try to keep the game code independent from networking details.)

        // (Note: no unsubscribe needed in OnDestroy, since the server owns the private board variable.)

        // Subscribe listeners for incoming messages:
        // The (optional) list of parameter types (OSCUtil.INT) lets the dispatcher filter
        //  messages that do not satisfy the expected signature (=parameter list):
        dispatcher.AddListener("/PlayCard", PlayCardRpc, OSCUtil.INT);
        dispatcher.AddListener("/DrawCard", DrawCardRpc);
        dispatcher.AddListener("/ChooseSuit", ChooseSuitRpc, OSCUtil.STRING);
    }

	// ----- Handle incoming RPCs(called by dispatcher) :

	void PlayCardRpc(OSCMessageIn message, IPEndPoint remote)
    {
		int cardIndex = message.ReadInt();
        int player = PlayerFromRemote(remote);
		model.PlayerPlayedCard(player, cardIndex);
	}

	void DrawCardRpc(OSCMessageIn message, IPEndPoint remote)
	{
        int player = PlayerFromRemote(remote);
		model.PlayerDrawCard(player);
    }

	void ChooseSuitRpc(OSCMessageIn message, IPEndPoint remote)
	{
        int player = PlayerFromRemote(remote);
		string suit = message.ReadString();
		model.ChooseSuit(player, suit);
    }

    /// <summary>
    /// gets the playerID atached to the tcpnetworkconnection that has the same remote as the one put in.
    /// Used for finding which player send an Rcp
    /// </summary>
    /// <returns>PlayerID when a match is found and 0 when no match is found. (good playerID's start at 1)</returns>
    int PlayerFromRemote(IPEndPoint remote)
	{
		foreach(TcpNetworkConnection connection in playerIDs.Keys)
		{
			if (connection.Remote == remote)
				return playerIDs[connection];

        }
		return 0;

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

	/// <summary>
	/// Sends private info to the player with the attached playerID.
	/// </summary>
	/// <param name="playerID">starting from 0</param>
	void SendToPlayerID(int playerID, byte[] packet)
	{
        TcpNetworkConnection playerConnection = playerIDs.FirstOrDefault(x => x.Value == playerID + 1).Key;
        playerConnection.Send(packet);
    }

    // These RPCs are called by game model events:
	public void SetCardsInHand(int player, string cards)
	{
		OSCMessageOut message = new OSCMessageOut("/OnSetCardsInHand").AddString(cards);
        SendToPlayerID(player, message.GetBytes());
    }

	public void SetFirstCard(string card)
	{
        OSCMessageOut message = new OSCMessageOut("/OnSetFirstCard").AddString(card);
		Broadcast(message.GetBytes());
    }

	public void PlayerPlayedCard(int player, string card)
	{
		OSCMessageOut message = new OSCMessageOut("/OnPlayerPlayedCard").AddInt(player).AddString(card);
		Broadcast(message.GetBytes());
    }

	public void ChangePlayerTurn(int player)
	{
        OSCMessageOut message = new OSCMessageOut("/OnChangePlayerTurn").AddInt(player);
        Broadcast(message.GetBytes());
    }

	public void PlayerDrawCards(int player, string cards)
	{
		OSCMessageOut privateMessage = new OSCMessageOut("/SetDrawnCards").AddString(cards);
		SendToPlayerID(player - 1, privateMessage.GetBytes());

		OSCMessageOut message = new OSCMessageOut("/PlayerDrawCards").AddInt(player).AddInt(cards.Trim().Split(' ').Length);
		Broadcast(message.GetBytes());
	}

	public void PlayerChooseSuit(string suit)
	{
        OSCMessageOut message = new OSCMessageOut("/PlayerChooseSuit").AddString(suit);
		Broadcast(message.GetBytes());
    }

}
