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
    public List<string> player1cards;
    public List<string> player2cards;
    public int i = 0;
    public bool server = false;

    public override void OnStartClient()
    {
        base.OnStartClient();
        this.name = "PlayerManager";
        PlayerDeckZone = GameObject.Find("PlayerDeckZone");
        EnemyDeckZone = GameObject.Find("EnemyDeckZone");
        PlayerHand = GameObject.Find("PlayerHand");
        EnemyHand = GameObject.Find("EnemyHand");
        inputManager = GameObject.Find("InputManager");
        if (isServer)
            server = true;
        else
            server = false;
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
            {
                cardFightManager.player1_deck.Add(item);
            }
        }
        else
        {
            foreach (string item in input)
            {
                cardFightManager.player2_deck.Add(item);
            }
        }
        if (cardFightManager.player1_deck.Count > 0 && cardFightManager.player2_deck.Count > 0 && cardFightManager.cardFight == null)
        {
            RpcTargetInitialize(cardFightManager.player1_deck, cardFightManager.player2_deck, Random.Range((int)1, (int)999999));
        }
    }

    [ClientRpc]
    public void RpcAddCardToDeck(int playerID, string cardID)
    {
        CardFightManager = GameObject.Find("CardFightManager");
        CardFightManager cardFightManager = CardFightManager.GetComponent<CardFightManager>();
        if (playerID == 1)
            cardFightManager.player1_deck.Add(cardID);
        else
            cardFightManager.player2_deck.Add(cardID);
    }

    [ClientRpc]
    public void RpcTargetInitialize(List<string> player1cards, List<string> player2cards, int seed)
    {
        CardFightManager = GameObject.Find("CardFightManager");
        CardFightManager cardFightManager = CardFightManager.GetComponent<CardFightManager>();
        cardFightManager.InitializeCardFight(player1cards, player2cards, seed);
    }

    //[Command]
    //public void CmdChangeInput(int player, int input)
    //{
    //    VisualInputManager vim = inputManager.GetComponent<VisualInputManager>();
    //    if (player == 1)
    //        vim.player1_input = input;
    //    else
    //        vim.player2_input = input;
    //    Debug.Log("Player: " + player + " Input: " + input);
    //    vim.numResponses++;
    //    Debug.Log("numResponses: " + vim.numResponses.ToString());
    //    if (vim.numResponses == 2)
    //    {
    //        RpcResetReceive();
    //    }
    //}

    //[Command]
    //public void CmdChangeInputs(List<int> inputs)
    //{
    //    Debug.Log("CmdChangeInputs");
    //    VisualInputManager vim = inputManager.GetComponent<VisualInputManager>();
    //    foreach (int input in inputs)
    //        vim.inputs.Add(input);
    //    vim.numResponses = 2;
    //    RpcResetReceive();
    //}

    ////[Command]
    ////public void CmdSingleInput(int selection)
    ////{
    ////    Debug.Log("CmdSingleInput");
    ////    VisualInputManager vim = inputManager.GetComponent<VisualInputManager>();
    ////    vim.player1_input = selection;
    ////    vim.input1 = selection;
    ////    vim.numResponses = 2;
    ////    RpcResetReceive();
    ////}

    //[Command]
    //public void CmdSingleInputs(int selection1, int selection2)
    //{
    //    Debug.Log("CmdSingleInputs");
    //    VisualInputManager vim = inputManager.GetComponent<VisualInputManager>();
    //    vim.input1 = selection1;
    //    vim.input2 = selection2;
    //    vim.numResponses = 2;
    //    RpcResetReceive();
    //}

    //[Command]
    //public void CmdReady()
    //{
    //    VisualInputManager vim = inputManager.GetComponent<VisualInputManager>();
    //    vim.numResponses--;
    //    Debug.Log("numResponses: " + vim.numResponses.ToString());
    //    if (vim.numResponses == 0)
    //    {
    //        Debug.Log("ready to continue");
    //        vim.readyToContinue = true;
    //    }
    //}

    //[ClientRpc]
    //public void RpcResetReceive()
    //{
    //    VisualInputManager vim = inputManager.GetComponent<VisualInputManager>();
    //    vim.ResetReceive();
    //}

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

    [Command]
    public void CmdShuffleSeed(int playerID, int seed)
    {
        Debug.Log("CmdShuffleSeed, playerID: " + playerID + ", seed: " + seed);
        RpcReadSeed(playerID, seed);
    }

    [ClientRpc]
    public void RpcReadSeed(int playerID, int seed)
    {
        CardFightManager = GameObject.Find("CardFightManager");
        CardFightManager cardFightManager = CardFightManager.GetComponent<CardFightManager>();
        if (playerID == 1 && !isServer)
            CardFightManager.GetComponent<CardFightManager>().ReadSeed(playerID, seed);
        else if (playerID == 2 && isServer)
            CardFightManager.GetComponent<CardFightManager>().ReadSeed(playerID, seed);
    }

    [Command]
    public void CmdInputMade(bool player, Inputs input)
    {
        RpcInputMade(player, input);
    }

    [ClientRpc]
    public void RpcInputMade(bool player, Inputs input)
    {
        if (player != isServer)
        {
            inputManager.GetComponent<VisualInputManager>().inputQueue.Enqueue(input);
        }
    }
}
