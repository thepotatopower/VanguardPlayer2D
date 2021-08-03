using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VanguardEngine;
using UnityEngine.UI;
using Mirror;
using System.Threading;
using System;

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
        PlayerDropZone.GetComponent<Pile>().SetAddToTop();
        EnemyDropZone = GameObject.Find("EnemyDropZone");
        EnemyDropZone.GetComponent<Pile>().SetAddToTop();
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
            playerManager.CmdInitialize(LoadCards.GenerateList(Application.dataPath + "/../dsd01.txt", LoadCode.WithRideDeck), 1);
        }
        else
        {
            Debug.Log("this is client");
            remote = networkIdentity;
            playerManager.CmdInitialize(LoadCards.GenerateList(Application.dataPath + "/../dsd01.txt", LoadCode.WithRideDeck), 2);
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
        List<Card> tokens = LoadCards.GenerateCardsFromList(LoadCards.GenerateList(Application.dataPath + "/../tokens.txt", LoadCode.Tokens), SQLpath);
        Debug.Log("player1 count: " + player1_generatedDeck.Count);
        Debug.Log("player2 count: " + player2_generatedDeck.Count);
        inputManager.InitializeInputManager();
        cardFight = new VanguardEngine.CardFight();
        cardFight.Initialize(player1_generatedDeck, player2_generatedDeck, tokens, inputManager.inputManager, Application.dataPath + "/../lua");
        //cardFight._player1.OnRideFromRideDeck += PerformRideFromRideDeck;
        //cardFight._player2.OnRideFromRideDeck += PerformRideFromRideDeck;
        cardFight._player1.OnStandUpVanguard += PerformStandUpVanguard;
        cardFight._player1.OnZoneChanged += ChangeZone;
        RpcInitializeDecks(cardFight._player1.GetDeck().Count, cardFight._player2.GetDeck().Count);
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
            PlayerDeckZone.GetComponent<Pile>().UpdateCount(player1_count);
            EnemyDeckZone.GetComponent<Pile>().UpdateCount(player2_count);
            PlayerRideDeckZone.GetComponent<Pile>().UpdateCount(3);
            EnemyRideDeckZone.GetComponent<Pile>().UpdateCount(3);
        }
        else
        {
            Debug.Log("client setting up decks");
            PlayerDeckZone.GetComponent<Pile>().UpdateCount(player2_count);
            EnemyDeckZone.GetComponent<Pile>().UpdateCount(player1_count);
            PlayerRideDeckZone.GetComponent<Pile>().UpdateCount(3);
            EnemyRideDeckZone.GetComponent<Pile>().UpdateCount(3);
        }
        PlayerDropZone.GetComponent<Pile>().UpdateCount(0);
        EnemyDropZone.GetComponent<Pile>().UpdateCount(0);
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

    public void ChangeZone(object sender, CardEventArgs e)
    {
        RpcChangeZone(e.previousLocation.Item1, e.previousLocation.Item2, e.currentLocation.Item1, e.currentLocation.Item2, e.card);
    }

    [ClientRpc]
    public void RpcChangeZone(int previousLocation, int previousFL, int currentLocation, int currentFL, Card card)
    {
        StartCoroutine(ChangeZoneRoutine(previousLocation, previousFL, currentLocation, currentFL, card));
    }

    IEnumerator ChangeZoneRoutine(int previousLocation, int previousFL, int currentLocation, int currentFL, Card card)
    {
        while (inAnimation)
            yield return null;
        inAnimation = true;
        GameObject previousZone = null;
        GameObject currentZone = null;
        GameObject zone = null;
        GameObject newCard = null;
        int location = previousLocation;
        for (int i = 0; i < 2; i++)
        {
            if (i == 1)
                location = currentLocation;
            Debug.Log(location);
            if (location == Location.Deck)
            {
                if (isServer)
                {
                    if (card.originalOwner == 1)
                        zone = PlayerDeckZone;
                    else
                        zone = EnemyDeckZone;
                }
                else
                {
                    if (card.originalOwner == 1)
                        zone = EnemyDeckZone;
                    else
                        zone = PlayerDeckZone;
                }
            }
            else if (location == Location.RideDeck)
            {
                Debug.Log("ride deck here");
                if (isServer)
                {
                    if (card.originalOwner == 1)
                        zone = PlayerRideDeckZone;
                    else
                        zone = EnemyRideDeckZone;
                }
                else
                {
                    if (card.originalOwner == 1)
                        zone = EnemyRideDeckZone;
                    else
                        zone = PlayerRideDeckZone;
                }
            }
            else if (location == Location.Hand)
            {
                if (isServer)
                {
                    if (card.originalOwner == 1)
                        zone = PlayerHand;
                    else
                        zone = EnemyHand;
                }
                else
                {
                    if (card.originalOwner == 1)
                        zone = EnemyHand;
                    else
                        zone = PlayerHand;
                }
            }
            else if (location == Location.Drop)
            {
                if (isServer)
                {
                    if (card.originalOwner == 1)
                        zone = PlayerDropZone;
                    else
                        zone = EnemyDropZone;
                }
                else
                {
                    if (card.originalOwner == 1)
                        zone = EnemyDropZone;
                    else
                        zone = PlayerDropZone;
                }
            }
            else if (location == Location.VC)
            {
                Debug.Log("vc here");
                if (isServer)
                {
                    if (card.originalOwner == 1)
                        zone = inputManager.PlayerVG;
                    else
                        zone = inputManager.EnemyVG;
                }
                else
                {
                    if (card.originalOwner == 1)
                        zone = inputManager.EnemyVG;
                    else
                        zone = inputManager.PlayerVG;
                }
            }
            else
                yield break;
            if (i < 1)
                previousZone = zone;
            else
                currentZone = zone;
        }
        inputManager.cardsAreHoverable = false;


        if (previousZone == PlayerHand)
        {
            for (int i = 0; i < PlayerHand.transform.childCount; i++)
            {
                newCard = PlayerHand.transform.GetChild(i).gameObject;
                if (int.Parse(newCard.name) == card.tempID)
                {
                    //GameObject.Destroy(newCard.GetComponent<CardBehavior>().selectedCard);
                    break;
                }
            }
        }
        else if (previousZone == EnemyHand)
        {
            newCard = EnemyHand.transform.GetChild(EnemyHand.transform.childCount - 1).gameObject;
            newCard.transform.SetParent(Field.transform);
        }
        else
        {
            newCard = CreateNewCard(card.id, card.tempID);
            newCard.transform.SetParent(Field.transform);
            newCard.transform.position = previousZone.transform.position;
        }

        
        if (currentZone == EnemyDeckZone || currentZone == EnemyHand)
            newCard.GetComponent<Image>().sprite = LoadSprite(Application.dataPath + "/../cardart/FaceDownCard.jpg");
        else if (currentZone == inputManager.PlayerVG || currentZone == inputManager.EnemyVG)
        {
            newCard.GetComponent<CardBehavior>().faceup = true;
            newCard.GetComponent<Image>().sprite = LoadSprite(FixFileName(card.id));
            newCard.GetComponent<CardBehavior>().card = LookUpCard(card.id);
            if (currentZone == inputManager.EnemyVG)
                newCard.transform.Rotate(new Vector3(180, 0, 0));
        }
        GameObject blankCard = GameObject.Instantiate(cardPrefab);
        blankCard.transform.GetComponent<Image>().enabled = false;


        if (currentZone == PlayerHand || currentZone == EnemyHand)
            blankCard.transform.SetParent(currentZone.transform);
        else
        {
            blankCard.transform.SetParent(Field.transform);
            blankCard.transform.position = currentZone.transform.position;
        }


        if (previousZone.GetComponent<Pile>() != null)
            previousZone.GetComponent<Pile>().RemoveCard(card);


        float step = 2000 * Time.deltaTime;
        while (Vector3.Distance(newCard.transform.position, blankCard.transform.position) > 0.001f)
        {
            newCard.transform.position = Vector3.MoveTowards(newCard.transform.position, blankCard.transform.position, step);
            yield return null;
        }
        GameObject.Destroy(blankCard);


        if (currentZone == PlayerHand || currentZone == EnemyHand)
            newCard.transform.SetParent(currentZone.transform);
        else if (currentZone == inputManager.PlayerVG || currentZone == inputManager.EnemyVG)
        {
            currentZone.GetComponent<UnitSlotBehavior>().AddCard(card.grade, currentZone.GetComponent<UnitSlotBehavior>()._soul + 1, card.critical, card.power, true, true, card.id, newCard);
        }
        else
        {
            newCard.transform.SetParent(null);
            GameObject.Destroy(newCard);
        }


        if (currentZone.GetComponent<Pile>() != null)
            currentZone.GetComponent<Pile>().AddCard(card, LoadSprite(FixFileName(card.id)));

        inputManager.cardsAreHoverable = true;
        inAnimation = false;
    }

    public GameObject CreateNewCard(string cardID, int tempID)
    {
        GameObject newCard = GameObject.Instantiate(cardPrefab);
        newCard.name = tempID.ToString();
        newCard.GetComponent<CardBehavior>().cardID = cardID;
        newCard.GetComponent<CardBehavior>().faceup = true;
        newCard.GetComponent<Image>().sprite = LoadSprite(FixFileName(cardID));
        return newCard;
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
        RideDeck.GetComponent<Pile>().CountChange(-1);
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
        return (Application.dataPath + "/../cardart/" + input + ".png");
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
