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
    bool inAnimation = false;
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
        PhaseManager = GameObject.Find("PhaseManager");
        POW = GameObject.Find("POW");
        SLD = GameObject.Find("SLD");
        cardDict = new Dictionary<string, Card>();
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        cardPrefab = playerManager.cardPrefab;
        SQLpath = "Data Source=" + Application.dataPath + "/../cards.db;Version=3;";
        StartCoroutine(AnimateAnimations());
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
            playerManager.CmdInitialize(LoadCards.GenerateList(Application.dataPath + "/../dsd02.txt", LoadCode.WithRideDeck), 2);
        }
    }

    IEnumerator AnimateAnimations()
    {
        while (true)
        {
            if (animations.Count > 0)
            {
                inAnimation = true;
                StartCoroutine(animations[0]);
                while (inAnimation)
                {
                    //Debug.Log("inanimation");
                    yield return null;
                }
                animations.RemoveAt(0);
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
        cardFight.Initialize(player1_generatedDeck, player2_generatedDeck, tokens, inputManager.inputManager, Application.dataPath + "/../lua");
        //cardFight._player1.OnRideFromRideDeck += PerformRideFromRideDeck;
        //cardFight._player2.OnRideFromRideDeck += PerformRideFromRideDeck;
        cardFight._player1.OnStandUpVanguard += PerformStandUpVanguard;
        cardFight._player1.OnZoneChanged += ChangeZone;
        cardFight._player1.OnZoneSwapped += SwapZone;
        cardFight._player1.OnUpRightChanged += ChangeUpRight;
        cardFight.OnDrawPhase += PerformDrawPhase;
        cardFight.OnStandPhase += PerformStandPhase;
        cardFight.OnRidePhase += PerformRidePhase;
        cardFight.OnMainPhase += PerformMainPhase;
        cardFight.OnBattlePhase += PerformBattlePhase;
        cardFight._player1.OnAttack += PerformAttack;
        cardFight._player2.OnAttack += PerformAttack;
        cardFight._player1.OnShieldValueChanged += ChangeShieldValue;
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
            UnitSlots.GetComponent<UnitSlots>().Initialize(1);
        }
        else
        {
            Debug.Log("client setting up decks");
            PlayerDeckZone.GetComponent<Pile>().UpdateCount(player2_count);
            EnemyDeckZone.GetComponent<Pile>().UpdateCount(player1_count);
            PlayerRideDeckZone.GetComponent<Pile>().UpdateCount(3);
            EnemyRideDeckZone.GetComponent<Pile>().UpdateCount(3);
            UnitSlots.GetComponent<UnitSlots>().Initialize(2);
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
            inputManager.EnemyVG.GetComponent<UnitSlotBehavior>().AddCard(card2.grade, 0, card2.critical, card2.power, true, false, cardID2, Card2);
        }
        else
        {
            inputManager.PlayerVG.GetComponent<UnitSlotBehavior>().AddCard(card2.grade, 0, card2.critical, card2.power, true, false, cardID2, Card2);
            inputManager.EnemyVG.GetComponent<UnitSlotBehavior>().AddCard(card1.grade, 0, card1.critical, card1.power, true, false, cardID1, Card1);
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
            card = cardFight._player1.GetUnitAt(e.currentLocation.Item2);
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
        PlayerHand.GetComponent<PlayerHand>().Reset();
        GameObject previousZone = null;
        GameObject currentZone = null;
        GameObject zone = null;
        GameObject newCard = null;
        int location = previousLocation;
        int FL = previousFL;
        for (int i = 0; i < 2; i++)
        {
            if (i == 1)
            {
                location = currentLocation;
                FL = currentFL;
            }
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
        inputManager.cardsAreHoverable = false;
        if (currentZone == null || previousZone == null)
            yield break;

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
        else if (previousZone == Globals.Instance.guardianCircle.gameObject)
        {
            newCard = Globals.Instance.guardianCircle.RemoveCard(card.tempID);
        }
        else
        {
            newCard = CreateNewCard(card.id, card.tempID);
            newCard.transform.SetParent(Field.transform);
            newCard.transform.position = previousZone.transform.position;
        }

        if (previousZone.GetComponent<UnitSlotBehavior>() != null)
        {
            previousZone.GetComponent<UnitSlotBehavior>().RemoveCard(card.id);
        }

        
        if (currentZone == EnemyDeckZone || currentZone == EnemyHand)
            newCard.GetComponent<Image>().sprite = LoadSprite(Application.dataPath + "/../cardart/FaceDownCard.jpg");
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
        else if (currentZone.GetComponent<UnitSlotBehavior>() != null)
        {
            currentZone.GetComponent<UnitSlotBehavior>().AddCard(card.grade, soul, critical, card.power, true, true, card.id, newCard);
        }
        else if (currentZone.GetComponent<GuardianCircle>() != null)
            currentZone.GetComponent<GuardianCircle>().AddCard(newCard, card.tempID);
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

    public void SwapZone(object sender, CardEventArgs e)
    {
        Debug.Log("swapping zones");
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
            StartCoroutine(unit.Flash());
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
            UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(circle).GetComponent<UnitSlotBehavior>()._shield = shield;
        else
            Debug.Log("invalid circle: " + circle);
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
