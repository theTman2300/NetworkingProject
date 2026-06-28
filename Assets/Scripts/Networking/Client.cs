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

    public int playerID;

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
    void HandlePacket(byte[] packet, IPEndPoint remote)
    {
        OSCMessageIn mess = new OSCMessageIn(packet);
        Debug.Log("Message arrives on client: " + mess);
        dispatcher.HandlePacket(packet, remote);
    }

    void Update()
    {
        // Check for incoming packets, and deal with them:
        while (connection.Available()>0)
        {
            HandlePacket(connection.GetPacket(), connection.Remote);
        }
        // TODO: disconnect handling
    }

    void Initialize()
    {
        dispatcher.AddListener("/PlayerInfo", RecievePrivateInformationCommandRpc, OSCUtil.INT);
        dispatcher.AddListener("/OnSetCardsInHand", SetCardsInHandRpc, OSCUtil.STRING);
        dispatcher.AddListener("/OnSetFirstCard", SetFirstCardRpc, OSCUtil.STRING);
        dispatcher.AddListener("/OnPlayerPlayedCard", PlayerPlayedCardRpc, OSCUtil.INT, OSCUtil.STRING);
        dispatcher.AddListener("/OnChangePlayerTurn", ChangePlayerTurn, OSCUtil.INT);
        dispatcher.AddListener("/SetDrawnCards", DrawCards, OSCUtil.STRING);
        dispatcher.AddListener("/PlayerDrawCards", PlayerDrawCard, OSCUtil.INT, OSCUtil.INT);
        dispatcher.AddListener("/PlayerChooseSuit", PlayerChooseSuit, OSCUtil.STRING);

    }

    #region Incoming RPCs
    void RecievePrivateInformationCommandRpc(OSCMessageIn message, IPEndPoint remote)
    {
        int id = message.ReadInt() - 1;
        playerID = id;
        Debug.Log("Connected to server with player index " + playerID);
    }

    //only called once at start of game
    void SetCardsInHandRpc(OSCMessageIn message, IPEndPoint remote)
    {
        string[] cards = message.ReadString().Trim().Split(' ');
        StartCoroutine(localPlayer.DrawCards(cards));
        localPlayer.StartConnected();
    }

    void SetFirstCardRpc(OSCMessageIn message, IPEndPoint remote)
    {
        StartCoroutine(localPlayer.SetFirstCard(message.ReadString()));
    }

    void PlayerPlayedCardRpc(OSCMessageIn message, IPEndPoint remote)
    {
        int player = message.ReadInt();
        string card = message.ReadString();
        localPlayer.PlayerPlayedCard(player, card);
    }

    void ChangePlayerTurn(OSCMessageIn message, IPEndPoint remote)
    {
        int player = message.ReadInt();
        localPlayer.OnChangeTurn(player);
    }

    void DrawCards(OSCMessageIn message, IPEndPoint remote)
    {
        string cardsString = message.ReadString();
        string[] cards = cardsString.Trim().Split(' ');
        StartCoroutine(localPlayer.DrawCards(cards));
    }

    void PlayerDrawCard(OSCMessageIn message, IPEndPoint remote)
    {
        int player = message.ReadInt();
        int cardCount = message.ReadInt();
        StartCoroutine(localPlayer.PlayerDrawCards(player, cardCount));
    }

    void PlayerChooseSuit(OSCMessageIn message, IPEndPoint remote)
    {
        string suit = message.ReadString();
        localPlayer.PlayerChoseSuit(suit);
    }

    #endregion

    #region Outgoing RPCs
    public void PlayCard(int cardIndex)
    {
        OSCMessageOut message = new OSCMessageOut("/PlayCard").AddInt(cardIndex);
        connection.Send(message.GetBytes());
    }

    public void DrawCard()
    {
        OSCMessageOut message = new OSCMessageOut("/DrawCard");
        connection.Send(message.GetBytes());

    }

    public void ChooseJokerSuit(string suit)
    {
        OSCMessageOut message = new OSCMessageOut("/ChooseSuit").AddString(suit);
        connection.Send(message.GetBytes());
    }
    #endregion
}
