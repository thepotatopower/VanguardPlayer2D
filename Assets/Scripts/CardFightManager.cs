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
    public bool inAnimation = false;
    public List<IEnumerator> animations = new List<IEnumerator>();

    [SyncVar]
    public int counter;
    public SyncList<string> player1_deck = new SyncList<string>();
    public SyncList<string> player2_deck = new SyncList<string>();
    [SyncVar]
    public NetworkIdentity host;
    [SyncVar]
    public NetworkIdentity remote;

    public int _attacker = -1;
    public List<int> _attacked;
    public int _booster;

    public void Start()
    {
        this.name = "CardFightManager";
        Globals.Instance.cardFightManager = this;
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
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        cardPrefab = playerManager.cardPrefab;
        SQLpath = "Data Source=" + Application.dataPath + "/../cards.db;Version=3;";
        StartCoroutine(AnimateAnimations());
        string deckPath = GameObject.Find("InputField").GetComponent<InputField>().text;
        Debug.Log(GameObject.Find("InputField").GetComponent<InputField>().text);
        Debug.Log("deckPath: " + deckPath);
        if (!System.IO.File.Exists(Application.dataPath + "/../" + deckPath))
            deckPath = "eugene.txt";
        if (isServer)
        {
            Debug.Log("this is server");
            host = networkIdentity;
            playerManager.CmdInitialize(LoadCards.GenerateList(Application.dataPath + "/../" + deckPath, LoadCode.WithRideDeck), 1);
        }
        else
        {
            Debug.Log("this is client");
            remote = networkIdentity;
            playerManager.CmdInitialize(LoadCards.GenerateList(Application.dataPath + "/../" + deckPath, LoadCode.WithRideDeck), 2);
        }
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

    public void InitializeCardFight()
    {
        List<Card> player1_generatedDeck = LoadCards.GenerateCardsFromList(SyncListToList(player1_deck), SQLpath);
        List<Card> player2_generatedDeck = LoadCards.GenerateCardsFromList(SyncListToList(player2_deck), SQLpath);
        List<Card> tokens = LoadCards.GenerateCardsFromList(LoadCards.GenerateList(Application.dataPath + "/../tokens.txt", LoadCode.Tokens), SQLpath);
        Debug.Log("player1 count: " + player1_generatedDeck.Count);
        Debug.Log("player2 count: " + player2_generatedDeck.Count);
        inputManager.InitializeInputManager();
        cardFight = new VanguardEngine.CardFight();
        string luaPath = Application.dataPath + "/../lua";
        //C:/Users/Jason/Desktop/VanguardEngine/VanguardEngine/lua
        cardFight.Initialize(player1_generatedDeck, player2_generatedDeck, tokens, inputManager.inputManager, "C:/Users/Jason/Desktop/VanguardEngine/VanguardEngine/lua");
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
        cardFight._player1.OnCardValueChanged += ChangeCardValue;
        cardFight._player1.OnAttackHits += CheckIfAttackHits;
        cardFight._player2.OnAttackHits += CheckIfAttackHits;
        cardFight._player1.OnReveal += PerformReveal;
        cardFight._player2.OnReveal += PerformReveal;
        cardFight._player1.OnSetPrison += PerformSetPrison;
        cardFight._player2.OnSetPrison += PerformSetPrison;
        cardFight._player1.OnImprison += PerformImprison;
        cardFight._player2.OnImprison += PerformImprison;
        inputManager.inputManager.OnChosen += PerformChosen;
        cardFight.OnFree += PerformFree;
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
            PlayerDeckZone.GetComponent<Pile>().InitializeCount(player1_count);
            EnemyDeckZone.GetComponent<Pile>().InitializeCount(player2_count);
            PlayerRideDeckZone.GetComponent<Pile>().InitializeCount(3);
            EnemyRideDeckZone.GetComponent<Pile>().InitializeCount(3);
            UnitSlots.GetComponent<UnitSlots>().Initialize(1);
        }
        else
        {
            Debug.Log("client setting up decks");
            PlayerDeckZone.GetComponent<Pile>().InitializeCount(player2_count);
            EnemyDeckZone.GetComponent<Pile>().InitializeCount(player1_count);
            PlayerRideDeckZone.GetComponent<Pile>().InitializeCount(3);
            EnemyRideDeckZone.GetComponent<Pile>().InitializeCount(3);
            UnitSlots.GetComponent<UnitSlots>().Initialize(2);
        }
        PlayerDropZone.GetComponent<Pile>().InitializeCount(0);
        EnemyDropZone.GetComponent<Pile>().InitializeCount(0);
        Globals.Instance.playerOrderZone.GetComponent<Pile>().InitializeCount(0);
        Globals.Instance.enemyOrderZone.GetComponent<Pile>().InitializeCount(0);
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
            inputManager.PlayerVG.GetComponent<UnitSlotBehavior>().AddCard(card1.grade, card1.critical, card1.power, card1.power, true, false, cardID1, Card1);
            inputManager.EnemyVG.GetComponent<UnitSlotBehavior>().AddCard(card2.grade, card2.critical, card2.power, card2.power, true, false, cardID2, Card2);
        }
        else
        {
            inputManager.PlayerVG.GetComponent<UnitSlotBehavior>().AddCard(card2.grade, card2.critical, card2.power, card2.power, true, false, cardID2, Card2);
            inputManager.EnemyVG.GetComponent<UnitSlotBehavior>().AddCard(card1.grade, card1.critical, card1.power, card1.power, true, false, cardID1, Card1);
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
                grade = card.grade;
                soul = cardFight._player1.GetSoul().Count;
                critical = card.critical;
                faceup = true;
                upright = true;
            }
        }
        if (e.previousLocation == null)
            Debug.Log("error with previouslocation");
        if (e.currentLocation == null)
            Debug.Log("error with currentlocation");
        RpcChangeZone(e.previousLocation.Item1, e.previousLocation.Item2, e.currentLocation.Item1, e.currentLocation.Item2, e.card, grade, soul, critical, faceup, upright);
    }

    [ClientRpc]
    public void RpcChangeZone(int previousLocation, int previousFL, int currentLocation, int currentFL, Card card, int grade, int soul, int critical, bool faceup, bool upright)
    {
        animations.Add(ChangeZoneRoutine(previousLocation, previousFL, currentLocation, currentFL, card, grade, soul, critical, faceup, upright));
    }

    IEnumerator ChangeZoneRoutine(int previousLocation, int previousFL, int currentLocation, int currentFL, Card card, int grade, int soul, int critical, bool faceup, bool upright)
    {
        Debug.Log("changing zone");
        inAnimation = true;
        PlayerHand.GetComponent<Hand>().Reset();
        GameObject previousZone = null;
        GameObject currentZone = null;
        GameObject zone = null;
        GameObject newCard = null;
        int location = previousLocation;
        int FL = previousFL;
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
                Debug.Log("drop here");
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
            else if (location == Location.VC || location == Location.RC)
            {
                Debug.Log("vc/rc here");
                zone = UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(FL);
                Debug.Log("FL: " + FL);
                if (zone == null)
                    Debug.Log("no unit found");
            }
            else if (location == Location.GC)
            {
                Debug.Log("gc here");
                zone = Globals.Instance.guardianCircle.gameObject;
            }
            else if (location == Location.Trigger)
            {
                Debug.Log("trigger here");
                if (isPlayerAction(card.originalOwner))
                    zone = Globals.Instance.playerTriggerZone;
                else
                    zone = Globals.Instance.enemyTriggerZone;
            }
            else if (location == Location.Damage)
            {
                Debug.Log("damage here");
                if (isPlayerAction(card.originalOwner))
                    zone = Globals.Instance.playerDamageZone;
                else
                    zone = Globals.Instance.enemyDamageZone;
            }
            else if (location == Location.Soul)
            {
                Debug.Log("soul here");
                zone = Globals.Instance.unitSlots.GetUnitSlot(FL);
            }
            else if (location == Location.OrderArea)
            {
                Debug.Log("order area here");
                zone = Globals.Instance.orderArea;
            }
            else if (location == Location.Order)
            {
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
            else if (location == -1)
            {
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

        if (previousLocation == Location.Soul)
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
        if (currentLocation == Location.Soul)
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
            newCard.transform.position = Vector3.MoveTowards(newCard.transform.position, blankCard.transform.position, step);
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
        else if (currentZone.GetComponent<UnitSlotBehavior>() != null && currentLocation != Location.Soul)
        {
            currentZone.GetComponent<UnitSlotBehavior>().AddCard(card.grade, critical, card.power, card.power, true, true, card.id, newCard);
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
            currentZone.GetComponent<Pile>().AddCard(card);

        inputManager.cardsAreHoverable = true;
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
        RpcSwapZone(e.previousLocation.Item2, e.currentLocation.Item2);
    }

    [ClientRpc]
    public void RpcSwapZone(int previousFL, int currentFL)
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
            GameObject newCard = CreateNewCard(cardID, -1);
            newCard.transform.SetParent(Field.transform);
            newCard.transform.position = previousObject.transform.position;
            if (previousObject.transform.name.Contains("Enemy"))
                newCard.transform.Rotate(new Vector3(0, 0, 180));
            float step = 2000 * Time.deltaTime;
            while (Vector3.Distance(newCard.transform.position, nextObject.transform.position) > 0.001f)
            {
                newCard.transform.position = Vector3.MoveTowards(newCard.transform.position, nextObject.transform.position, step);
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
        RpcShowAbilityActivated(e.card);
    }

    [ClientRpc]
    public void RpcShowAbilityActivated(Card card)
    {
        animations.Add(ShowAbilityActivatedRoutine(card));
    }

    IEnumerator ShowAbilityActivatedRoutine(Card card)
    {
        GameObject target = GameObject.Find(card.tempID.ToString());
        bool abilityBoxAnimation = true;
        bool sliding = false;
        IEnumerator SlideAbilityBox()
        {
            GameObject abilityBox = Globals.Instance.AbilityBox;
            abilityBox.GetComponentInChildren<Text>().text = card.name + "'s ability activates!";
            abilityBox.transform.localPosition = Globals.Instance.AbilityBoxResetPosition;
            float step = 600 * Time.deltaTime;
            while (Vector3.Distance(abilityBox.transform.localPosition, Globals.Instance.AbilityBoxSlidePosition) > 0.001f)
            {
                abilityBox.transform.localPosition = Vector3.MoveTowards(abilityBox.transform.localPosition, Globals.Instance.AbilityBoxSlidePosition, step);
                yield return null;
            }
            yield return new WaitForSecondsRealtime(1);
            while (Vector3.Distance(abilityBox.transform.localPosition, Globals.Instance.AbilityBoxResetPosition) > 0.001f)
            {
                abilityBox.transform.localPosition = Vector3.MoveTowards(abilityBox.transform.localPosition, Globals.Instance.AbilityBoxResetPosition, step);
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
                    target.GetComponent<CardBehavior>().cardID = card.id;
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
        else if (Globals.Instance.unitSlots.GetUnitSlotWithSoul(card.tempID) != null)
        {
            IEnumerator SlideOutFromSoul()
            {
                GameObject unit = Globals.Instance.unitSlots.GetUnitSlotWithSoul(card.tempID);
                GameObject newCard = CreateNewCard(card.id, -1);
                newCard.transform.SetParent(GameObject.Find("MainCanvas").transform);
                newCard.transform.SetSiblingIndex(unit.transform.GetSiblingIndex() - 1);
                newCard.transform.localPosition = unit.transform.localPosition;
                float step = 600 * Time.deltaTime;
                Vector3 slidePosition = new Vector3(newCard.transform.localPosition.x + 75, newCard.transform.localPosition.y, 0);
                while (Vector3.Distance(newCard.transform.localPosition, slidePosition) > 0.001f)
                {
                    newCard.transform.localPosition = Vector3.MoveTowards(newCard.transform.localPosition, slidePosition, step);
                    yield return null;
                }
                newCard.GetComponent<CardBehavior>().inAnimation = true;
                StartCoroutine(newCard.GetComponent<CardBehavior>().Flash(Color.yellow));
                while (newCard.GetComponent<CardBehavior>().inAnimation)
                    yield return null;
                slidePosition = new Vector3(newCard.transform.localPosition.x - 75, newCard.transform.localPosition.y, 0);
                while (Vector3.Distance(newCard.transform.localPosition, slidePosition) > 0.001f)
                {
                    newCard.transform.localPosition = Vector3.MoveTowards(newCard.transform.localPosition, slidePosition, step);
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
        RpcChangePhase(Phase.Draw, cardFight.actingPlayer._playerID, cardFight._turn);
    }

    public void PerformStandPhase(object sender, CardEventArgs e)
    {
        Debug.Log("stand phase");
        RpcChangePhase(Phase.Stand, cardFight.actingPlayer._playerID, cardFight._turn);
    }

    public void PerformRidePhase(object sender, CardEventArgs e)
    {
        Debug.Log("ride phase");
        if (isPlayerAction(cardFight.actingPlayer._playerID))
            Debug.Log("my turn");
        else
            Debug.Log("their turn");
        RpcChangePhase(Phase.Ride, cardFight.actingPlayer._playerID, cardFight._turn);
    }

    public void PerformMainPhase(object sender, CardEventArgs e)
    {
        Debug.Log("main phase");
        RpcChangePhase(Phase.Main, cardFight.actingPlayer._playerID, cardFight._turn); 
    }

    public void PerformBattlePhase(object sender, CardEventArgs e)
    {
        Debug.Log("battle phase");
        RpcChangePhase(Phase.Battle, cardFight.actingPlayer._playerID, cardFight._turn);
    }

    [ClientRpc]
    public void RpcChangePhase(int phase, int actingPlayer, int turn)
    {
        if ((actingPlayer == 1 && isServer) || (actingPlayer == 2 && !isServer))
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
        RpcPerformAttack(player.GetCircle(player.GetAttacker()), player.GetCircle(player.GetAttackedCards()[0]), booster);
    }

    [ClientRpc]
    public void RpcPerformAttack(int attackingCircle, int attackedCircle, int booster)
    {
        _attacked.Clear();
        _attacker = attackingCircle;
        _attacked.Add(attackedCircle);
        _booster = booster;
        animations.Add(FlashUnit(attackedCircle));
        animations.Add(ShowAttack());
    }

    IEnumerator ShowAttack()
    {
        UnitSlots.GetComponent<UnitSlots>().PerformAttack(_attacker, _attacked[0]);
        POW.GetComponent<POWSLD>().compare = true;
        SLD.GetComponent<POWSLD>().compare = true;
        POW.GetComponent<POWSLD>().auto = true;
        SLD.GetComponent<POWSLD>().auto = true;
        inAnimation = false;
        yield return null;
    }

    IEnumerator FlashUnit(int circle)
    {
        UnitSelectArea unit;
        if (UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(circle) != null)
        {
            unit = UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(circle).GetComponentInChildren<UnitSelectArea>();
            unit.inAnimation = true;
            StartCoroutine(unit.Flash(Color.white));
            while (unit.inAnimation)
                yield return null;
            inAnimation = false;
        }
    }

    public void ChangeUpRight(object sender, CardEventArgs e)
    {
        Debug.Log("rotating");
        Player player = sender as Player;
        RpcChangeUpRight(player.GetCircle(player.GetCard(e.i)), e.upright);
    }

    [ClientRpc]
    public void RpcChangeUpRight(int circle, bool upright)
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
        Player player = sender as Player;
        RpcChangeFaceUp(e.i, e.faceup);
    }

    [ClientRpc]
    public void RpcChangeFaceUp(int tempID, bool faceup)
    {
        animations.Add(Flip(tempID, faceup));
    }

    IEnumerator Flip(int tempID, bool faceup)
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
        inAnimation = false;
    }

    public void ChangeShieldValue(object sender, CardEventArgs e)
    {
        Player player = sender as Player;
        Debug.Log("updating shield value: " + e.currentShield);
        RpcChangeShieldValue(e.circle, e.currentShield);
    }

    [ClientRpc]
    public void RpcChangeShieldValue(int circle, int shield)
    {
        if (UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(circle) != null)
            animations.Add(UpdateShieldValue(circle, shield));
        else
            Debug.Log("invalid circle: " + circle);
    }

    IEnumerator UpdateShieldValue(int circle, int shield)
    {
        UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(circle).GetComponent<UnitSlotBehavior>()._shield = shield;
        inAnimation = false;
        yield return null;
    }

    public void ChangeCardValue(object sender, CardEventArgs e)
    {
        Player player = sender as Player;
        Debug.Log("updating power value: " + e.currentPower);
        RpcChangeCardValue(e.circle, e.currentPower, e.currentCritical);
    }

    [ClientRpc]
    public void RpcChangeCardValue(int circle, int power, int critical)
    {
        if (UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(circle) != null)
            animations.Add(UpdateCardValue(circle, power, critical));
        else
            Debug.Log("invalid circle: " + circle);
    }

    IEnumerator UpdateCardValue(int circle, int power, int critical)
    {
        UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(circle).GetComponent<UnitSlotBehavior>()._power = power;
        UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(circle).GetComponent<UnitSlotBehavior>()._critical = critical;
        inAnimation = false;
        yield return null;
    }

    public void CheckIfAttackHits(object sender, CardEventArgs e)
    {
        RpcCheckIfAttackHits();
    }

    [ClientRpc]
    public void RpcCheckIfAttackHits()
    {
        animations.Add(CheckIfAttackHitsAnimation());
    }

    IEnumerator CheckIfAttackHitsAnimation()
    {
        UnitSlots.GetComponent<UnitSlots>().EndAttack();
        POW.GetComponent<POWSLD>().Reset();
        SLD.GetComponent<POWSLD>().Reset();
        inAnimation = false;
        yield return null;
    }

    public void PerformReveal(object sender, CardEventArgs e)
    {
        RpcPerformReveal(e.card, e.currentLocation.Item1);
    }

    [ClientRpc]
    public void RpcPerformReveal(Card card, int location)
    {
        animations.Add(Reveal(card, location));
    }

    IEnumerator Reveal(Card card, int location)
    {
        if (GameObject.Find("EnemyHand").GetComponent<Hand>().IsInHand(card.tempID))
        {
            CardBehavior revealedCard = GameObject.Find(card.tempID.ToString()).GetComponent<CardBehavior>();
            revealedCard.cardID = card.id;
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
        else if (GameObject.Find("PlayerHand").GetComponent<Hand>().IsInHand(card.tempID))
        {
            CardBehavior revealedCard = GameObject.Find(card.tempID.ToString()).GetComponent<CardBehavior>();
            revealedCard.cardID = card.id;
            revealedCard.inAnimation = true;
            StartCoroutine(revealedCard.Flash(Color.white));
            while (revealedCard.inAnimation)
                yield return null;
        }
        else if (location == Location.Deck)
        {
            CardBehavior revealedCard = CreateNewCard(card.id, card.tempID).GetComponent<CardBehavior>();
            if (isPlayerAction(card.originalOwner))
                revealedCard.transform.SetParent(GameObject.Find("PlayerDeck").transform);
            else
                revealedCard.transform.SetParent(GameObject.Find("EnemyDeck").transform);
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
        RpcSetPrison(e.playerID);
    }

    [ClientRpc]
    public void RpcSetPrison(int playerID)
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
        RpcPerformImprison(e.playerID);
    }

    [ClientRpc]
    public void RpcPerformImprison(int playerID)
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
        RpcPerformFree(e.playerID);
    }

    [ClientRpc]
    public void RpcPerformFree(int playerID)
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
        RpcPerformChosen(e.card.tempID);
    }

    [ClientRpc]
    public void RpcPerformChosen(int tempID)
    {
        UnitSlotBehavior unitSlot = Globals.Instance.unitSlots.FindUnitSlot(tempID);
        if (unitSlot != null)
            animations.Add(FlashUnit(unitSlot._FL));
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
            Debug.Log(filename + " doesn't exist.");
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
