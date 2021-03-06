using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VanguardEngine;
using UnityEngine.UI;
using Mirror;
using System.Threading;
using System;

public class CardFightManager : MonoBehaviour
{
    // Start is called before the first frame update
    public VanguardEngine.CardFight cardFight = null;
    public GameObject PhaseManager;
    public GameObject UnitSlots;
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
    public GameObject POW;
    public GameObject SLD;
    public List<Card> playerDeck;
    public List<Card> enemyDeck;
    public PlayerManager playerManager;
    public VisualInputManager inputManager;
    public Dictionary<string, Card> cardDict;
    public string SQLpath;
    public string namePath;
    public bool inAnimation = false;
    public List<IEnumerator> animations = new List<IEnumerator>();
    public List<IEnumerator> RpcCalls = new List<IEnumerator>();
    public Dictionary<int, RecordedCardValue> _recordedCardValues = new Dictionary<int, RecordedCardValue>();
    public Dictionary<int, RecordedUnitValue> _recordedUnitValues = new Dictionary<int, RecordedUnitValue>();
    public Dictionary<int, int> _recordedShieldValues = new Dictionary<int, int>();
    public int testValue = -1;
    public int shuffleCount = 0;
    public int _playerID = 0;

    public int _attacker = -1;
    public List<int> _attacked;
    public int _booster;

    public void Start()
    {
        Debug.Log("CardFightManager Start");
        this.name = "CardFightManager";
        //Globals.Instance.cardFightManager = this;
        _attacked = new List<int>();
        UnitSlots = GameObject.Find("UnitSlots");
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
        PhaseManager = GameObject.Find("PhaseManager");
        POW = GameObject.Find("POW");
        SLD = GameObject.Find("SLD");
        cardDict = new Dictionary<string, Card>();
        SQLpath = "Data Source=" + Application.dataPath + "/../cards.db;Version=3;";
        namePath = "Data Source=" + Application.dataPath + "/../names.db;Version=3;";
        StartCoroutine(AnimateAnimations());
        StartCoroutine(ProcessRpcCalls());
    }

    public void Initialize(PlayerManager player)
    {
        playerManager = player;
        string deckPath = GameObject.Find("InputField").GetComponent<InputField>().text;
        Debug.Log(GameObject.Find("InputField").GetComponent<InputField>().text);
        Debug.Log("deckPath: " + deckPath);
        if (!System.IO.File.Exists(Application.dataPath + "/../" + deckPath))
            //deckPath = Application.dataPath + "/../" + "dsd01.txt";
            deckPath = "C:/Users/Jason/Desktop/VanguardEngine/VanguardEngine/Properties/testDeck.txt";
        playerManager.CmdInitialize(LoadCards.GenerateList(deckPath, LoadCode.WithRideDeck, -1), playerManager.gameObject);
    }

    IEnumerator AnimateAnimations()
    {
        while (true)
        {
            if (animations.Count > 0)
            {
                inputManager.ResetPositions();
                inAnimation = true;
                inputManager.cardsAreHoverable = false;
                StartCoroutine(animations[0]);
                while (inAnimation)
                {
                    //Debug.Log("inanimation");
                    yield return null;
                }
                animations.RemoveAt(0);
                inputManager.cardsAreHoverable = true;
            }
            //if (cardFight != null && cardFight._player1 != null)
            //{
            //    testValue = cardFight._player1.GetSeedsToBeRead().Count;
            //}
            yield return null;
        }
    }

    IEnumerator ProcessRpcCalls()
    {
        while (true)
        {
            if (RpcCalls.Count > 0)
            {
                IEnumerator NextCall = RpcCalls[0];
                RpcCalls.RemoveAt(0);
                if (NextCall != null)
                    StartCoroutine(NextCall);
            }
            yield return null;
        }
    }

    public bool InAnimation()
    {
        if (animations.Count == 0)
            return false;
        return true;
    }

    public List<string> SyncListToList(SyncList<string> input)
    {
        List<string> output = new List<string>();
        foreach (string item in input)
            output.Add(item);
        return output;
    }

    public void InitializeCardFight(List<string> player1Deck, List<string> player2Deck, int seed, int playerID, string replay)
    {
        SQLpath = "Data Source=" + Application.dataPath + "/../cards.db;Version=3;";
        _playerID = playerID;
        List<Card> player1_generatedDeck = LoadCards.GenerateCardsFromList(new List<string>(player1Deck), SQLpath);
        List<Card> player2_generatedDeck = LoadCards.GenerateCardsFromList(new List<string>(player2Deck), SQLpath);
        List<Card> tokens = LoadCards.GenerateCardsFromList(LoadCards.GenerateList(Application.dataPath + "/../tokens.txt", LoadCode.Tokens, -1), SQLpath);
        Debug.Log("player1 count: " + player1_generatedDeck.Count);
        Debug.Log("player2 count: " + player2_generatedDeck.Count);
        inputManager.InitializeInputManager(_playerID);
        if (replay != "")
        {
            inputManager.inputManager.ReadFromInputLog(replay);
            seed = inputManager.inputManager.GetSeed();
            _playerID = inputManager.inputManager.GetPOV();
            inputManager.SetPlayerID(_playerID);
        }
        //string luaPath = Application.dataPath + "/../lua";
        string luaPath = "C:/Users/Jason/Desktop/VanguardEngine/VanguardEngine/lua";
        cardFight = new VanguardEngine.CardFight();
        cardFight.SetInitialDamage(0);
        cardFight.SetInitialSoul(0);
        cardFight.SetInitialDrop(0);
        //C:/Users/Jason/Desktop/VanguardEngine/VanguardEngine/lua
        Debug.Log("beginning cardfight initialization");
        Debug.Log(Application.dataPath + "/../Names.config");
        cardFight.Initialize(player1_generatedDeck, player2_generatedDeck, tokens, inputManager.inputManager, luaPath, "Data Source=./cards.db;Version=3;", namePath, seed, _playerID);
        Debug.Log("cardfight initialization finished");
        //cardFight._player1.OnRideFromRideDeck += PerformRideFromRideDeck;
        //cardFight._player2.OnRideFromRideDeck += PerformRideFromRideDeck;
        cardFight._player1.OnStandUpVanguard += PerformStandUpVanguard;
        cardFight._player1.OnZoneChanged += ChangeZone;
        cardFight._player1.OnZoneSwapped += SwapZone;
        cardFight._player1.OnUpRightChanged += ChangeUpRight;
        cardFight._player1.OnFaceUpChanged += ChangeFaceUp;
        cardFight.OnDrawPhase += PerformDrawPhase;
        cardFight.OnStandPhase += PerformStandPhase;
        cardFight.OnRidePhase += PerformRidePhase;
        cardFight.OnMainPhase += PerformMainPhase;
        cardFight.OnBattlePhase += PerformBattlePhase;
        cardFight.OnAbilityActivated += ShowAbilityActivated;
        cardFight._player1.OnAttack += PerformAttack;
        cardFight._player2.OnAttack += PerformAttack;
        cardFight._player1.OnShieldValueChanged += ChangeShieldValue;
        //cardFight._player1.OnUnitValueChanged += ChangeUnitValue;
        cardFight._player1.OnCardValueChanged += ChangeCardValue;
        cardFight._player2.OnCardValueChanged += ChangeCardValue;
        cardFight._player1.OnAttackHits += CheckIfAttackHits;
        cardFight._player2.OnAttackHits += CheckIfAttackHits;
        cardFight._player1.OnReveal += PerformReveal;
        cardFight._player2.OnReveal += PerformReveal;
        cardFight._player1.OnSetPrison += PerformSetPrison;
        cardFight._player2.OnSetPrison += PerformSetPrison;
        cardFight._player1.OnImprison += PerformImprison;
        cardFight._player2.OnImprison += PerformImprison;
        cardFight._player1.OnAttackEnds += CheckIfAttackHits;
        cardFight._player2.OnAttackEnds += CheckIfAttackHits;
        cardFight._player1.OnShuffle += SendSeed;
        cardFight._player1.OnSentToDeck += PerformSentToDeck;
        cardFight._player2.OnSentToDeck += PerformSentToDeck;
        cardFight._player1.OnLooking += PerformLooking;
        cardFight._player2.OnLooking += PerformLooking;
        cardFight._player1.OnUpdateValues += PerformUpdateValues;
        cardFight.OnGameOver += OnGameOver;
        //cardFight._player2.OnShuffle += SendSeed;
        inputManager.inputManager.OnChosen += PerformChosen;
        cardFight.OnFree += PerformFree;
        List<CardData> player1IDs = new List<CardData>();
        List<CardData> player2IDs = new List<CardData>();
        List<CardData> player1IDsRide = new List<CardData>();
        List<CardData> player2IDsRide = new List<CardData>();
        foreach (Card card in cardFight._player1.GetDeck())
            player1IDs.Add(new CardData(card.tempID, card.id));
        foreach (Card card in cardFight._player2.GetDeck())
            player2IDs.Add(new CardData(card.tempID, card.id));
        foreach (Card card in cardFight._player1.GetRideDeck())
            player1IDsRide.Add(new CardData(card.tempID, card.id));
        foreach (Card card in cardFight._player2.GetRideDeck())
            player2IDsRide.Add(new CardData(card.tempID, card.id));
        InitializeDecks(player1IDs.ToArray(), player2IDs.ToArray(), player1IDsRide.ToArray(), player2IDsRide.ToArray());
        PlaceStarter(cardFight._player1.Vanguard().id, cardFight._player1.Vanguard().tempID, cardFight._player2.Vanguard().id, cardFight._player2.Vanguard().tempID);
        StartCardFight(cardFight.StartFight);
        Debug.Log("player id: " + _playerID);
        Debug.Log("cardfight started");
    }

    public void StartCardFight(ThreadStart start)
    {
        Thread newThread = new Thread(start);
        newThread.Start();
    }

    public void OnGameOver(object sender, CardEventArgs args)
    {
        OnGameOver(!(_playerID == args.i));
    }

    public void OnGameOver(bool loser)
    {
        IEnumerator Dialog()
        {
            IEnumerator Animation()
            {
                cardFight = null;
                if (!loser)
                    Debug.Log("You win.");
                else
                    Debug.Log("You lose.");
                inputManager.GameOver();
                yield return null;
            }
            animations.Add(Animation());
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void Surrender()
    {
        playerManager.CmdSurrender();
    }

    public void InitializeDecks(CardData[] player1Cards, CardData[] player2Cards, CardData[] player1RideCards, CardData[] player2RideCards)
    {
        CardData[] myDeck;
        CardData[] myRideDeck;
        CardData[] enemyDeck;
        CardData[] enemyRideDeck;
        if (_playerID == 1)
        {
            myDeck = player1Cards;
            myRideDeck = player1RideCards;
            enemyDeck = player2Cards;
            enemyRideDeck = player2RideCards;
            Debug.Log("server setting up decks");
            UnitSlots.GetComponent<UnitSlots>().Initialize(1);
        }
        else
        {
            myDeck = player2Cards;
            myRideDeck = player2RideCards;
            enemyDeck = player1Cards;
            enemyRideDeck = player1RideCards;
            Debug.Log("client setting up decks");
            UnitSlots.GetComponent<UnitSlots>().Initialize(2);
        }
        for (int i = 0; i < myDeck.Length; i++)
        {
            Card card = LookUpCard(myDeck[i]._cardID);
            card.tempID = myDeck[i]._tempID;
            PlayerDeckZone.GetComponent<Pile>().AddCard(card, false, false);
        }
        PlayerDeckZone.GetComponent<Pile>().UpdateVisuals(true);
        for (int i = 0; i < enemyDeck.Length; i++)
        {
            Card card = LookUpCard(enemyDeck[i]._cardID);
            card.tempID = enemyDeck[i]._tempID;
            EnemyDeckZone.GetComponent<Pile>().AddCard(card, false, false);
        }
        EnemyDeckZone.GetComponent<Pile>().UpdateVisuals(true);
        for (int i = 0; i < myRideDeck.Length; i++)
        {
            Card card = LookUpCard(myRideDeck[i]._cardID);
            card.tempID = myRideDeck[i]._tempID;
            PlayerRideDeckZone.GetComponent<Pile>().AddCard(card, false, false);
        }
        PlayerRideDeckZone.GetComponent<Pile>().UpdateVisuals(true);
        for (int i = 0; i < enemyRideDeck.Length; i++)
        {
            Card card = LookUpCard(enemyRideDeck[i]._cardID);
            card.tempID = enemyRideDeck[i]._tempID;
            EnemyRideDeckZone.GetComponent<Pile>().AddCard(card, false, false);
        }
        EnemyRideDeckZone.GetComponent<Pile>().UpdateVisuals(true);
    }

    public void SendSeed(object sender, CardEventArgs e)
    {
        //Debug.Log("sending seed, playerID: " + e.playerID + ", seed: " + e.i);
        IEnumerator Dialog()
        {
            //playerManager.CmdShuffleSeed(e.playerID, e.i);
            animations.Add(Animation());
            yield return null;
        }
        IEnumerator Animation()
        {
            if (shuffleCount > 1)
            {
                GameObject messageBox = GameObject.Instantiate(Globals.Instance.deckMessageBoxPrefab);
                messageBox.transform.position = Globals.Instance.ResetPosition;
                messageBox.transform.SetParent(Field.transform);
                if (isPlayerAction(e.playerID))
                    messageBox.transform.localPosition = PlayerDeckZone.transform.localPosition;
                else
                    messageBox.transform.localPosition = EnemyDeckZone.transform.localPosition;
                messageBox.GetComponentInChildren<Text>().text = "Shuffling.";
                yield return new WaitForSeconds((float).33);
                messageBox.GetComponentInChildren<Text>().text += ".";
                yield return new WaitForSeconds((float).33);
                messageBox.GetComponentInChildren<Text>().text += ".";
                yield return new WaitForSeconds((float).33);
                messageBox.transform.SetParent(null);
                GameObject.Destroy(messageBox);
            }
            shuffleCount++;
            inAnimation = false;
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void ReadSeed(int playerID, int seed)
    {
        if ((playerID == 1 && playerID == 2) || (playerID != 1 && playerID == 1))
        {
            IEnumerator Dialog()
            {
                Debug.Log("reading seed, playerID: " + playerID + ", seed: " + seed);
                cardFight._player1.ReadSeed(seed);
                Debug.Log("seed read");
                if (shuffleCount > 2)
                {
                    GameObject messageBox = GameObject.Instantiate(Globals.Instance.deckMessageBoxPrefab);
                    messageBox.transform.position = Globals.Instance.ResetPosition;
                    messageBox.transform.SetParent(Field.transform);
                    if (isPlayerAction(playerID))
                        messageBox.transform.localPosition = PlayerDeckZone.transform.localPosition;
                    else
                        messageBox.transform.localPosition = EnemyDeckZone.transform.localPosition;
                    messageBox.GetComponentInChildren<Text>().text = "Shuffling.";
                    yield return new WaitForSeconds((float).33);
                    messageBox.GetComponentInChildren<Text>().text += ".";
                    yield return new WaitForSeconds((float).33);
                    messageBox.GetComponentInChildren<Text>().text += ".";
                    yield return new WaitForSeconds((float).33);
                    messageBox.transform.SetParent(null);
                    GameObject.Destroy(messageBox);
                }
                shuffleCount++;
                inAnimation = false;
                yield return null;
            }
            animations.Add(Dialog());
        }
    }

    public void PlaceStarter(string cardID1, int tempID1, string cardID2, int tempID2)
    {
        Card card1 = LookUpCard(cardID1);
        Card card2 = LookUpCard(cardID2);
        GameObject Card1 = GameObject.Instantiate(cardPrefab);
        GameObject Card2 = GameObject.Instantiate(cardPrefab);
        Card1.name = tempID1.ToString();
        Card2.name = tempID2.ToString();
        Card1.GetComponent<CardBehavior>().card = card1;
        Card2.GetComponent<CardBehavior>().card = card2;
        if (_playerID == 1)
        {
            inputManager.PlayerVG.GetComponent<UnitSlotBehavior>().AddCard(card1.OriginalGrade(), card1.critical, card1.power, card1.power, true, false, cardID1, Card1);
            inputManager.EnemyVG.GetComponent<UnitSlotBehavior>().AddCard(card2.OriginalGrade(), card2.critical, card2.power, card2.power, true, false, cardID2, Card2);
        }
        else
        {
            inputManager.PlayerVG.GetComponent<UnitSlotBehavior>().AddCard(card2.OriginalGrade(), card2.critical, card2.power, card2.power, true, false, cardID2, Card2);
            inputManager.EnemyVG.GetComponent<UnitSlotBehavior>().AddCard(card1.OriginalGrade(), card1.critical, card1.power, card1.power, true, false, cardID1, Card1);
        }
    }

    public void ChangeZone(object sender, CardEventArgs e)
    {
        int grade = -1;
        int soul = -1;
        int critical = -1;
        bool faceup = false;
        bool upright = false;
        Card card;
        if (e.currentLocation.Item2 > 0)
        {
            card = cardFight._player1.GetUnitAt(e.currentLocation.Item2, false);
            if (card != null)
            {
                grade = card.OriginalGrade();
                soul = cardFight._player1.GetSoul().Count;
                critical = card.critical;
                faceup = e.faceup;
                upright = e.upright;
            }
        }
        if (e.previousLocation == null)
        {
            Debug.Log("error with previouslocation");
            return;
        }
        if (e.currentLocation == null)
        {
            Debug.Log("error with currentlocation");
            return;
        }
        IEnumerator Dialog()
        {
            ChangeZone(e.previousLocation.Item1, e.previousLocation.Item2, e.currentLocation.Item1, e.currentLocation.Item2, e.card.id, e.card.tempID, e.card.originalOwner, grade, soul, critical, faceup, upright, e.left);
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void ChangeZone(int previousLocation, int previousFL, int currentLocation, int currentFL, string cardID, int tempID, int originalOwner, int grade, int soul, int critical, bool faceup, bool upright, bool left)
    {
        animations.Add(ChangeZoneRoutine(previousLocation, previousFL, currentLocation, currentFL, cardID, tempID, originalOwner, grade, soul, critical, faceup, upright, left));
    }

    IEnumerator ChangeZoneRoutine(int previousLocation, int previousFL, int currentLocation, int currentFL, string cardID, int tempID, int originalOwner, int grade, int soul, int critical, bool faceup, bool upright, bool left)
    {
        Debug.Log("changing zone");
        Card card = LookUpCard(cardID).Clone();
        card.tempID = tempID;
        card.originalOwner = originalOwner;
        inAnimation = true;
        PlayerHand.GetComponent<Hand>().Reset();
        GameObject previousZone = null;
        GameObject currentZone = null;
        GameObject zone = null;
        GameObject newCard = null;
        int location = previousLocation;
        int FL = previousFL;
        string locationName = "";
        Debug.Log("previous location: " + previousLocation + " current location: " + currentLocation);
        for (int i = 0; i < 2; i++)
        {
            if (i == 1)
            {
                location = currentLocation;
                FL = currentFL;
            }
            if (location == Location.Deck)
            {
                locationName = "Deck";
                if (_playerID == 1)
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
                locationName = "Ride Deck";
                Debug.Log("ride deck here");
                if (_playerID == 1)
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
                locationName = "Hand";
                if (_playerID == 1)
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
                locationName = "Drop";
                Debug.Log("drop here");
                if (_playerID == 1)
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
            else if (location == Location.VC || location == Location.RC || location == Location.Arm)
            {
                if (location == Location.VC)
                    locationName = "VC";
                else if (location == Location.RC)
                    locationName = "RC";
                else if (location == Location.Arm)
                    locationName = "Arm";
                Debug.Log("vc/rc here");
                zone = UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(FL);
                Debug.Log("FL: " + FL);
                if (zone == null)
                    Debug.Log("no unit found");
            }
            else if (location == Location.GC)
            {
                locationName = "GC";
                Debug.Log("gc here");
                zone = Globals.Instance.guardianCircle.gameObject;
            }
            else if (location == Location.Trigger)
            {
                locationName = "Trigger Zone";
                Debug.Log("trigger here");
                if (isPlayerAction(card.originalOwner))
                    zone = Globals.Instance.playerTriggerZone;
                else
                    zone = Globals.Instance.enemyTriggerZone;
            }
            else if (location == Location.Damage)
            {
                locationName = "Damage Zone";
                Debug.Log("damage here");
                if (isPlayerAction(card.originalOwner))
                    zone = Globals.Instance.playerDamageZone;
                else
                    zone = Globals.Instance.enemyDamageZone;
            }
            else if (location == Location.Soul || location == Location.originalDress)
            {
                if (location == Location.Soul)
                    locationName = "Soul";
                else if (location == Location.originalDress)
                    locationName = "originalDress";
                Debug.Log("soul/oD here");
                zone = Globals.Instance.unitSlots.GetUnitSlot(FL);
            }
            else if (location == Location.OrderArea)
            {
                locationName = "Order Area";
                Debug.Log("order area here");
                zone = Globals.Instance.orderArea;
            }
            else if (location == Location.Order)
            {
                locationName = "Order Zone";
                Debug.Log("order zone here");
                if (isPlayerAction(card.originalOwner))
                {
                    if (card.unitType > -1) // for imprisoning
                        zone = Globals.Instance.enemyOrderZone.gameObject;
                    else
                        zone = Globals.Instance.playerOrderZone.gameObject;
                }
                else
                {
                    if (card.unitType > -1)
                        zone = Globals.Instance.playerOrderZone.gameObject;
                    else
                        zone = Globals.Instance.enemyOrderZone.gameObject;
                }
            }
            else if (location == Location.Bind)
            {
                locationName = "Bind Zone";
                Debug.Log("bind zone here");
                if (isPlayerAction(card.originalOwner))
                    zone = Globals.Instance.playerBindZone.gameObject;
                else
                    zone = Globals.Instance.enemyBindZone.gameObject;
            }
            else if (location == -1)
            {
                locationName = "???";
                Debug.Log("other location here");
                zone = null;
                //GameObject removed = GameObject.Find(card.tempID.ToString());
                //if (removed != null)
                //{
                //    removed.transform.SetParent(null);
                //    GameObject.Destroy(removed);
                //}
            }
            else
            {
                inAnimation = false;
                yield break;
            }
            if (i < 1)
                previousZone = zone;
            else
                currentZone = zone;
        }

        if (previousLocation == Location.Soul || previousLocation == Location.originalDress)
        {
            if (Globals.Instance.unitSlots.GetUnitSlot(previousFL) != null)
            {
                Debug.Log("removing from soul");
                Globals.Instance.unitSlots.GetUnitSlot(previousFL).GetComponent<UnitSlotBehavior>().RemoveFromSoul(card);
            }
            else
            {
                previousZone = Globals.Instance.guardianCircle.gameObject;
            }
        }
        if (currentLocation == Location.Soul || currentLocation == Location.originalDress)
        {
            bool addToSoul = true;
            if (Globals.Instance.unitSlots.GetUnitSlot(currentFL) == null)
                addToSoul = false;
            else
            {
                foreach (Card c in Globals.Instance.unitSlots.GetUnitSlot(currentFL).GetComponent<UnitSlotBehavior>()._soul)
                {
                    if (c.tempID == card.tempID)
                    {
                        addToSoul = false;
                        break;
                    }
                }
            }
            if (addToSoul)
            {
                Debug.Log("adding to soul");
                Globals.Instance.unitSlots.GetUnitSlot(currentFL).GetComponent<UnitSlotBehavior>()._soul.Add(card);
            }
        }

        if (currentZone != null && currentZone.GetComponent<UnitSlotBehavior>() != null && currentLocation != Location.Soul && previousZone == null)
        {
            GameObject token = CreateNewCard(card.id, card.tempID);
            Debug.Log("token generated: " + token.name);
            currentZone.GetComponent<UnitSlotBehavior>().AddCard(card.OriginalGrade(), critical, card.power, card.power, upright, true, card.id, token);
            inAnimation = false;
            yield break;
        }
        
        if (currentZone == null && previousZone != null && previousZone.GetComponent<UnitSlotBehavior>() != null)
        {
            Debug.Log("removing token");
            GameObject removedCard = previousZone.GetComponent<UnitSlotBehavior>().RemoveCard(card.tempID);
            if (removedCard != false)
            {
                removedCard.transform.SetParent(null);
                GameObject.Destroy(removedCard);
            }
            inAnimation = false;
            yield break;
        }

        if (currentZone == null || previousZone == null || (currentFL >= 0 && previousFL >= 0 && currentFL == previousFL))
        {
            inAnimation = false;
            Debug.Log("destroying object");
            GameObject remove = GameObject.Find(card.tempID.ToString());
            if (remove != null)
            {
                remove.transform.SetParent(null);
                GameObject.Destroy(remove);
            }
            yield break;
        }

        if (currentZone == Globals.Instance.playerTriggerZone || currentZone == Globals.Instance.enemyTriggerZone)
        {
            Field.GetComponent<Board>().inAnimation = true;
            if (currentZone == Globals.Instance.playerTriggerZone)
                StartCoroutine(Field.GetComponent<Board>().ZoomInOnTrigger(true));
            else
                StartCoroutine(Field.GetComponent<Board>().ZoomInOnTrigger(false));
            while (Field.GetComponent<Board>().inAnimation)
                yield return null;
            yield return new WaitForSecondsRealtime(1);
            Debug.Log("done zooming in");
        }

        if (previousZone == PlayerHand || previousZone == EnemyHand)
        {
            for (int i = 0; i < previousZone.transform.childCount; i++)
            {
                newCard = previousZone.transform.GetChild(i).gameObject;
                if (int.Parse(newCard.name) == card.tempID)
                {
                    //GameObject.Destroy(newCard.GetComponent<CardBehavior>().selectedCard);
                    break;
                }
            }
        }
        else if (previousZone == Globals.Instance.guardianCircle.gameObject)
        {
            Debug.Log("removing from gc");
            newCard = Globals.Instance.guardianCircle.RemoveCard(card.tempID);
            if (newCard == null)
            {
                newCard = CreateNewCard(card.id, card.tempID);
                newCard.transform.SetParent(Field.transform);
                newCard.transform.position = previousZone.transform.position;
                if (previousZone.name.Contains("Enemy"))
                    newCard.transform.Rotate(0, 0, 180);
            }
        }
        else if (previousZone.GetComponent<DamageZone>() != null)
            newCard = previousZone.GetComponent<DamageZone>().RemoveCard(card.tempID);
        else if (previousZone.name.Contains("Trigger") || previousZone.name.Contains("Damage"))
        {
            newCard = previousZone.transform.GetChild(0).gameObject;
            newCard.transform.SetParent(GameObject.Find("Field").transform);
        }
        else if (previousLocation == Location.Arm)
        {
            newCard = previousZone.GetComponent<UnitSlotBehavior>().RemoveArm(tempID);
        }
        else if (previousZone.GetComponent<UnitSlotBehavior>() != null && previousZone.GetComponent<UnitSlotBehavior>().unit != null && Int32.Parse(previousZone.GetComponent<UnitSlotBehavior>().unit.name) == card.tempID)
        {
            Debug.Log("removing from unit slot");
            previousZone.GetComponent<UnitSlotBehavior>().DisableCard();
            newCard = previousZone.GetComponent<UnitSlotBehavior>().RemoveCard(Int32.Parse(previousZone.GetComponent<UnitSlotBehavior>().unit.name));
            newCard.transform.SetParent(GameObject.Find("Field").transform);
        }
        else if (previousZone.GetComponent<UnitSlotBehavior>() != null && previousZone.GetComponent<UnitSlotBehavior>().unit == null)
        {
            newCard = CreateNewCard(card.id, card.tempID);
            newCard.transform.SetParent(Field.transform);
            newCard.transform.localPosition = previousZone.transform.localPosition;
            if (previousZone.name.Contains("Enemy"))
                newCard.transform.Rotate(0, 0, 180);
        }
        else if (previousZone.name == "OrderArea")
        {
            newCard = Globals.Instance.orderArea.transform.GetChild(0).gameObject;
            newCard.transform.SetParent(GameObject.Find("Field").transform);
        }
        else
        {
            Debug.Log("else");
            newCard = CreateNewCard(card.id, card.tempID);
            newCard.transform.SetParent(Field.transform);
            newCard.transform.position = previousZone.transform.position;
            if (previousZone.name.Contains("Enemy"))
                newCard.transform.Rotate(0, 0, 180);
            //newCard.transform.localScale = Field.transform.localScale;
        }
        if (newCard == null)
        {
            newCard = CreateNewCard(cardID, tempID);
            newCard.transform.SetParent(GameObject.Find("Field").transform);
            newCard.transform.localPosition = previousZone.transform.localPosition;
        }
        newCard.name = card.tempID.ToString();
        newCard.GetComponent<CardBehavior>().Reset();
        if (faceup)
            newCard.GetComponent<CardBehavior>().faceup = true;
        else
            newCard.GetComponent<CardBehavior>().faceup = false;

        if (currentZone.name.Contains("Trigger"))
        {
            newCard.transform.Rotate(new Vector3(0, 0, 90));
            newCard.transform.localScale = newCard.transform.localScale * (float)(8 / 1.2);
        }


        if (currentZone == EnemyDeckZone || currentZone == EnemyHand)
        {
            newCard.GetComponent<Image>().sprite = LoadSprite(Application.dataPath + "/../cardart/FaceDownCard.jpg");
        }
        else if (currentZone.GetComponent<UnitSlotBehavior>() != null)
        {
            newCard.GetComponent<CardBehavior>().faceup = true;
            newCard.GetComponent<Image>().sprite = LoadSprite(FixFileName(card.id));
            newCard.GetComponent<CardBehavior>().card = LookUpCard(card.id);
        }
        else
            newCard.GetComponent<Image>().sprite = LoadSprite(FixFileName(card.id));

        Debug.Log(currentZone.name);
        if (currentZone.name.Contains("Enemy"))
        {
            Debug.Log("is enemy");
            newCard.transform.Rotate(new Vector3(0, 0, 180));
        }
        GameObject blankCard = GameObject.Instantiate(cardPrefab);
        blankCard.transform.GetComponent<Image>().enabled = false;


        if (currentZone == PlayerHand || currentZone == EnemyHand ||
            currentZone == Globals.Instance.playerDamageZone || currentZone == Globals.Instance.enemyDamageZone)
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
            newCard.transform.position = Vector2.MoveTowards(newCard.transform.position, blankCard.transform.position, step);
            yield return null;
        }
        blankCard.transform.SetParent(null);
        GameObject.Destroy(blankCard);

        if (currentZone == Globals.Instance.playerTriggerZone || currentZone == Globals.Instance.enemyTriggerZone)
        {
            yield return new WaitForSecondsRealtime(1);
            Field.GetComponent<Board>().inAnimation = true;
            StartCoroutine(Field.GetComponent<Board>().ZoomBack());
            while (Field.GetComponent<Board>().inAnimation)
                yield return null;
        }

        if (!currentZone.name.Contains("Trigger") && !currentZone.name.Contains("Damage"))
            newCard.transform.rotation = Quaternion.Euler(0, 0, 0);


        if (currentZone.name.Contains("Hand") || currentZone.name.Contains("Trigger"))
        {
            newCard.transform.SetParent(currentZone.transform);
            if (currentZone.name == "EnemyHand")
            {
                Debug.Log("shuffling hand");
                StartCoroutine(currentZone.GetComponent<Hand>().Shuffle());
                while (currentZone.GetComponent<Hand>().inAnimation)
                    yield return null;
            }
        }
        else if (currentZone.GetComponent<UnitSlotBehavior>() != null && currentLocation != Location.Soul && currentLocation != Location.originalDress)
        {
            if (currentLocation != Location.Arm)
            {
                int currentPower = card.power;
                int currentCritical = card.critical;
                if (_recordedUnitValues.ContainsKey(FL))
                {
                    currentPower = _recordedUnitValues[FL].currentPower;
                    currentCritical = _recordedUnitValues[FL].currentCritical;
                }
                currentZone.GetComponent<UnitSlotBehavior>().AddCard(card.OriginalGrade(), currentCritical, currentPower, card.power, upright, true, card.id, newCard);
            }
            else
            {
                currentZone.GetComponent<UnitSlotBehavior>().AddArm(left, card.id, newCard);
            }
        }
        else if (currentZone.GetComponent<GuardianCircle>() != null)
            currentZone.GetComponent<GuardianCircle>().AddCard(newCard, card.tempID);
        else if (currentZone.GetComponent<DamageZone>() != null)
            currentZone.GetComponent<DamageZone>().AddCard(newCard, card.tempID);
        else if (currentZone.name == "OrderArea")
            newCard.transform.SetParent(currentZone.transform);
        else
        {
            newCard.transform.SetParent(null);
            GameObject.Destroy(newCard);
        }


        if (currentZone.GetComponent<Pile>() != null)
        {
            if (currentZone.name == "PlayerDeck" || currentZone.name == "EnemyDeck")
                currentZone.GetComponent<Pile>().AddCard(card, false);
            else
                currentZone.GetComponent<Pile>().AddCard(card, true);
        }

        Globals.Instance.playerMiscStats.SetWorld(Globals.Instance.playerOrderZone.GetComponent<Pile>().GetCards());
        Globals.Instance.enemyMiscStats.SetWorld(Globals.Instance.enemyOrderZone.GetComponent<Pile>().GetCards());
        inputManager.cardsAreHoverable = true;
        Globals.Instance.logWindow.AddLogItem(isPlayerAction(card.originalOwner), cardID, Location.GetName(previousLocation), true, "To " + Location.GetName(currentLocation), "", "");
        inAnimation = false;
    }

    public void SwapZone(object sender, CardEventArgs e)
    {
        Debug.Log("swapping zones");
        if (e.previousLocation == null || e.currentLocation == null)
        {
            Debug.Log("tuple swap error");
            return;
        }
        if (e.previousLocation.Item2 == e.currentLocation.Item2)
            return;
        IEnumerator Dialog()
        {
            SwapZone(e.previousLocation.Item2, e.currentLocation.Item2);
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void SwapZone(int previousFL, int currentFL)
    {
        animations.Add(ChangeZoneRoutine(previousFL, currentFL));
    }

    IEnumerator ChangeZoneRoutine(int previousFL, int currentFL)
    {
        string previousCardID = UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(previousFL).GetComponent<UnitSlotBehavior>()._cardID;
        string currentCardID = UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(previousFL).GetComponent<UnitSlotBehavior>()._cardID;
        UnitSlots.GetComponent<UnitSlots>().SwapUnitSlots(previousFL, currentFL);
        UnitSlots.GetComponent<UnitSlots>().Hide(previousFL);
        UnitSlots.GetComponent<UnitSlots>().Hide(currentFL);
        int i = 2;
        IEnumerator MoveUnitSlot(GameObject previousObject, GameObject nextObject, string cardID)
        {
            if (cardID == "")
            {
                i--;
                yield break;
            }
            GameObject newCard = CreateNewCard(cardID, -1);
            newCard.transform.SetParent(Field.transform);
            newCard.transform.position = previousObject.transform.position;
            if (previousObject.transform.name.Contains("Enemy"))
                newCard.transform.Rotate(new Vector3(0, 0, 180));
            float step = 2000 * Time.deltaTime;
            while (Vector3.Distance(newCard.transform.position, nextObject.transform.position) > 0.001f)
            {
                newCard.transform.position = Vector2.MoveTowards(newCard.transform.position, nextObject.transform.position, step);
                yield return null;
            }
            GameObject.Destroy(newCard);
            i--;
        }
        StartCoroutine(MoveUnitSlot(UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(previousFL), UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(currentFL), previousCardID));
        StartCoroutine(MoveUnitSlot(UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(currentFL), UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(previousFL), currentCardID));
        while (i > 0)
            yield return null;
        UnitSlots.GetComponent<UnitSlots>().Show(previousFL);
        UnitSlots.GetComponent<UnitSlots>().Show(currentFL);
        inAnimation = false;
    }

    public void ShowAbilityActivated(object sender, CardEventArgs e)
    {
        Debug.Log("ShowAbilityActivated");
        CardFight cardFight = sender as CardFight;
        string location = Location.GetName(cardFight._player1.GetLocation(e.card));
        string description = e.abilityText;
        IEnumerator Dialog()
        {
            ShowAbilityActivated(e.card, location, description);
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void ShowAbilityActivated(Card card, string location, string description)
    {
        animations.Add(ShowAbilityActivatedRoutine(card, location, description));
    }

    IEnumerator ShowAbilityActivatedRoutine(Card card, string location, string description)
    {
        int tempID = card.tempID;
        string cardID = card.id;
        string name = card.name;
        GameObject target = GameObject.Find(tempID.ToString());
        bool abilityBoxAnimation = true;
        bool sliding = false;
        IEnumerator SlideAbilityBox()
        {
            GameObject abilityBox = Globals.Instance.AbilityBox;
            abilityBox.GetComponentInChildren<Text>().text = name + "'s ability activates!";
            abilityBox.transform.localPosition = Globals.Instance.AbilityBoxResetPosition;
            float step = 600 * Time.deltaTime;
            while (Vector3.Distance(abilityBox.transform.localPosition, Globals.Instance.AbilityBoxSlidePosition) > 0.001f)
            {
                abilityBox.transform.localPosition = Vector2.MoveTowards(abilityBox.transform.localPosition, Globals.Instance.AbilityBoxSlidePosition, step);
                yield return null;
            }
            yield return new WaitForSecondsRealtime(1);
            while (Vector3.Distance(abilityBox.transform.localPosition, Globals.Instance.AbilityBoxResetPosition) > 0.001f)
            {
                abilityBox.transform.localPosition = Vector2.MoveTowards(abilityBox.transform.localPosition, Globals.Instance.AbilityBoxResetPosition, step);
                yield return null;
            }
            abilityBoxAnimation = false;
        }
        StartCoroutine(SlideAbilityBox());
        if (target != null)
        {
            if (target.GetComponentInParent<UnitSlotBehavior>() != null)
            {
                StartCoroutine(target.transform.parent.GetComponentInChildren<UnitSelectArea>().Flash(Color.yellow));
                target.transform.parent.GetComponentInChildren<UnitSelectArea>().inAnimation = true;
                while (target.transform.parent.GetComponentInChildren<UnitSelectArea>().inAnimation)
                    yield return null;
            }
            else if (target.GetComponentInParent<Hand>() != null)
            {
                if (target.transform.parent.name == "EnemyHand" && !target.GetComponent<CardBehavior>().faceup)
                {
                    target.GetComponent<CardBehavior>().card = LookUpCard(cardID);
                    target.GetComponent<CardBehavior>().inAnimation = true;
                    StartCoroutine(target.GetComponent<CardBehavior>().Flip());
                    while (target.GetComponent<CardBehavior>().inAnimation)
                        yield return null;
                }
                target.GetComponent<CardBehavior>().inAnimation = true;
                StartCoroutine(target.GetComponent<CardBehavior>().Flash(Color.yellow));
                while (target.GetComponent<CardBehavior>().inAnimation)
                    yield return null;
                if (target.transform.parent.name == "EnemyHand" && target.GetComponent<CardBehavior>().faceup)
                {
                    target.GetComponent<CardBehavior>().inAnimation = true;
                    StartCoroutine(target.GetComponent<CardBehavior>().Flip());
                    while (target.GetComponent<CardBehavior>().inAnimation)
                        yield return null;
                }
            }
        }
        else if (Globals.Instance.unitSlots.GetUnitSlotWithSoul(tempID) != null)
        {
            IEnumerator SlideOutFromSoul()
            {
                GameObject unit = Globals.Instance.unitSlots.GetUnitSlotWithSoul(tempID);
                GameObject newCard = CreateNewCard(cardID, -1);
                newCard.transform.SetParent(GameObject.Find("Field").transform);
                newCard.transform.SetSiblingIndex(unit.transform.GetSiblingIndex() - 1);
                newCard.transform.localPosition = unit.transform.localPosition;
                float step = 600 * Time.deltaTime;
                Vector3 slidePosition = new Vector3(newCard.transform.localPosition.x + 75, newCard.transform.localPosition.y, 0);
                while (Vector3.Distance(newCard.transform.localPosition, slidePosition) > 0.001f)
                {
                    newCard.transform.localPosition = Vector2.MoveTowards(newCard.transform.localPosition, slidePosition, step);
                    yield return null;
                }
                newCard.GetComponent<CardBehavior>().inAnimation = true;
                StartCoroutine(newCard.GetComponent<CardBehavior>().Flash(Color.yellow));
                while (newCard.GetComponent<CardBehavior>().inAnimation)
                    yield return null;
                slidePosition = new Vector3(newCard.transform.localPosition.x - 75, newCard.transform.localPosition.y, 0);
                while (Vector3.Distance(newCard.transform.localPosition, slidePosition) > 0.001f)
                {
                    newCard.transform.localPosition = Vector2.MoveTowards(newCard.transform.localPosition, slidePosition, step);
                    yield return null;
                }
                GameObject.Destroy(newCard);
                sliding = false;
            }
            sliding = true;
            StartCoroutine(SlideOutFromSoul());
            while (sliding)
                yield return null;
        }
        while (abilityBoxAnimation)
            yield return null;
        Globals.Instance.logWindow.AddLogItem(isPlayerAction(card.originalOwner), cardID, location, true, "Ability activated", "", "");
        if (description != "")
            Globals.Instance.logWindow.AddLogItem(isPlayerAction(card.originalOwner), description);
        inAnimation = false;
    }

    public GameObject CreateNewCard(string cardID, int tempID)
    {
        GameObject newCard = GameObject.Instantiate(cardPrefab);
        newCard.name = tempID.ToString();
        newCard.GetComponent<CardBehavior>().card = LookUpCard(cardID);
        newCard.GetComponent<CardBehavior>().card.tempID = tempID;
        newCard.GetComponent<CardBehavior>().faceup = true;
        newCard.GetComponent<Image>().sprite = LoadSprite(FixFileName(cardID));
        return newCard;
    }

    public void PerformStandUpVanguard(object sender, CardEventArgs e)
    {
        IEnumerator Dialog()
        {
            StandUpVanguard();
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void StandUpVanguard()
    {
        animations.Add(WaitForFlip());
    }

    IEnumerator WaitForFlip()
    {
        inAnimation = true;
        StartCoroutine(inputManager.PlayerVG.GetComponent<UnitSlotBehavior>().Flip());
        StartCoroutine(inputManager.EnemyVG.GetComponent<UnitSlotBehavior>().Flip());
        while (inputManager.PlayerVG.GetComponent<UnitSlotBehavior>().inAnimation)
            yield return null;
        inAnimation = false;
    }

    public void PerformDrawPhase(object sender, CardEventArgs e)
    {
        Debug.Log("draw phase");
        if (sender == null)
        {
            Debug.Log("error with sender");
            return;
        }
        int player = cardFight._actingPlayer._playerID;
        int turn = cardFight._turn;
        IEnumerator Dialog()
        {
            ChangePhase(Phase.Draw, player, turn);
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void PerformStandPhase(object sender, CardEventArgs e)
    {
        Debug.Log("stand phase");
        if (sender == null)
        {
            Debug.Log("error with sender");
            return;
        }
        int player = cardFight._actingPlayer._playerID;
        int turn = cardFight._turn;
        IEnumerator Dialog()
        {
            ChangePhase(Phase.Stand, player, turn);
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void PerformRidePhase(object sender, CardEventArgs e)
    {
        Debug.Log("ride phase");
        if (sender == null)
        {
            Debug.Log("error with sender");
            return;
        }
        int player = cardFight._actingPlayer._playerID;
        int turn = cardFight._turn;
        if (isPlayerAction(cardFight._actingPlayer._playerID))
            Debug.Log("my turn");
        else
            Debug.Log("their turn");
        IEnumerator Dialog()
        {
            ChangePhase(Phase.Ride, player, turn);
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void PerformMainPhase(object sender, CardEventArgs e)
    {
        Debug.Log("main phase");
        if (sender == null)
        {
            Debug.Log("error with sender");
            return;
        }
        int player = cardFight._actingPlayer._playerID;
        int turn = cardFight._turn;
        IEnumerator Dialog()
        {
            ChangePhase(Phase.Main, player, turn);
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void PerformBattlePhase(object sender, CardEventArgs e)
    {
        Debug.Log("battle phase");
        if (sender == null)
        {
            Debug.Log("error with sender");
            return;
        }
        int player = cardFight._actingPlayer._playerID;
        int turn = cardFight._turn;
        IEnumerator Dialog()
        {
            ChangePhase(Phase.Battle, player, turn);
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void ChangePhase(int phase, int actingPlayer, int turn)
    {
        if ((actingPlayer == 1 && _playerID == 1) || (actingPlayer == 2 && _playerID == 2))
            animations.Add(WaitForPhase(phase, true, turn));
        else
            animations.Add(WaitForPhase(phase, false, turn));
    }

    IEnumerator WaitForPhase(int phase, bool actingPlayer, int turn)
    {
        inAnimation = true;
        inAnimation = true;
        PhaseManager.GetComponent<PhaseManager>().ChangePhase(phase, actingPlayer, turn);
        yield return new WaitForSeconds(1);
        inAnimation = false;
    }

    public void PerformAttack(object sender, CardEventArgs e)
    {
        VanguardEngine.Player player = sender as VanguardEngine.Player;
        Debug.Log(player.GetAttacker().tempID);
        Debug.Log(player.GetAttackedCards().Count);
        int booster = -1;
        if (player.Booster() != null)
            booster = player.GetCircle(player.Booster());
        int attackingCircle = player.GetCircle(player.GetAttacker());
        List<int> attackedCircles = new List<int>();
        foreach (Card card in player.GetAttackedCards())
            attackedCircles.Add(player.GetCircle(card));
        foreach (int attackedCircle in attackedCircles)
        {
            IEnumerator Dialog()
            {
                PerformAttack(attackingCircle, attackedCircle, booster);
                yield return null;
            }
            RpcCalls.Add(Dialog());
        }
    }

    public void PerformAttack(int attackingCircle, int attackedCircle, int booster)
    {
        _attacker = attackingCircle;
        _attacked.Add(attackedCircle);
        _booster = booster;
        if (Globals.Instance.unitSlots.GetUnitSlot(attackedCircle) != null)
        {
            UnitSlotBehavior unit = Globals.Instance.unitSlots.GetUnitSlot(attackedCircle).GetComponent<UnitSlotBehavior>();
            if (unit == null)
            {
                IEnumerator Dummy()
                {
                    while (true)
                        yield return null;
                }
                animations.Add(Dummy());
            }
            animations.Add(Flash(unit._cardID, Int32.Parse(unit.unit.name)));
        }
        animations.Add(ShowAttack(attackingCircle, attackedCircle));
    }

    IEnumerator ShowAttack(int attackingCircle, int attackedCircle)
    {
        UnitSlots.GetComponent<UnitSlots>().PerformAttack(attackingCircle, attackedCircle);
        POW.GetComponent<POWSLD>().compare = true;
        SLD.GetComponent<POWSLD>().compare = true;
        POW.GetComponent<POWSLD>().auto = true;
        SLD.GetComponent<POWSLD>().auto = true;
        inAnimation = false;
        yield return null;
    }

    IEnumerator Flash(string cardID, int tempID)
    {
        UnitSelectArea unit;
        UnitSlotBehavior unitSlot = Globals.Instance.unitSlots.FindUnitSlot(tempID);
        int circle = 0;
        if (unitSlot != null)
            circle = unitSlot._FL;
        if (UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(circle) != null)
        {
            unit = UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(circle).GetComponentInChildren<UnitSelectArea>();
            unit.inAnimation = true;
            StartCoroutine(unit.Flash(Color.white));
            while (unit.inAnimation)
                yield return null;
            inAnimation = false;
            yield break;
        }
        GameObject drop = null;
        if (PlayerDropZone.GetComponent<Pile>().ContainsCard(tempID))
            drop = PlayerDropZone;
        else if (EnemyDropZone.GetComponent<Pile>().ContainsCard(tempID))
            drop = EnemyDropZone;
        if (drop != null)
        {
            GameObject newCard = CreateNewCard(cardID, tempID);
            newCard.transform.SetParent(drop.transform);
            newCard.transform.localPosition = new Vector3(0, 0, 0);
            newCard.GetComponent<CardBehavior>().inAnimation = true;
            StartCoroutine(newCard.GetComponent<CardBehavior>().Flash(Color.white));
            while (newCard.GetComponent<CardBehavior>().inAnimation)
                yield return null;
            newCard.transform.SetParent(null);
            GameObject.Destroy(newCard);
            inAnimation = false;
            yield break;
        }
        inAnimation = false;
        yield break;
    }

    public void ChangeUpRight(object sender, CardEventArgs e)
    {
        Debug.Log("rotating");
        Player player = sender as Player;
        if (e == null)
        {
            Debug.Log("error with cardeventargs");
            return;
        }
        int circle = player.GetCircle(player.GetCard(e.i));
        IEnumerator Dialog()
        {
            ChangeUpRight(circle, e.upright);
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void ChangeUpRight(int circle, bool upright)
    {
        animations.Add(RotateUnit(circle, upright));
    }

    IEnumerator RotateUnit(int circle, bool upright)
    {
        UnitSlotBehavior unit;
        if (UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(circle) != null)
        {
            unit = UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(circle).GetComponent<UnitSlotBehavior>();
            unit.inAnimation = true;
            StartCoroutine(unit.Rotate(upright));
            while (unit.inAnimation)
                yield return null;
        }
        inAnimation = false;
    }

    public void ChangeFaceUp(object sender, CardEventArgs e)
    {
        Debug.Log("flipping");
        IEnumerator Dialog()
        {
            ChangeFaceUp(e.i, e.faceup);
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void ChangeFaceUp(int tempID, bool faceup)
    {
        animations.Add(Flip(tempID, faceup));
    }

    IEnumerator Flip(int tempID, bool faceup)
    {
        if (GameObject.Find(tempID.ToString()) != null)
        {
            CardBehavior card = GameObject.Find(tempID.ToString()).GetComponent<CardBehavior>();
            //if (card.faceup != faceup)
            //{
            //    card.inAnimation = true;
            //    StartCoroutine(card.Flip());
            //    while (card.inAnimation)
            //        yield return null;
            //}
            card.inAnimation = true;
            StartCoroutine(card.Flip());
            while (card.inAnimation)
                yield return null;
            if (card.transform.parent.GetComponent<UnitSlotBehavior>() != null)
                card.transform.parent.GetComponent<UnitSlotBehavior>()._faceup = faceup;
        }
        if (Globals.Instance.playerOrderZone.ContainsCard(tempID))
        {
            Globals.Instance.playerOrderZone.Flipped(tempID, faceup);
        }
        if (Globals.Instance.enemyOrderZone.ContainsCard(tempID))
        {
            Globals.Instance.enemyOrderZone.Flipped(tempID, faceup);
        }

        inAnimation = false;
    }

    public void ChangeShieldValue(object sender, CardEventArgs e)
    {
        //Player player = sender as Player;
        //Debug.Log("updating shield value: " + e.currentShield);
        IEnumerator Dialog()
        {
            ChangeShieldValue(e.circle, e.currentShield);
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void ChangeShieldValue(int circle, int shield)
    {
        animations.Add(UpdateShieldValue(circle, shield));
    }

    IEnumerator UpdateShieldValue(int circle, int shield)
    {
        _recordedShieldValues[circle] = shield;
        UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(circle).GetComponent<UnitSlotBehavior>()._shield = shield;
        inAnimation = false;
        yield return null;
    }

    public void ChangeUnitValue(object sender, CardEventArgs e)
    {
        //Player player = sender as Player;
        //Debug.Log("updating power value: " + e.currentPower);
        //if (e == null)
        //    Debug.Log("error with CardEventArgs");
        //else
        //{
        //    IEnumerator Dialog()
        //    {
        //        ChangeUnitValue(e.circle, cardFight._player1.CalculatePowerOfUnit(e.circle), e.currentCritical);
        //        yield return null;
        //    }
        //    RpcCalls.Add(Dialog());
        //}
        //IEnumerator Dialog()
        //{
        //    ChangeUnitValue(e.circle, e.recordedUnitValue.currentPower, e.recordedUnitValue.currentCritical);
        //    yield return null;
        //}
        //RpcCalls.Add(Dialog());
    }

    public void ChangeUnitValue(int circle, int power, int critical)
    {
        animations.Add(UpdateUnitValue(circle, power, critical));
    }

    IEnumerator UpdateUnitValue(int circle, int power, int critical)
    {
        //UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(circle).GetComponent<UnitSlotBehavior>()._power = power;
        //UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(circle).GetComponent<UnitSlotBehavior>()._critical = critical;
        //_recordedUnitValues[circle] = recordedUnitValue;
        UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(circle).GetComponent<UnitSlotBehavior>()._power = power;
        UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(circle).GetComponent<UnitSlotBehavior>()._critical = critical;
        //foreach (GameObject slot in UnitSlots.GetComponent<UnitSlots>().GetUnitSlots())
        //{
        //    UnitSlotBehavior unitSlot = slot.GetComponent<UnitSlotBehavior>();
        //    if (_recordedUnitValues.ContainsKey(unitSlot._FL))
        //    {
        //        unitSlot._power = _recordedUnitValues[unitSlot._FL].currentPower;
        //        unitSlot._critical = _recordedUnitValues[unitSlot._FL].currentCritical;
        //    }
        //}
        inAnimation = false;
        yield return null;
    }

    public void ChangeCardValue(object sender, CardEventArgs e)
    {
        Player player = sender as Player;
        IEnumerator Dialog()
        {
            ChangeCardValue(e.i, e.currentGrade);
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void ChangeCardValue(int tempID, int currentGrade)
    {
        animations.Add(UpdateCardValue(tempID, currentGrade));
    }

    IEnumerator UpdateCardValue(int tempID, int currentGrade)
    {
        Debug.Log("updating grade, tempID: " + tempID + ", grade: " + currentGrade);
        if (!_recordedCardValues.ContainsKey(tempID))
            _recordedCardValues[tempID] = new RecordedCardValue(currentGrade);
        else
            _recordedCardValues[tempID].currentGrade = currentGrade;
        inAnimation = false;
        yield return null;
    }

    public void PerformUpdateValues(object sender, CardEventArgs e)
    {
        IEnumerator Animation()
        {
            foreach (CardValues cv in e.cardValues)
            {
                if (Globals.Instance.unitSlots.GetUnitSlot(cv.circle) != null)
                {
                    UnitSlotBehavior unitSlot = Globals.Instance.unitSlots.GetUnitSlot(cv.circle).GetComponent<UnitSlotBehavior>();
                    if (unitSlot.unit == null)
                        continue;
                    unitSlot._power = cv.power;
                    unitSlot._critical = cv.critical;
                    foreach (var cardState in cv.cardStates)
                        unitSlot.UpdateCardState(cardState.Item1, cardState.Item2);
                }
            }
            inAnimation = false;
            yield return null;
        }
        IEnumerator Dialog()
        {
            animations.Add(Animation());
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void CheckIfAttackHits(object sender, CardEventArgs e)
    {
        IEnumerator Dialog()
        {
            CheckIfAttackHits();
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void CheckIfAttackHits()
    {
        animations.Add(CheckIfAttackHitsAnimation());
    }

    IEnumerator CheckIfAttackHitsAnimation()
    {
        UnitSlots.GetComponent<UnitSlots>().EndAttack();
        _attacked.Clear();
        POW.GetComponent<POWSLD>().Reset();
        SLD.GetComponent<POWSLD>().Reset();
        inAnimation = false;
        yield return null;
    }

    public void PerformSentToDeck(object sender, CardEventArgs e)
    {
        if (isPlayerAction(e.playerID))
            return;
        IEnumerator Animation()
        {
            GameObject messageBox = GameObject.Instantiate(Globals.Instance.deckMessageBoxPrefab);
            messageBox.transform.position = Globals.Instance.ResetPosition;
            messageBox.transform.SetParent(Field.transform);
            messageBox.transform.localPosition = EnemyDeckZone.transform.localPosition;
            messageBox.GetComponentInChildren<Text>().text = e.message;
            yield return new WaitForSeconds(1);
            messageBox.transform.SetParent(null);
            GameObject.Destroy(messageBox);
            inAnimation = false;
        }
        IEnumerator Dialog()
        {
            animations.Add(Animation());
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void PerformLooking(object sender, CardEventArgs e)
    {
        if (isPlayerAction(e.playerID))
            return;
        IEnumerator Animation()
        {
            GameObject messageBox = GameObject.Instantiate(Globals.Instance.deckMessageBoxPrefab);
            messageBox.transform.position = Globals.Instance.ResetPosition;
            messageBox.transform.SetParent(Field.transform);
            messageBox.transform.localPosition = EnemyDeckZone.transform.localPosition;
            messageBox.GetComponentInChildren<Text>().text = e.message;
            yield return new WaitForSeconds(1);
            messageBox.transform.SetParent(null);
            GameObject.Destroy(messageBox);
            inAnimation = false;
        }
        IEnumerator Dialog()
        {
            animations.Add(Animation());
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void PerformReveal(object sender, CardEventArgs e)
    {
        IEnumerator Dialog()
        {
            PerformReveal(e.card.tempID, e.card.id, e.card.originalOwner, e.currentLocation.Item1);
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void PerformReveal(int tempID, string cardID, int playerID, int location)
    {
        animations.Add(Reveal(tempID, cardID, playerID, location));
    }

    IEnumerator Reveal(int tempID, string cardID, int playerID, int location)
    {
        if (GameObject.Find("EnemyHand").GetComponent<Hand>().IsInHand(tempID))
        {
            CardBehavior revealedCard = GameObject.Find(tempID.ToString()).GetComponent<CardBehavior>();
            revealedCard.card = LookUpCard(cardID);
            revealedCard.inAnimation = true;
            revealedCard.faceup = false;
            StartCoroutine(revealedCard.Flip());
            while (revealedCard.inAnimation)
                yield return null;
            revealedCard.inAnimation = true;
            StartCoroutine(revealedCard.Flash(Color.white));
            while (revealedCard.inAnimation)
                yield return null;
            revealedCard.inAnimation = true;
            StartCoroutine(revealedCard.Flip());
            while (revealedCard.inAnimation)
                yield return null;
        }
        else if (GameObject.Find("PlayerHand").GetComponent<Hand>().IsInHand(tempID))
        {
            CardBehavior revealedCard = GameObject.Find(tempID.ToString()).GetComponent<CardBehavior>();
            revealedCard.card = LookUpCard(cardID);
            revealedCard.inAnimation = true;
            StartCoroutine(revealedCard.Flash(Color.white));
            while (revealedCard.inAnimation)
                yield return null;
        }
        else if (location == Location.Deck || location == Location.RideDeck)
        {
            CardBehavior revealedCard = CreateNewCard(cardID, tempID).GetComponent<CardBehavior>();
            if (isPlayerAction(playerID))
            {
                if (location == Location.RideDeck)
                    revealedCard.transform.SetParent(GameObject.Find("PlayerRideDeck").transform);
                else
                    revealedCard.transform.SetParent(GameObject.Find("PlayerDeck").transform);
            }
            else
            {
                if (location == Location.RideDeck)
                    revealedCard.transform.SetParent(GameObject.Find("EnemyRideDeck").transform);
                else
                    revealedCard.transform.SetParent(GameObject.Find("EnemyDeck").transform);
            }
            revealedCard.transform.localPosition = Vector3.zero;
            revealedCard.inAnimation = true;
            yield return null;
            StartCoroutine(revealedCard.Flash(Color.white));
            while (revealedCard.inAnimation)
                yield return null;
            revealedCard.transform.SetParent(null);
            GameObject.Destroy(revealedCard.gameObject);
        }
        inAnimation = false;
    }

    public void PerformSetPrison(object sender, CardEventArgs e)
    {
        IEnumerator Dialog()
        {
            SetPrison(e.playerID);
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void SetPrison(int playerID)
    {
        IEnumerator Dialog()
        {
            if (isPlayerAction(playerID))
                Globals.Instance.playerMiscStats.SetPrison();
            else
                Globals.Instance.enemyMiscStats.SetPrison();
            inAnimation = false;
            yield return null;
        }
        animations.Add(Dialog());
    }

    public void PerformImprison(object sender, CardEventArgs e)
    {
        IEnumerator Dialog()
        {
            PerformImprison(e.playerID);
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void PerformImprison(int playerID)
    {
        IEnumerator Dialog()
        {
            if (isPlayerAction(playerID))
                Globals.Instance.playerMiscStats.Imprison();
            else
                Globals.Instance.enemyMiscStats.Imprison();
            inAnimation = false;
            yield return null;
        }
        animations.Add(Dialog());
    }

    public void PerformFree(object sender, CardEventArgs e)
    {
        IEnumerator Dialog()
        {
            PerformFree(e.playerID);
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void PerformFree(int playerID)
    {
        IEnumerator Dialog()
        {
            if (isPlayerAction(playerID))
                Globals.Instance.playerMiscStats.Free();
            else
                Globals.Instance.enemyMiscStats.Free();
            inAnimation = false;
            yield return null;
        }
        animations.Add(Dialog());
    }

    public void PerformChosen(object sender, CardEventArgs e)
    {
        IEnumerator Dialog()
        {
            PerformChosen(e.card.id, e.card.tempID);
            yield return null;
        }
        RpcCalls.Add(Dialog());
    }

    public void PerformChosen(string cardID, int tempID)
    {
        if (cardID == null)
        {
            Debug.Log("error with cardID");
            return;
        }
        Debug.Log(tempID + " chosen");
        animations.Add(Flash(cardID, tempID));
    }

    public static Sprite LoadSprite(string filename)
    {
        byte[] bytes;
        if (System.IO.File.Exists(filename))
            bytes = System.IO.File.ReadAllBytes(filename);
        else
            bytes = System.IO.File.ReadAllBytes(Application.dataPath + "/../cardart/FaceDownCard.jpg");
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(bytes);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        return sprite;
    }

    void Update()
    {
        
    }

    public Card LookUpCard(string cardID)
    {
        List<Card> card;
        List<string> _cardID;
        //Debug.Log("looking up " + cardID + "...");
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

    public void DisplayCard(string cardID, int tempID)
    {
        GameObject ZoomIn = GameObject.Find("ZoomIn");
        Text CardName = GameObject.Find("CardName").GetComponent<Text>();
        Text CardEffect = GameObject.Find("CardEffect").GetComponent<Text>();
        ZoomIn.GetComponent<Image>().sprite = LoadSprite(FixFileName(cardID));
        Card card = LookUpCard(cardID);
        CardName.text = card.name;
        string effect = "[Power: " + card.power + "] [Shield: " + card.shield + "] [Grade: ";
        if (_recordedCardValues.ContainsKey(tempID))
        {
            effect += _recordedCardValues[tempID].currentGrade;
        }
        else
        {
            //Debug.Log("recordedCardValues does not contain tempID " + tempID);
            effect += card.OriginalGrade();
        }
        effect += "]\n" + card.effect;
        CardEffect.text = effect;
        GameObject.Find("CardEffectScrollbar").GetComponent<Scrollbar>().value = 1;
    }

    public bool isPlayerAction(int playerID)
    {
        return _playerID == playerID;
        //if (playerID == 1)
        //{
        //    if (playerID == 1)
        //        return true;
        //    return false;
        //}
        //else
        //{
        //    if (playerID == 2)
        //        return true;
        //    return false;
        //}
    }

    public readonly struct CardData
    {
        public readonly int _tempID;
        public readonly string _cardID;

        public CardData(int tempID, string cardID)
        {
            _tempID = tempID;
            _cardID = cardID;
        }
    }
}
