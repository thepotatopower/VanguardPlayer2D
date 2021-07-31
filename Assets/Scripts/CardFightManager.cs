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
    public GameObject PlayerDropZone;
    public GameObject EnemyDropZone;
    public GameObject HandCard;
    public CardBehavior cardBehavior;
    public GameObject PlayerDeckZone;
    public GameObject PlayerRideDeckZone;
    public GameObject EnemyDeckZone;
    public GameObject EnemyRideDeckZone;
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
        PlayerRideDeckZone = GameObject.Find("PlayerRideDeck");
        PlayerDropZone = GameObject.Find("PlayerDropZone");
        EnemyDropZone = GameObject.Find("EnemyDropZone");
        EnemyDeckZone = GameObject.Find("EnemyDeck");
        EnemyRideDeckZone = GameObject.Find("EnemyRideDeck");
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
        cardFight._player1.OnRideFromRideDeck += PerformRideFromRideDeck;
        cardFight._player2.OnRideFromRideDeck += PerformRideFromRideDeck;
        cardFight._player1.OnDiscard += PerformDiscard;
        cardFight._player2.OnDiscard += PerformDiscard;
        cardFight._player1.OnStandUpVanguard += PerformStandUpVanguard;
        //cardFight._player2.OnStandUpVanguard += PerformStandUpVanguard;
        RpcInitializeDecks(cardFight._player1.PlayerDeckCount(), cardFight._player2.PlayerDeckCount());
        RpcPlaceStarter(cardFight._player1.Vanguard().id, cardFight._player1.Vanguard().tempID, cardFight._player2.Vanguard().id, cardFight._player2.Vanguard().tempID);
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
            PlayerRideDeckZone.GetComponent<Deck>().UpdateCount(3);
            EnemyRideDeckZone.GetComponent<Deck>().UpdateCount(3);
        }
        else
        {
            Debug.Log("client setting up decks");
            PlayerDeckZone.GetComponent<Deck>().UpdateCount(player2_count);
            EnemyDeckZone.GetComponent<Deck>().UpdateCount(player1_count);
            PlayerRideDeckZone.GetComponent<Deck>().UpdateCount(3);
            EnemyRideDeckZone.GetComponent<Deck>().UpdateCount(3);
        }
        PlayerDropZone.GetComponent<Deck>().UpdateCount(0);
        EnemyDropZone.GetComponent<Deck>().UpdateCount(0);
    }

    [ClientRpc]
    public void RpcPlaceStarter(string cardID1, int tempID1, string cardID2, int tempID2)
    {
        Card card1 = LookUpCard(cardID1);
        Card card2 = LookUpCard(cardID2);
        GameObject Card1 = GameObject.Instantiate(cardPrefab);
        GameObject Card2 = GameObject.Instantiate(cardPrefab);
        Card1.name = tempID1.ToString();
        Card2.name = tempID2.ToString();
        if (isServer)
        {
            inputManager.PlayerVG.GetComponent<UnitSlotBehavior>().AddCard(card1.grade, 0, card1.critical, card1.power, true, false, cardID1, Card1);
            Card2.transform.Rotate(new Vector3(180, 0, 0));
            inputManager.EnemyVG.GetComponent<UnitSlotBehavior>().AddCard(card2.grade, 0, card2.critical, card2.power, true, false, cardID2, Card2);
        }
        else
        {
            inputManager.PlayerVG.GetComponent<UnitSlotBehavior>().AddCard(card2.grade, 0, card2.critical, card2.power, true, false, cardID2, Card2);
            Card1.transform.Rotate(new Vector3(180, 0, 0));
            inputManager.EnemyVG.GetComponent<UnitSlotBehavior>().AddCard(card1.grade, 0, card1.critical, card1.power, true, false, cardID1, Card1);
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
        float step = 2000 * Time.deltaTime;
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
        while (inAnimation)
            yield return null;
        inAnimation = true;
        inputManager.cardsAreHoverable = false;
        GameObject target;
        GameObject newCard = GameObject.Instantiate(cardPrefab);
        newCard.transform.position = card.transform.position;
        newCard.transform.SetParent(Field.transform);
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

        card.transform.SetParent(null);
        GameObject.Destroy(card);
        float step = 2000 * Time.deltaTime;
        while (Vector3.Distance(newCard.transform.position, target.transform.position) > 0.001f)
        {
            newCard.transform.position = Vector3.MoveTowards(newCard.transform.position, target.transform.position, step);
            yield return null;
        }
        GameObject.Destroy(newCard);
        target.GetComponent<Deck>().CountChange(1);
        inAnimation = false;
        inputManager.cardsAreHoverable = true;
    }

    public void PerformStandUpVanguard(object sender, CardEventArgs e)
    {
        RpcStandUpVanguard();
    }

    [ClientRpc]
    public void RpcStandUpVanguard()
    {
        StartCoroutine(WaitForFlip());
    }

    IEnumerator WaitForFlip()
    {
        while (inAnimation)
            yield return null;
        inAnimation = true;
        StartCoroutine(inputManager.PlayerVG.GetComponent<UnitSlotBehavior>().Flip());
        StartCoroutine(inputManager.EnemyVG.GetComponent<UnitSlotBehavior>().Flip());
        while (inputManager.PlayerVG.GetComponent<UnitSlotBehavior>().inAnimation)
            yield return null;
        inAnimation = false;
    }

    public void PerformRideFromRideDeck(object sender, CardEventArgs e)
    {
        Debug.Log("Riding " + e.card.name);
        RpcRideFromRideDeck(e.playerID, e.card.id);
    }

    [ClientRpc]
    public void RpcRideFromRideDeck(int playerID, string cardID)
    {
        bool player;
        GameObject RideDeck;
        GameObject VG;
        GameObject newVG = GameObject.Instantiate(cardPrefab);
        newVG.GetComponent<CardBehavior>().faceup = true;
        newVG.GetComponent<Image>().sprite = LoadSprite(FixFileName(cardID));
        newVG.GetComponent<CardBehavior>().card = LookUpCard(cardID);
        if (isPlayerAction(playerID))
        {
            player = true;
            RideDeck = PlayerRideDeckZone;
            VG = inputManager.PlayerVG;
        }
        else
        {
            player = false;
            RideDeck = EnemyRideDeckZone;
            VG = inputManager.EnemyVG;
            newVG.transform.Rotate(new Vector3(180, 0, 0));
        }
        StartCoroutine(MoveFromRideDeckToVG(newVG, RideDeck, VG));
    }

    IEnumerator MoveFromRideDeckToVG(GameObject newVG, GameObject RideDeck, GameObject VG)
    {
        while (inAnimation)
            yield return null;
        inAnimation = true;
        newVG.transform.SetParent(Field.transform);
        newVG.transform.position = RideDeck.transform.position;
        float step = 2000 * Time.deltaTime;
        while (Vector3.Distance(newVG.transform.position, VG.transform.position) > 0.01f)
        {
            newVG.transform.position = Vector3.MoveTowards(newVG.transform.position, VG.transform.position, step);
            yield return null;
        }
        Card card = newVG.GetComponent<CardBehavior>().card;
        VG.GetComponent<UnitSlotBehavior>().AddCard(card.grade, VG.GetComponent<UnitSlotBehavior>()._soul + 1, card.critical, card.power, true, true, card.id, newVG);
        RideDeck.GetComponent<Deck>().CountChange(-1);
        inAnimation = false;
    }

    public void PerformDiscard(object sender, CardEventArgs e)
    {
        foreach (Card card in e.cardList)
        {
            RpcDiscard(card.tempID, card.id, e.playerID);
        }
    }

    [ClientRpc]
    public void RpcDiscard(int tempID, string cardID, int playerID)
    {
        bool player;
        GameObject target;
        if (isPlayerAction(playerID))
        {
            player = true;
            target = PlayerHand;
        }
        else
        {
            player = false;
            target = EnemyHand;
        }
        for (int i = 0; i < target.transform.childCount; i++)
        {
            if (int.Parse(target.transform.GetChild(i).name) == tempID)
            {
                StartCoroutine(MoveFromHandToDrop(target.transform.GetChild(i).gameObject, cardID, player));
            }
        }
    }

    IEnumerator MoveFromHandToDrop(GameObject card, string cardID, bool player)
    {
        while (inAnimation)
            yield return null;
        inAnimation = true;
        inputManager.cardsAreHoverable = false;
        GameObject target;
        GameObject newCard = GameObject.Instantiate(cardPrefab);
        newCard.transform.position = card.transform.position;
        newCard.transform.SetParent(Field.transform);
        if (player)
        {
            newCard.GetComponent<Image>().sprite = card.GetComponent<Image>().sprite;
            GameObject.Destroy(card.GetComponent<CardBehavior>().selectedCard);
            target = PlayerDropZone;
        }
        else
        {
            target = EnemyDropZone;
        }
        card.transform.SetParent(null);
        GameObject.Destroy(card);
        float step = 2000 * Time.deltaTime;
        while (Vector3.Distance(newCard.transform.position, target.transform.position) > 0.001f)
        {
            newCard.transform.position = Vector3.MoveTowards(newCard.transform.position, target.transform.position, step);
            yield return null;
        }
        GameObject.Destroy(newCard);
        target.GetComponent<Deck>().ChangeSprite(LoadSprite(FixFileName(cardID)));
        target.GetComponent<Deck>().CountChange(1);
        inAnimation = false;
        inputManager.cardsAreHoverable = true;
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
        Debug.Log("looking up " + cardID + "...");
        if (cardDict.ContainsKey(cardID))
            return cardDict[cardID];
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

    public void DisplayCard(string cardID)
    {
        GameObject ZoomIn = GameObject.Find("ZoomIn");
        Text CardName = GameObject.Find("CardName").GetComponent<Text>();
        Text CardEffect = GameObject.Find("CardEffect").GetComponent<Text>();
        ZoomIn.GetComponent<Image>().sprite = LoadSprite(FixFileName(cardID));
        Card card = LookUpCard(cardID);
        CardName.text = card.name;
        string effect = "[Power: " + card.power + "] [Shield: " + card.shield + "] [Grade: " + card.grade + "]\n" + card.effect;
        CardEffect.text = effect;
    }

    public bool isPlayerAction(int playerID)
    {
        if (isServer)
        {
            if (playerID == 1)
                return true;
            return false;
        }
        else
        {
            if (playerID == 2)
                return true;
            return false;
        }
    }
}
