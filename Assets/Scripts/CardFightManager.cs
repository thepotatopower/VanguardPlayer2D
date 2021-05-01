using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VanguardEngine;
using UnityEngine.UI;
using Mirror;
using System.Threading;

public class CardFightManager : NetworkBehaviour
{
    // Start is called before the first frame update
    public VanguardEngine.CardFight cardFight = null;
    public GameObject Field;
    public GameObject PlayerHand;
    public GameObject cardPrefab;
    public GameObject DeckZone;
    public GameObject HandCard;
    public CardBehavior cardBehavior;
    public GameObject PlayerDeckZone;
    public GameObject EnemyDeckZone;
    public List<Card> playerDeck;
    public List<Card> enemyDeck;
    public PlayerManager playerManager;
    public GameObject InputManager;

    [SyncVar]
    public int counter;
    public SyncList<string> player1_deck = new SyncList<string>();
    public SyncList<string> player2_deck = new SyncList<string>();
    [SyncVar]
    public NetworkIdentity host;
    [SyncVar]
    public NetworkIdentity remote;

    public void Start()
    {
        PlayerDeckZone = GameObject.Find("PlayerDeck");
        EnemyDeckZone = GameObject.Find("EnemyDeck");
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        if (isServer)
        {
            Debug.Log("this is server");
            host = networkIdentity;
            playerManager.CmdInitialize(LoadCards.GenerateList(Application.dataPath + "/../testDeck.txt"), 1);
        }
        else
        {
            Debug.Log("this is client");
            remote = networkIdentity;
            playerManager.CmdInitialize(LoadCards.GenerateList(Application.dataPath + "/../testDeck.txt"), 2);
        }
    }

    public List<string> SyncListToList(SyncList<string> input)
    {
        List<string> output = new List<string>();
        foreach (string item in input)
            output.Add(item);
        return output;
    }

    public void InitializeCardFight()
    {
        List<Card> player1_generatedDeck = LoadCards.GenerateCardsFromList(SyncListToList(player1_deck), "Data Source=" + Application.dataPath + "/../cards.db;Version=3;");
        List<Card> player2_generatedDeck = LoadCards.GenerateCardsFromList(SyncListToList(player2_deck), "Data Source=" + Application.dataPath + "/../cards.db;Version=3;");
        Debug.Log("player1 count: " + player1_generatedDeck.Count);
        Debug.Log("player2 count: " + player2_generatedDeck.Count);
        InputManager = GameObject.Find("InputManager");
        InputManager.GetComponent<VisualInputManager>().InitializeInputManager(playerManager);
        cardFight = new VanguardEngine.CardFight();
        cardFight.Initialize(player1_generatedDeck, player2_generatedDeck, InputManager.GetComponent<VisualInputManager>().inputManager);
        RpcInitializeDecks(cardFight._player1._field.PlayerDeck.Count, cardFight._player2._field.PlayerDeck.Count);
        StartCardFight(cardFight.StartFight);
        Debug.Log("cardfight started");
    }

    public void StartCardFight(ThreadStart start)
    {
        Thread newThread = new Thread(start);
        newThread.Start();
    }

    [ClientRpc]
    public void RpcInitializeDecks(int player1_count, int player2_count)
    {
        if (isServer)
        {
            Debug.Log("server setting up decks");
            PlayerDeckZone.GetComponent<Deck>().UpdateCount(player1_count);
            EnemyDeckZone.GetComponent<Deck>().UpdateCount(player2_count);
        }
        else
        {
            Debug.Log("client setting up decks");
            PlayerDeckZone.GetComponent<Deck>().UpdateCount(player2_count);
            EnemyDeckZone.GetComponent<Deck>().UpdateCount(player1_count);
        }
    }

    public void Draw()
    {
        Card drawnCard = playerDeck[0];
        playerDeck.RemoveAt(0);
        GameObject newCard = GameObject.Instantiate(cardPrefab);
        newCard.transform.SetParent(PlayerHand.transform);
        newCard.GetComponent<Image>().sprite = LoadSprite(FixFileName(drawnCard.id));
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

    void Update()
    {
        
    }

    public string FixFileName(string input)
    {
        int first = input.IndexOf('-');
        string firstHalf = input.Substring(0, first);
        string secondHalf = input.Substring(first + 1, input.Length - (first + 1));
        int second = secondHalf.IndexOf('/');
        return ("../art/" + firstHalf + secondHalf.Substring(0, second) + "_" + secondHalf.Substring(second + 1, secondHalf.Length - (second + 1)) + ".png").ToLower();
    }
}
