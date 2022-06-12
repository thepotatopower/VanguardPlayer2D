using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MyNetworkManager : NetworkManager
{
    public Communicator communicator;

    public Dictionary<int, NetworkConnection> PlayerConnectionIds = new Dictionary<int, NetworkConnection>();
    public Dictionary<int, int> PlayerPairs = new Dictionary<int, int>();
    public Dictionary<int, string> PlayerNames = new Dictionary<int, string>();
    public Dictionary<int, bool> PlayerReady = new Dictionary<int, bool>();
    public List<int> Hosts = new List<int>();

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
        PlayerReady[conn.connectionId] = false;
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        if (PlayerPairs.ContainsKey(conn.connectionId))
        {
            Debug.Log("disconnect " + PlayerConnectionIds[PlayerPairs[conn.connectionId]]);
            communicator.TargetDisconnectClient(PlayerConnectionIds[PlayerPairs[conn.connectionId]]);
            PlayerNames.Remove(PlayerPairs[conn.connectionId]);
            playerDecks.Remove(PlayerPairs[conn.connectionId]);
            PlayerReady.Remove(PlayerPairs[conn.connectionId]);
            PlayerPairs.Remove(PlayerPairs[conn.connectionId]);
            PlayerPairs.Remove(conn.connectionId);
        }
        PlayerConnectionIds.Remove(conn.connectionId);
        playerDecks.Remove(conn.connectionId);
        PlayerNames.Remove(conn.connectionId);
        Hosts.Remove(conn.connectionId);
    }
}
