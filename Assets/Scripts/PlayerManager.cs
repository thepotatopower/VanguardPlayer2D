using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using VanguardEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerManager : NetworkBehaviour
{
    public int i = 0;
    public bool server = false;
    public MyNetworkManager myNetworkManager;
    public LobbyManager lobbyManager;
    public PlayerManager playerManagerPrefab;

    public override void OnStartClient()
    {
        GameObject.DontDestroyOnLoad(this);
        base.OnStartClient();
        this.name = "Player" + NetworkClient.connection.connectionId;
        if (isServer)
            server = true;
        else
            server = false;
        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        myNetworkManager = GameObject.Find("UnityNetworkServer").GetComponent<MyNetworkManager>();
        myNetworkManager.communicator.gameObject.SetActive(true);
        if (isLocalPlayer)
        {
            myNetworkManager.communicator.playerManager = this;
            lobbyManager.playerManager = this;
            if (!lobbyManager.connected)
            {
                lobbyManager.playerManager = this;
                if (lobbyManager.hosting)
                    Debug.Log("is hosting");
                ClientSetName(this.GetComponent<NetworkIdentity>(), lobbyManager.GetName(), lobbyManager.hosting);
                lobbyManager.Proceed();
            }
        }
        //SceneManager.activeSceneChanged += ActiveSceneChanged;
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
        GameObject.DontDestroyOnLoad(this);
        base.OnStartServer();
        myNetworkManager = GameObject.Find("UnityNetworkServer").GetComponent<MyNetworkManager>();
        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
    }

    [Command]
    public void CmdInitialize(List<string> input, GameObject playerManager)
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
            Debug.Log(connectionId);
            Debug.Log(myNetworkManager.PlayerConnectionIds[connectionId].connectionId);
            Debug.Log(myNetworkManager.PlayerPairs[connectionId]);
            Debug.Log(myNetworkManager.PlayerConnectionIds[myNetworkManager.PlayerPairs[connectionId]].connectionId);
            IEnumerator Dialog()
            {
                while (true)
                {
                    if (myNetworkManager.PlayerConnectionIds[connectionId].isReady && myNetworkManager.PlayerConnectionIds[myNetworkManager.PlayerPairs[connectionId]].isReady)
                    {
                        RpcTargetInitialize(myNetworkManager.PlayerConnectionIds[connectionId], new List<string>(player1Deck), new List<string>(player2Deck), seed, 1);
                        RpcTargetInitialize(myNetworkManager.PlayerConnectionIds[myNetworkManager.PlayerPairs[connectionId]], new List<string>(player1Deck), new List<string>(player2Deck), seed, 2);
                        yield break;
                    }
                    else
                        yield return null;
                }
            }
            StartCoroutine(Dialog());
        }
    }

    [TargetRpc]
    public void RpcTargetInitialize(NetworkConnection conn, List<string> player1Deck, List<string> player2Deck, int seed, int playerID)
    {
        Debug.Log("RpcTargetInitialize");
        CardFightManager cardFightManager = GameObject.Find("CardFightManager").GetComponent<CardFightManager>();
        cardFightManager.InitializeCardFight(player1Deck, player2Deck, seed, playerID, "");
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
            Debug.Log("sending to: " + target.GetComponent<NetworkIdentity>().connectionToClient.connectionId);
            TargetInputMade(target.GetComponent<NetworkIdentity>().connectionToClient, input);
            Debug.Log("sending to: " + myNetworkManager.PlayerConnectionIds[myNetworkManager.PlayerPairs[connectionId]].connectionId);
            TargetInputMade(myNetworkManager.PlayerConnectionIds[myNetworkManager.PlayerPairs[connectionId]], input);
        }
    }

    [TargetRpc]
    public void TargetInputMade(NetworkConnection conn, Inputs input)
    {
        Debug.Log("received input");
        GameObject inputManager = GameObject.Find("InputManager");
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
        TargetGetHosts(source.connectionToClient, names, ids);
    }

    [TargetRpc]
    public void TargetGetHosts(NetworkConnection conn, List<string> names, List<int> ids)
    {
        GameObject.Find("LobbyManager").GetComponent<LobbyManager>().GetHosts(names, ids);
    }

    [Command] 
    public void CmdConnectToHost(int hostId, GameObject source)
    {
        ConnectToHost(hostId, source.GetComponent<NetworkIdentity>().connectionToClient.connectionId);
    }

    [Command]
    public void CmdBeginFight(int hostId, GameObject source)
    {
        if (source.GetComponent<NetworkIdentity>() == null)
            Debug.Log("client null");
        if (source.GetComponent<NetworkIdentity>().connectionToClient == null)
            Debug.Log("connectionToClient null");
        BeginFight(hostId, source.GetComponent<NetworkIdentity>().connectionToClient.connectionId);
    }

    [Command]
    public void CmdBeginFight(int hostId, int clientId)
    {
        BeginFight(hostId, clientId);
    }

    [Server]
    void BeginFight(int hostId, int clientId)
    {
        if (!myNetworkManager.PlayerConnectionIds.ContainsKey(hostId) || !myNetworkManager.PlayerConnectionIds.ContainsKey(clientId))
            return;
        myNetworkManager.Hosts.Remove(hostId);
        myNetworkManager.PlayerPairs[hostId] = clientId;
        myNetworkManager.PlayerPairs[clientId] = hostId;
        NetworkServer.SetClientNotReady(myNetworkManager.PlayerConnectionIds[hostId] as NetworkConnectionToClient);
        NetworkServer.SetClientNotReady(myNetworkManager.PlayerConnectionIds[clientId] as NetworkConnectionToClient);
        Debug.Log("starting fight for player " + hostId);
        TargetBeginFight(myNetworkManager.PlayerConnectionIds[hostId]);
        Debug.Log("starting fight for player " + clientId);
        TargetBeginFight(myNetworkManager.PlayerConnectionIds[clientId]);
    }

    public void ActiveSceneChanged(Scene current, Scene next)
    {
        Debug.Log(current.name + " " + next.name);
        if (isLocalPlayer)
        {
            if (next.name == "Fight")
            {
                Debug.Log("NetworkClient Ready");
                if (NetworkClient.Ready())
                    Debug.Log("successful");
            }
        }
    }

    [Server]
    void ConnectToHost(int hostId, int clientId)
    {
        if (!myNetworkManager.PlayerConnectionIds.ContainsKey(hostId) || !myNetworkManager.PlayerConnectionIds.ContainsKey(clientId))
            return;
        myNetworkManager.Hosts.Remove(hostId);
        myNetworkManager.PlayerPairs[hostId] = clientId;
        myNetworkManager.PlayerPairs[clientId] = hostId;
        Debug.Log("starting room for player " + hostId);
        TargetConnectToHost(myNetworkManager.PlayerConnectionIds[hostId], hostId, clientId);
        Debug.Log("starting room for player " + clientId);
        TargetConnectToHost(myNetworkManager.PlayerConnectionIds[clientId], hostId, clientId);
    }

    [TargetRpc]
    public void TargetBeginFight(NetworkConnection target)
    {
        GameObject communicator = myNetworkManager.communicator.gameObject;
        communicator.SetActive(true);
        IEnumerator Dialog()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Fight");
            while (!asyncOperation.isDone)
                yield return null;
            //SceneManager.MoveGameObjectToScene(myNetworkManager.gameObject, SceneManager.GetSceneByName("Fight"));
            SceneManager.MoveGameObjectToScene(communicator, SceneManager.GetSceneByName("Fight"));
            //SceneManager.MoveGameObjectToScene(lobbyManager.gameObject, SceneManager.GetSceneByName("Fight"));
            SceneManager.MoveGameObjectToScene(NetworkClient.connection.identity.GetComponent<PlayerManager>().gameObject, SceneManager.GetSceneByName("Fight"));
            NetworkClient.Ready();
            Globals.Instance.cardFightManager = GameObject.Instantiate(Globals.Instance.cardFightManagerPrefab).GetComponent<CardFightManager>();
            Globals.Instance.cardFightManager.Initialize(NetworkClient.connection.identity.GetComponent<PlayerManager>());
            yield return null;
        }
        StartCoroutine(Dialog());
    }

    [TargetRpc] 
    public void TargetConnectToHost(NetworkConnection conn, int hostId, int clientId)
    {
        lobbyManager.EstablishRoom(hostId, clientId);
    }

    [Command]
    public void CmdReplacePlayer(GameObject source)
    {
        GameObject oldPlayer = source.GetComponent<NetworkIdentity>().connectionToClient.identity.gameObject;
        //int connectionId = source.GetComponent<NetworkIdentity>().connectionToClient.connectionId;
        //NetworkConnectionToClient connection = myNetworkManager.PlayerConnectionIds[myNetworkManager.PlayerPairs[connectionId]] as NetworkConnectionToClient;
        //NetworkServer.ReplacePlayerForConnection(connection, Instantiate(playerManagerPrefab.gameObject), true);
        NetworkServer.Destroy(oldPlayer);
        NetworkServer.ReplacePlayerForConnection(source.GetComponent<NetworkIdentity>().connectionToClient, Instantiate(playerManagerPrefab.gameObject), true);
        //NetworkServer.Destroy(oldPlayer);
    }

    [Command]
    public void CmdReady(GameObject source)
    {
        int connectionId = source.GetComponent<NetworkIdentity>().connectionToClient.connectionId;
        myNetworkManager.PlayerReady[connectionId] = true;
        Debug.Log(connectionId + "ready");
        if (myNetworkManager.PlayerReady[myNetworkManager.PlayerPairs[connectionId]])
        {
            Debug.Log("both ready");
            TargetRpcReady(myNetworkManager.PlayerConnectionIds[connectionId]);
            TargetRpcReady(myNetworkManager.PlayerConnectionIds[myNetworkManager.PlayerPairs[connectionId]]);
        }
    }

    [TargetRpc]
    public void TargetRpcReady(NetworkConnection conn)
    {
        Debug.Log("ready to initialize cardfight");
        Globals.Instance.cardFightManager = GameObject.Instantiate(Globals.Instance.cardFightManagerPrefab).GetComponent<CardFightManager>();
    }

    [Command]
    public void CmdSurrender()
    {
        if (myNetworkManager.PlayerPairs.ContainsKey(this.GetComponent<NetworkIdentity>().connectionToClient.connectionId))
        {
            TargetSurrender(this.GetComponent<NetworkIdentity>().connectionToClient, true);
            TargetSurrender(myNetworkManager.PlayerConnectionIds[myNetworkManager.PlayerPairs[this.GetComponent<NetworkIdentity>().connectionToClient.connectionId]], false);
        }
    }

    [TargetRpc]
    public void TargetSurrender(NetworkConnection conn, bool loser)
    {
        GameObject cardFightManager = GameObject.Find("CardFightManager");
        cardFightManager.GetComponent<CardFightManager>().OnGameOver(loser);
    }

    [TargetRpc]
    public void TargetDisconnectClient(NetworkConnection target)
    {
        myNetworkManager.StopClient();
    }
}
