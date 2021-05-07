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
    public List<Card> player1_deck;
    public List<Card> player2_deck;
    public int i = 0;

    public override void OnStartClient()
    {
        base.OnStartClient();
        this.name = "PlayerManager";
        PlayerDeckZone = GameObject.Find("PlayerDeckZone");
        EnemyDeckZone = GameObject.Find("EnemyDeckZone");
        PlayerHand = GameObject.Find("PlayerHand");
        EnemyHand = GameObject.Find("EnemyHand");
        inputManager = GameObject.Find("InputManager");
    }

    [Server]
    public override void OnStartServer()
    {
        Debug.Log("start server");
        base.OnStartServer();
    }

    [Command]
    public void CmdInitialize(List<string> input, int player)
    {
        CardFightManager = GameObject.Find("CardFightManager");
        CardFightManager cardFightManager = CardFightManager.GetComponent<CardFightManager>();
        if (player == 1)
        {
            foreach (string item in input)
                cardFightManager.player1_deck.Add(item);
        }
        else
        {
            foreach (string item in input)
                cardFightManager.player2_deck.Add(item);
        }
        if (cardFightManager.player1_deck.Count > 0 && cardFightManager.player2_deck.Count > 0 && cardFightManager.cardFight == null)
            TargetInitialize(cardFightManager.host.connectionToClient);
    }

    [TargetRpc]
    public void TargetInitialize(NetworkConnection target)
    {
        CardFightManager = GameObject.Find("CardFightManager");
        CardFightManager cardFightManager = CardFightManager.GetComponent<CardFightManager>();
        cardFightManager.InitializeCardFight();
    }

    [Command]
    public void CmdChangeInput(int player, int input)
    {
        VisualInputManager vim = inputManager.GetComponent<VisualInputManager>();
        if (player == 1)
            vim.player1_input = input;
        else
            vim.player2_input = input;
        Debug.Log("Player: " + player + " Input: " + input);
        vim.numResponses++;
        Debug.Log("numResponses: " + vim.numResponses.ToString());
        if (vim.numResponses == 2)
        {
            RpcResetReceive();
        }
    }

    [Command]
    public void CmdChangeInputs(List<int> inputs)
    {
        VisualInputManager vim = inputManager.GetComponent<VisualInputManager>();
        foreach (int input in inputs)
            vim.inputs.Add(input);
        vim.numResponses = 2;
        RpcResetReceive();
    }

    [Command]
    public void CmdSingleInput(int selection)
    {
        VisualInputManager vim = inputManager.GetComponent<VisualInputManager>();
        vim.player1_input = selection;
        vim.numResponses = 2;
        RpcResetReceive();
    }

    [Command]
    public void CmdReady()
    {
        VisualInputManager vim = inputManager.GetComponent<VisualInputManager>();
        vim.numResponses--;
        Debug.Log("numResponses: " + vim.numResponses.ToString());
        if (vim.numResponses == 0)
        {
            Debug.Log("ready to continue");
            vim.readyToContinue = true;
        }
    }

    [ClientRpc]
    public void RpcResetReceive()
    {
        VisualInputManager vim = inputManager.GetComponent<VisualInputManager>();
        vim.ResetReceive();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Sprite LoadSprite(string filename)
    {
        if (System.IO.File.Exists(filename))
        {
            byte[] bytes = System.IO.File.ReadAllBytes(filename);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }
        else
        {
            Debug.Log(filename + "doesn't exist.");
            return null;
        }
    }

    public string FixFileName(string input)
    {
        int first = input.IndexOf('-');
        string firstHalf = input.Substring(0, first);
        string secondHalf = input.Substring(first + 1, input.Length - (first + 1));
        int second = secondHalf.IndexOf('/');
        return ("../art/" + firstHalf + secondHalf.Substring(0, second) + "_" + secondHalf.Substring(second + 1, secondHalf.Length - (second + 1)) + ".png").ToLower();
    }

    //InputManagerstuff
    [Command]
    public void CmdInitiateRPS()
    {
        RpcInitiateRPS();
    }

    [ClientRpc]
    public void RpcInitiateRPS()
    {
        inputManager.GetComponent<VisualInputManager>().GetRPSInput();
    }
}
