using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyItem : MonoBehaviour
{
    public LobbyManager lobbyManager;
    public GameObject LobbyItemPanel;
    public Text text;
    public bool selected = false;
    public int connectionId = -1;
    
    // Start is called before the first frame update
    void Start()
    {
        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseEnter()
    {
        LobbyItemPanel.GetComponent<Image>().color = Color.cyan;
    }

    public void OnMouseExit()
    {
        if (selected)
            LobbyItemPanel.GetComponent<Image>().color = Color.green;
        else
            LobbyItemPanel.GetComponent<Image>().color = Color.white;
    }

    public void OnClick()
    {
        lobbyManager.LobbyItemClicked(this);
    }

    public void Select()
    {
        selected = true;
        LobbyItemPanel.GetComponent<Image>().color = Color.green;
    }

    public void Deselect()
    {
        selected = false;
        LobbyItemPanel.GetComponent<Image>().color = Color.white;
    }
}
