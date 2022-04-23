using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MyNetworkManager : NetworkManager
{
    public Communicator Communicator;

    public Dictionary<int, NetworkConnection> PlayerConnectionIds = new Dictionary<int, NetworkConnection>();
    public Dictionary<int, int> PlayerPairs = new Dictionary<int, int>();
    public Dictionary<int, string> PlayerNames = new Dictionary<int, string>();
    public List<int> Hosts = new List<int>();
    public List<int> toBeConnected = new List<int>();

    public Dictionary<int, List<string>> playerDecks = new Dictionary<int, List<string>>();

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        if (!PlayerConnectionIds.ContainsKey(conn.connectionId))
            PlayerConnectionIds[conn.connectionId] = conn;
        if (toBeConnected.Count > 0)
        {
            int player1 = toBeConnected[0];
            toBeConnected.RemoveAt(0);
            PlayerPairs[player1] = conn.connectionId;
            PlayerPairs[conn.connectionId] = player1;
        }
        else
            toBeConnected.Add(conn.connectionId);
        //if (numPlayers == 1)
        //    player.GetComponent<PlayerManager>()._playerID = 1;
        //else
        //    player.GetComponent<PlayerManager>()._playerID = 2;
        NetworkServer.AddPlayerForConnection(conn, player);
        //if (numPlayers == 1)
        //    player.GetComponent<PlayerManager>()._playerID = 1;
        //if (PlayerPairs.ContainsKey(conn.connectionId))
        //{
        //    //player.GetComponent<PlayerManager>()._playerID = 2;
        //    //Debug.Log("spawning");
        //    //GameObject newCardFightManager = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "CardFightManager"));
        //    //newCardFightManager.name = "CardFightManager";
        //    //NetworkServer.Spawn(newCardFightManager);
        //    Debug.Log("starting fight for player " + conn.connectionId);
        //    Communicator.TargetBeginFight(conn);
        //    Debug.Log("starting fight for player " + PlayerConnectionIds[PlayerPairs[conn.connectionId]].connectionId);
        //    Communicator.TargetBeginFight(PlayerConnectionIds[PlayerPairs[conn.connectionId]]);
        //}
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        if (PlayerPairs.ContainsKey(conn.connectionId))
        {
            Communicator.TargetDisconnectClient(PlayerConnectionIds[PlayerPairs[conn.connectionId]]);
            PlayerNames.Remove(PlayerPairs[conn.connectionId]);
            playerDecks.Remove(PlayerPairs[conn.connectionId]);
            PlayerPairs.Remove(PlayerPairs[conn.connectionId]);
            PlayerPairs.Remove(conn.connectionId);
        }
        PlayerConnectionIds.Remove(conn.connectionId);
        playerDecks.Remove(conn.connectionId);
        PlayerNames.Remove(conn.connectionId);
        Hosts.Remove(conn.connectionId);
    }
}
