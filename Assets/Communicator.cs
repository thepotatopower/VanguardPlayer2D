using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class Communicator : NetworkBehaviour
{
    public MyNetworkManager myNetworkManager;
    public GameObject cardFightManagerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        GameObject.DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [TargetRpc]
    public void TargetDisconnectClient(NetworkConnection target)
    {
        myNetworkManager.StopClient();
    }

    [TargetRpc]
    public void TargetBeginFight(NetworkConnection target)
    {
        SceneManager.LoadScene("SampleScene");
        Debug.Log("instantiating");
        GameObject.Instantiate(cardFightManagerPrefab);
    }
}
