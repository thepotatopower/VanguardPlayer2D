using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class Communicator : NetworkBehaviour
{
    public PlayerManager playerManager;
    public MyNetworkManager myNetworkManager;

    // Start is called before the first frame update
    void Start()
    {
        GameObject.DontDestroyOnLoad(this);
        this.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        this.enabled = true;
    }

    [TargetRpc]
    public void TargetDisconnectClient(NetworkConnection target)
    {
        myNetworkManager.StopClient();
    }
}
