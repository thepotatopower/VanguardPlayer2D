using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using VanguardEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerManager : NetworkBehaviour
{
    public GameObject Field;
    public GameObject PlayerHand;
    public GameObject EnemyHand;
    public GameObject cardPrefab;
    public GameObject DeckZone;
    public GameObject HandCard;
    public CardBehavior cardBehavior;
    public GameObject PlayerDeckZone;
    public GameObject EnemyDeckZone;
    public GameObject inputManager;
    public GameObject CardFightManager;
    public VanguardEngine.CardFight cardFight;
    public int i = 0;
    public bool server = false;
    public NetworkConnection player1;
    public NetworkConnection player2;
    public MyNetworkManager myNetworkManager;
    public LobbyManager lobbyManager;

    public override void OnStartClient()
    {
        GameObject.DontDestroyOnLoad(this);
        base.OnStartClient();
        //this.name = "PlayerManager";
        PlayerDeckZone = GameObject.Find("PlayerDeckZone");
        EnemyDeckZone = GameObject.Find("EnemyDeckZone");
        PlayerHand = GameObject.Find("PlayerHand");
        EnemyHand = GameObject.Find("EnemyHand");
        inputManager = GameObject.Find("InputManager");
        if (isServer)
            server = true;
        else
            server = false;
        myNetworkManager = GameObject.Find("Communicator").GetComponent<Communicator>().myNetworkManager;
        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        if (isLocalPlayer)
        {
            lobbyManager.playerManager = this;
            ClientSetName(this.GetComponent<NetworkIdentity>(), lobbyManager.GetName(), lobbyManager.hosting);
            lobbyManager.Proceed();
        }
    }

    [Command]
    public void ClientSetName(NetworkIdentity networkIdentity, string name, bool hosting)
    {
        if (myNetworkManager == null)
            Debug.Log("myNetworkManager is null");
        myNetworkManager.PlayerNames[networkIdentity.connectionToClient.connectionId] = name;
        if (hosting)
            myNetworkManager.Hosts.Add(networkIdentity.connectionToClient.connectionId);
    }

    [Server]
    public override void OnStartServer()
    {
        base.OnStartServer();
        myNetworkManager = GameObject.Find("Communicator").GetComponent<Communicator>().myNetworkManager;
        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
    }

    [Command]
    public void CmdInitialize(List<string> input, GameObject playerManager, int playerID)
    {
        int connectionId = playerManager.GetComponent<NetworkIdentity>().connectionToClient.connectionId;
        myNetworkManager.playerDecks[connectionId] = new List<string>(input);
        Debug.Log("player" + connectionId + "detected");
        if (myNetworkManager.PlayerPairs.ContainsKey(connectionId) && myNetworkManager.playerDecks.ContainsKey(myNetworkManager.PlayerPairs[connectionId]))
        {
            Debug.Log("both players detected");
            int seed = Random.Range((int)1, (int)999999);
            List<string> player1Deck = input;
            List<string> player2Deck = myNetworkManager.playerDecks[myNetworkManager.PlayerPairs[connectionId]];
            RpcTargetInitialize(myNetworkManager.PlayerConnectionIds[connectionId], new List<string>(player1Deck), new List<string>(player2Deck), seed, 1);
            RpcTargetInitialize(myNetworkManager.PlayerConnectionIds[myNetworkManager.PlayerPairs[connectionId]], new List<string>(player1Deck), new List<string>(player2Deck), seed, 2);
        }
    }

    [TargetRpc]
    public void RpcTargetInitialize(NetworkConnection conn, List<string> player1Deck, List<string> player2Deck, int seed, int playerID)
    {
        CardFightManager = GameObject.Find("CardFightManager");
        CardFightManager cardFightManager = CardFightManager.GetComponent<CardFightManager>();
        cardFightManager.InitializeCardFight(player1Deck, player2Deck, seed, playerID);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //[Command]
    //public void CmdShuffleSeed(int playerID, int seed)
    //{
    //    Debug.Log("CmdShuffleSeed, playerID: " + playerID + ", seed: " + seed);
    //    RpcReadSeed(playerID, seed);
    //}

    //[ClientRpc]
    //public void RpcReadSeed(int playerID, int seed)
    //{
    //    CardFightManager = GameObject.Find("CardFightManager");
    //    CardFightManager cardFightManager = CardFightManager.GetComponent<CardFightManager>();
    //    if (playerID == 1 && !isServer)
    //        CardFightManager.GetComponent<CardFightManager>().ReadSeed(playerID, seed);
    //    else if (playerID == 2 && isServer)
    //        CardFightManager.GetComponent<CardFightManager>().ReadSeed(playerID, seed);
    //}

    [Command]
    public void CmdInputMade(GameObject target, Inputs input)
    {
        int connectionId = target.GetComponent<NetworkIdentity>().connectionToClient.connectionId;
        if (myNetworkManager.PlayerPairs.ContainsKey(connectionId))
        {
            TargetInputMade(target.GetComponent<NetworkIdentity>().connectionToClient, input);
            TargetInputMade(myNetworkManager.PlayerConnectionIds[myNetworkManager.PlayerPairs[connectionId]], input);
        }
    }

    [TargetRpc]
    public void TargetInputMade(NetworkConnection conn, Inputs input)
    {
        //if (player != isServer)
        //{
        //    inputManager.GetComponent<VisualInputManager>().inputQueue.Enqueue(input);
        //}
        inputManager.GetComponent<VisualInputManager>().inputQueue.Enqueue(input);
    }

    [Command]
    public void CmdGetHosts(NetworkIdentity source)
    {
        List<string> names = new List<string>();
        List<int> ids = new List<int>();
        if (myNetworkManager == null)
            Debug.Log("MyNetworkManager null");
        if (lobbyManager == null)
            Debug.Log("LobbyManager null");
        foreach (int key in myNetworkManager.Hosts)
        {
            if (myNetworkManager.PlayerNames.ContainsKey(key))
            {
                ids.Add(key);
                names.Add(myNetworkManager.PlayerNames[key]);
            }
        }
        lobbyManager.TargetGetHosts(source.connectionToClient, names, ids);
    }

    [Command]
    public void CmdBeginFight(int hostId, NetworkIdentity client)
    {
        if (client == null)
            Debug.Log("client null");
        if (client.connectionToClient == null)
            Debug.Log("connectionToClient null");
        BeginFight(hostId, client.connectionToClient.connectionId);
    }

    [Command]
    public void CmdBeginFight(int hostId, int clientId)
    {
        BeginFight(hostId, clientId);
    }

    void BeginFight(int hostId, int clientId)
    {
        if (!myNetworkManager.PlayerConnectionIds.ContainsKey(hostId) || !myNetworkManager.PlayerConnectionIds.ContainsKey(clientId))
            return;
        myNetworkManager.Hosts.Remove(hostId);
        myNetworkManager.PlayerPairs[hostId] = clientId;
        myNetworkManager.PlayerPairs[clientId] = hostId;
        Communicator communicator = GameObject.Find("Communicator").GetComponent<Communicator>();
        Debug.Log("starting fight for player " + hostId);
        communicator.TargetBeginFight(myNetworkManager.PlayerConnectionIds[hostId]);
        Debug.Log("starting fight for player " + clientId);
        communicator.TargetBeginFight(myNetworkManager.PlayerConnectionIds[clientId]);
    }
}
