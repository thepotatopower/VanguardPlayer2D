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
    public GameObject EnemyHand;
    public GameObject cardPrefab;
    public GameObject DeckZone;
    public GameObject HandCard;
    public CardBehavior cardBehavior;
    public GameObject PlayerDeckZone;
    public GameObject EnemyDeckZone;
    public List<Card> playerDeck;
    public List<Card> enemyDeck;
    public PlayerManager playerManager;
    public VisualInputManager inputManager;
    public Dictionary<string, Card> cardDict;
    public string SQLpath;
    public bool inAnimation = false;

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
        this.name = "CardFightManager";
        PlayerDeckZone = GameObject.Find("PlayerDeck");
        EnemyDeckZone = GameObject.Find("EnemyDeck");
        PlayerHand = GameObject.Find("PlayerHand");
        EnemyHand = GameObject.Find("EnemyHand");
        Field = GameObject.Find("Field");
        inputManager = GameObject.Find("InputManager").GetComponent<VisualInputManager>();
        inputManager.cardFightManager = this;
        cardDict = new Dictionary<string, Card>();
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        cardPrefab = playerManager.cardPrefab;
        SQLpath = "Data Source=" + Application.dataPath + "/../cards.db;Version=3;"; 
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
        List<Card> player1_generatedDeck = LoadCards.GenerateCardsFromList(SyncListToList(player1_deck), SQLpath);
        List<Card> player2_generatedDeck = LoadCards.GenerateCardsFromList(SyncListToList(player2_deck), SQLpath);
        Debug.Log("player1 count: " + player1_generatedDeck.Count);
        Debug.Log("player2 count: " + player2_generatedDeck.Count);
        inputManager.InitializeInputManager();
        cardFight = new VanguardEngine.CardFight();
        cardFight.Initialize(player1_generatedDeck, player2_generatedDeck, inputManager.inputManager, Application.dataPath + "/../lua");
        cardFight._player1.OnDraw += PerformDraw;
        cardFight._player2.OnDraw += PerformDraw;
        cardFight._player1.OnReturnCardFromHandToDeck += PerformHandToDeck;
        cardFight._player2.OnReturnCardFromHandToDeck += PerformHandToDeck;
        RpcInitializeDecks(cardFight._player1.PlayerDeckCount(), cardFight._player2.PlayerDeckCount());
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

    public void PerformDraw(object sender, CardEventArgs e)
    {
        foreach (Card card in e.cardList)
        {
            RpcDraw(card.id, card.tempID, e.playerID);
        }
    }

    [ClientRpc]
    public void RpcDraw(string cardID, int tempID, int playerID)
    {
        GameObject newCard = GameObject.Instantiate(cardPrefab);
        newCard.name = tempID.ToString();
        newCard.GetComponent<CardBehavior>().cardID = cardID;
        if (playerID == 1)
        {
            if (isServer)
            {
                newCard.GetComponent<CardBehavior>().faceup = true;
                newCard.GetComponent<Image>().sprite = LoadSprite(FixFileName(cardID));
                StartCoroutine(MoveFromDeckToHand(newCard, true));
            }
            else
            {
                StartCoroutine(MoveFromDeckToHand(newCard, false));
            }
        }
        else
        {
            if (isServer)
            {
                StartCoroutine(MoveFromDeckToHand(newCard, false));
            }
            else
            {
                newCard.GetComponent<CardBehavior>().faceup = true;
                newCard.GetComponent<Image>().sprite = LoadSprite(FixFileName(cardID));
                StartCoroutine(MoveFromDeckToHand(newCard, true));
            }
        }
    }

    public void PerformHandToDeck(object sender, CardEventArgs e)
    {
        RpcHandToDeck(e.card.tempID, e.playerID);
    }

    [ClientRpc]
    public void RpcHandToDeck(int tempID, int playerID)
    {
        GameObject card;
        if (playerID == 1)
        {
            if (isServer)
            {
                for (int i = 0; i < PlayerHand.transform.childCount; i++)
                {
                    card = PlayerHand.transform.GetChild(i).gameObject;
                    if (int.Parse(card.name) == tempID)
                    {
                        Debug.Log("moving back" + card.name);
                        StartCoroutine(MoveFromHandToDeck(card, true));
                        break;
                    }
                }
            }
            else
            {
                card = EnemyHand.transform.GetChild(EnemyHand.transform.childCount - 1).gameObject;
                card.transform.SetParent(Field.transform);
                StartCoroutine(MoveFromHandToDeck(card, false));
            }
        }
        else
        {
            if (isServer)
            {
                card = EnemyHand.transform.GetChild(EnemyHand.transform.childCount - 1).gameObject;
                card.transform.SetParent(Field.transform);
                StartCoroutine(MoveFromHandToDeck(card, false));
            }
            else
            {
                for (int i = 0; i < PlayerHand.transform.childCount; i++)
                {
                    card = PlayerHand.transform.GetChild(i).gameObject;
                    if (int.Parse(card.name) == tempID)
                    {
                        StartCoroutine(MoveFromHandToDeck(card, true));
                        break;
                    }
                }
            }
        }
    }

    IEnumerator MoveFromDeckToHand(GameObject card, bool player)
    {
        while (inAnimation)
            yield return null;
        inAnimation = true;
        GameObject blankCard = GameObject.Instantiate(cardPrefab);
        card.transform.SetParent(Field.transform);
        blankCard.transform.GetComponent<Image>().enabled = false;
        if (player)
        {
            blankCard.transform.SetParent(PlayerHand.transform);
            card.transform.position = PlayerDeckZone.transform.position;
            PlayerDeckZone.GetComponent<Deck>().CountChange(-1);
        }
        else
        {
            blankCard.transform.SetParent(EnemyHand.transform);
            card.transform.position = EnemyDeckZone.transform.position;
            EnemyDeckZone.GetComponent<Deck>().CountChange(-1);
        }
        float step = 5 * Time.deltaTime;
        while (Vector3.Distance(card.transform.position, blankCard.transform.position) > 0.001f)
        {
            card.transform.position = Vector3.MoveTowards(card.transform.position, blankCard.transform.position, step);
            yield return null;
        }
        GameObject.Destroy(blankCard);
        if (player)
            card.transform.SetParent(PlayerHand.transform);
        else
            card.transform.SetParent(EnemyHand.transform);
        inAnimation = false;
    }

    IEnumerator MoveFromHandToDeck(GameObject card, bool player)
    {
        GameObject target;
        GameObject newCard = GameObject.Instantiate(cardPrefab);
        newCard.transform.position = card.transform.position;
        newCard.transform.SetParent(Field.transform);
        while (inAnimation)
            yield return null;
        inAnimation = true;
        if (player)
        {
            newCard.GetComponent<Image>().sprite = card.GetComponent<Image>().sprite;
            GameObject.Destroy(card.GetComponent<CardBehavior>().selectedCard);
            target = PlayerDeckZone;
        }
        else
        {
            target = EnemyDeckZone;
        }
        GameObject.Destroy(card);
        float step = 5 * Time.deltaTime;
        while (Vector3.Distance(newCard.transform.position, target.transform.position) > 0.001f)
        {
            newCard.transform.position = Vector3.MoveTowards(newCard.transform.position, target.transform.position, step);
            yield return null;
        }
        GameObject.Destroy(newCard);
        target.GetComponent<Deck>().CountChange(1);
        inAnimation = false;
    }

    public static Sprite LoadSprite(string filename)
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

    public Card LookUpCard(string cardID)
    {
        List<Card> card;
        List<string> _cardID;
        if (cardDict.ContainsKey(cardID))
            return cardDict[name];
        else
        {
            _cardID = new List<string>();
            _cardID.Add(cardID);
            card = LoadCards.GenerateCardsFromList(_cardID, SQLpath);
            cardDict.Add(cardID, card[0]);
            return card[0];
        }
    }

    public static string FixFileName(string input)
    {
        int first = input.IndexOf('-');
        string firstHalf = input.Substring(0, first);
        string secondHalf = input.Substring(first + 1, input.Length - (first + 1));
        int second = secondHalf.IndexOf('/');
        return ("../art/" + firstHalf + secondHalf.Substring(0, second) + "_" + secondHalf.Substring(second + 1, secondHalf.Length - (second + 1)) + ".png").ToLower();
    }
}
