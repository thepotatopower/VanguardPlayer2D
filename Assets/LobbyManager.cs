using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.Networking;
using Mirror;

public class LobbyManager : NetworkBehaviour
{
    public string PlayerName = "Anonymous";
    public InputField inputField;
    public Text input;
    public bool hosting = false;
    public MyNetworkManager networkManager;
    public Communicator communicator;
    public ClientStartUp clientStartUp;
    public Dictionary<string, int> Hosts = new Dictionary<string, int>();
    public List<LobbyItem> lobbyItems = new List<LobbyItem>();
    public LobbyItem selectedLobbyItem = null;
    public LobbyItem lobbyItemPrefab;
    public Button connectToHost;
    public Button hostButton;
    public Button joinButton;
    public GameObject hostList;
    public GameObject scrollList;
    public PlayerManager playerManager;

    // Start is called before the first frame update
    void Start()
    {
        GameObject.DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Host()
    {
        hosting = true;
        clientStartUp.OnLoginUserButtonClick();
    }

    public void Join()
    {
        hosting = false;
        clientStartUp.OnLoginUserButtonClick();
    }

    public void Proceed()
    {
        inputField.gameObject.SetActive(false);
        hostButton.gameObject.SetActive(false);
        joinButton.gameObject.SetActive(false);
        if (!hosting)
        {
            playerManager.CmdGetHosts(playerManager.GetComponent<NetworkIdentity>());
            scrollList.gameObject.SetActive(true);
            connectToHost.gameObject.SetActive(true);
        }
    }

    public string GetName()
    {
        if (input.text == "")
            return "Anonymous";
        return input.text;
    }

    [TargetRpc]
    public void TargetGetHosts(NetworkConnection conn, List<string> names, List<int> ids)
    {
        ResetLobbyItems();
        for (int i = 0; i < names.Count; i++)
            Hosts[names[i]] = ids[i];
        foreach (string key in Hosts.Keys)
        {
            LobbyItem newLobbyItem = GameObject.Instantiate(lobbyItemPrefab);
            newLobbyItem.name = key;
            newLobbyItem.connectionId = Hosts[key];
            newLobbyItem.text.text = newLobbyItem.name;
            newLobbyItem.transform.SetParent(hostList.transform);
            lobbyItems.Add(newLobbyItem);
        }
    }

    public void ResetLobbyItems()
    {
        Hosts.Clear();
        while (hostList.transform.childCount > 0)
        {
            GameObject toDestroy = hostList.transform.GetChild(0).gameObject;
            toDestroy.transform.SetParent(null);
            GameObject.Destroy(toDestroy);
        }
        lobbyItems.Clear();
        selectedLobbyItem = null;
    }

    public void LobbyItemClicked(LobbyItem clickedItem)
    {
        if (!clickedItem.selected)
        {
            selectedLobbyItem = clickedItem;
            clickedItem.Select();
            connectToHost.interactable = true;
        }
        else
        {
            selectedLobbyItem = null;
            clickedItem.Deselect();
            connectToHost.interactable = false;
        }
    }

    public void ConnectToHostClicked()
    {
        if (selectedLobbyItem != null)
        {
            playerManager.CmdBeginFight(selectedLobbyItem.connectionId, this.GetComponent<NetworkIdentity>());
        }
    }
}
