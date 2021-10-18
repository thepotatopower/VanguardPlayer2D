using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VanguardEngine;
using UnityEngine.UI;
using Mirror;
using System.Threading;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

public class VisualInputManager : NetworkBehaviour
{ 
    public Button rockButton;
    public Button scissorsButton;
    public Button paperButton;
    public Button mulliganButton;
    public Button yesButton;
    public Button noButton;
    public Button toggle;
    public Button cardButton;
    public Button cardButton2;
    public Button rideFromRideDeck;
    public Button cancelButton;
    public Button viewButton;
    public GameObject PlayerBackRight;
    public GameObject PlayerBackMiddle;
    public GameObject PlayerBackLeft;
    public GameObject PlayerFrontRight;
    public GameObject PlayerVG;
    public GameObject PlayerFrontLeft;
    public GameObject EnemyFrontLeft;
    public GameObject EnemyVG;
    public GameObject EnemyBackLeft;
    public GameObject EnemyBackMiddle;
    public GameObject EnemyFrontRight;
    public GameObject EnemyBackRight;
    public GameObject messageBox;
    public GameObject PlayerHand;
    public GameObject PhaseManager;
    public GameObject UnitSlots;
    public GameObject POW;
    public GameObject SLD;
    public GameObject Buttons;
    public CardSelect cardSelect;
    public CardSelect cardViewer;
    public IM inputManager;
    public Thread currentThread;
    public PlayerManager playerManager;
    public CardFightManager cardFightManager;
    [SyncVar]
    public int count = 0;
    [SyncVar]
    public int player1_input = -1;
    [SyncVar]
    public int player2_input = -1;
    //[SyncVar(hook = nameof(OnInputSignalChanged))]
    public int inputSignal = -1;
    [SyncVar]
    public int numResponses = 0;
    [SyncVar]
    public int input1 = -1;
    [SyncVar]
    public int input2 = -1;
    [SyncVar]
    public int input3 = -1;
    [SyncVar]
    public bool readyToContinue = false;
    [SyncVar]
    public int min = 0;
    [SyncVar]
    public string query = "";
    [SyncVar]
    public bool bool1 = false;
    [SyncVar]
    public int int1 = 0;
    [SyncVar]
    public int int2 = 0;
    [SyncVar]
    public string string1 = "";
    [SyncVar]
    public int actingPlayer = 1;
    public SyncList<int> inputs = new SyncList<int>();
    public SyncList<int> tempIDs = new SyncList<int>();
    public SyncList<int> tempIDs2 = new SyncList<int>();
    public SyncList<string> cardIDs = new SyncList<string>();
    public SyncList<string> cardIDs2 = new SyncList<string>();
    public SyncList<string> strings = new SyncList<string>();
    public SyncList<bool> faceup = new SyncList<bool>();
    public SyncList<bool> upright = new SyncList<bool>();
    public SyncList<bool> bools = new SyncList<bool>();
    public SyncList<int> ints = new SyncList<int>();
    public bool receivedInput = false;
    public bool cardsAreSelectable = false;
    public bool cardsAreHoverable = true;
    public List<int> selectedCards;
    public UnityEvent m_myEvent;
    public EventSystem eventSystem;
    int selectedCard = -1;
    int selectedUnit = -1;
    GameObject selectedGameObject = null;
    List<Button> miscellaneousButtons;
    bool clicked = false;
    bool browsingField = false;
    Vector3 currentMessageBoxPosition;
    Vector3 currentYesPosition;
    Vector3 currentNoPosition;
    Vector3 currentCardSelectPosition;
    Vector3 currentSelectionButton1Position;
    Vector3 currentSelectionButton2Position;
    Vector3 currentCallCardPosition;
    WaitForUIButtons waitForButton;
    IEnumerator currentRoutine = null;

    //VanguardEngine's InputManager logic
    public class IM : InputManager
    {
        public VisualInputManager inputManager;
        public int inputSignal = 0;
        public bool inputActive = false;
        public bool reversed = false;
        public int _count = 0;
        public int _min = 0;
        public List<int> _tempIDs = new List<int>();
        public List<int> _tempIDs2 = new List<int>();
        public List<string> _cardIDs = new List<string>();
        public List<string> _cardIDs2 = new List<string>();
        public List<string> _strings = new List<string>();
        public List<bool> _upright = new List<bool>();
        public List<bool> _faceup = new List<bool>();
        public List<bool> _bools = new List<bool>();
        public string string_value;

        protected override void RPS_Input()
        {
            Debug.Log("RPS_Input started");
            if (inputManager.player1_input >= 0 && inputManager.player2_input >= 0)
            {
                int_input = inputManager.player2_input;
                Debug.Log("second RPS input: " + int_input.ToString());
                inputSignal = InputType.ResolveRPS;
                while (!inputManager.readyToContinue) ;
                inputManager.readyToContinue = false;
                oSignalEvent.Set();
            }
            else
            {
                inputSignal = InputType.RPS;
                Debug.Log(inputManager.readyToContinue);
                while (!inputManager.readyToContinue) ;
                inputManager.readyToContinue = false;
                int_input = inputManager.player1_input;
                Debug.Log("first RPS input: " + int_input.ToString());
                oSignalEvent.Set();
            }
        }

        protected override void YesNo_Input()
        {
            Thread.Sleep(250);
            while (inputActive) ;
            inputActive = true;
            inputSignal = InputType.YesNo;
            _query = string_input;
            if (string_input == "Boost?")
                int_value = _actingPlayer.GetBooster(_actingPlayer.GetAttacker().tempID);
            while (!inputManager.readyToContinue) ;
            inputManager.readyToContinue = false;
            if (inputManager.player1_input == 0)
                bool_input = true;
            else
                bool_input = false;
            Debug.Log("selection made: " + int_input);
            inputSignal = InputType.Reset;
            while (!inputManager.readyToContinue) ;
            inputManager.readyToContinue = false;
            inputActive = false;
        }

        protected override void SelectCardsToMulligan_Input()
        {
            Thread.Sleep(250); //need to refine this later
            while (inputActive) ;
            inputActive = true;
            intlist_input.Clear();
            inputSignal = InputType.Mulligan;
            while (!inputManager.readyToContinue) ;
            inputManager.readyToContinue = false;
            foreach (int input in inputManager.inputs)
                intlist_input.Add(input);
            inputManager.inputs.Clear();
            inputSignal = InputType.Reset;
            while (!inputManager.readyToContinue) ;
            inputManager.readyToContinue = false;
            inputActive = false;
        }

        protected override void SelectFromList_Input()
        {
            Thread.Sleep(250);
            string location;
            while (inputActive) ;
            inputActive = true;
            _count = int_value;
            _min = int_value2;
            _cardIDs.Clear();
            _tempIDs.Clear();
            _strings.Clear();
            _faceup.Clear();
            _upright.Clear();
            _bools.Clear();
            intlist_input.Clear();
            foreach (Card card in cardsToSelect)
            {
                _tempIDs.Add(card.tempID);
                _cardIDs.Add(card.id);
                location = "<>";
                //Debug.Log(_actingPlayer.GetLocation(card));
                switch (_actingPlayer.GetLocation(card))
                {
                    case Location.RC:
                        location = "<RC>";
                        break;
                    case Location.VC:
                        location = "<VC>";
                        break;
                    case Location.Hand:
                        location = "<Hand>";
                        break;
                    case Location.RideDeck:
                        location = "<RideDeck>";
                        break;
                    case Location.Soul:
                        location = "<Soul>";
                        break;
                    case Location.Deck:
                        location = "<Deck>";
                        break;
                }
                _strings.Add(location);
                _faceup.Add(_actingPlayer.IsFaceUp(card));
                _upright.Add(_actingPlayer.IsUpRight(card));
            }
            inputSignal = InputType.SelectFromList;
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void ChooseOrder_Input()
        {
            while (inputActive) ;
            inputActive = true;
            _query = "Choose order for cards.";
            int_value = cardsToSelect.Count;
            int_value2 = cardsToSelect.Count;
            inputActive = false;
            SelectFromList_Input();
        }

        protected override void DisplayCards_Input()
        {
            while (inputActive) ;
            inputActive = true;
            _query = "Press Cancel to finish looking.";
            int_value = 0;
            int_value2 = 0;
            inputActive = false;
            SelectFromList_Input();
        }

        protected override void SelectRidePhaseAction_Input()
        {
            while (inputActive) ;
            inputActive = true;
            _cardIDs.Clear();
            _tempIDs.Clear();
            bool_value = false;
            if (_actingPlayer.CanRideFromHand())
            {
                foreach (Card card in _actingPlayer.GetRideableCards(false))
                {
                    _cardIDs.Add(card.id);
                    _tempIDs.Add(card.tempID);
                }
            }
            if (_actingPlayer.CanRideFromRideDeck())
                bool_value = true;
            Thread.Sleep(250);
            inputSignal = InputType.SelectRidePhaseAction;
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectMainPhaseAction_Input()
        {
            while (inputActive) ;
            inputActive = true;
            _cardIDs.Clear();
            _cardIDs2.Clear();
            _tempIDs.Clear();
            _tempIDs2.Clear();
            bool_value = false;
            int_value = _actingPlayer.Turn;
            if (_actingPlayer.CanCallRearguard())
            {
                foreach (Card card in _actingPlayer.GetCallableRearguards())
                {
                    _cardIDs.Add(card.id);
                    _tempIDs.Add(card.tempID);
                }
            }
            foreach (Ability ability in _abilities) //ACT and Orders
            {
                _cardIDs2.Add(ability.GetCard().id);
                _tempIDs2.Add(ability.GetCard().tempID);
            }
            Thread.Sleep(250);
            inputSignal = InputType.SelectMainPhaseAction;
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectCallLocation_Input()
        {
            Thread.Sleep(250);
            while (inputActive) ;
            inputActive = true;
            bool proceed = false;
            while (!proceed)
            {
                string_value = card_input.id;
                inputSignal = InputType.SelectCallLocation;
                WaitForReadyToContinue();
                if (!_ints.Contains(int_input) && ((_ints2.Count > 0 && _ints2.Contains(int_input)) || _ints2.Count == 0))
                {
                    proceed = true;
                }
            }
            inputActive = false;
        }

        protected override void SelectBattlePhaseAction_Input()
        {
            while (inputActive) ;
            inputActive = true;
            _cardIDs.Clear();
            _tempIDs.Clear();
            bool_value = false;
            if (_actingPlayer.CanAttack())
            {
                foreach (Card card in _actingPlayer.GetCardsToAttackWith())
                {
                    _cardIDs.Add(card.id);
                    _tempIDs.Add(card.tempID);
                }
            }
            Thread.Sleep(250);
            inputSignal = InputType.SelectBattlePhaseAction;
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectUnitToAttack_Input()
        {
            while (inputActive) ;
            inputActive = true;
            _cardIDs.Clear();
            _tempIDs.Clear();
            bool_value = false;
            foreach (Card card in _actingPlayer.GetPotentialAttackTargets())
            {
                _cardIDs.Add(card.id);
                _tempIDs.Add(card.tempID);
            }
            Thread.Sleep(250);
            inputSignal = InputType.SelectUnitToAttack;
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectGuardPhaseAction_Input()
        {
            while (inputActive) ;
            inputActive = true;
            if (_actingPlayer.CanGuard())
                bool_value = true;
            else
                bool_value = false;
            _cardIDs.Clear();
            _tempIDs.Clear();
            _cardIDs2.Clear();
            _tempIDs2.Clear();
            foreach (Card card in _actingPlayer.GetInterceptableCards())
            {
                _cardIDs.Add(card.id);
                _tempIDs.Add(card.tempID);
            }
            foreach (Ability ability in _abilities) // Blitz Orders
            {
                _cardIDs2.Add(ability.GetCard().id);
                _tempIDs2.Add(ability.GetCard().tempID);
            }
            Thread.Sleep(250);
            inputSignal = InputType.SelectGuardStepAction;
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectActiveUnit_Input()
        {
            while (inputActive) ;
            inputActive = true;
            _cardIDs.Clear();
            _tempIDs.Clear();
            bool_value = false;
            _query = "Choose unit to give +";
            _query += value;
            if (prompt == PromptType.AddCritical)
                _query += " critical.";
            else if (prompt == PromptType.AddPower)
                _query += " power.";
            foreach (Card card in _actingPlayer.GetActiveUnits())
            {
                _cardIDs.Add(card.id);
                _tempIDs.Add(card.tempID);
            }
            Thread.Sleep(250);
            inputSignal = InputType.SelectActiveUnit;
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectCardToGuard_Input()
        {
            while (inputActive) ;
            inputActive = true;
            _cardIDs.Clear();
            _tempIDs.Clear();
            bool_value = false;
            _query = "Choose unit to guard.";
            foreach (Card card in _actingPlayer.GetAttackedCards())
            {
                _cardIDs.Add(card.id);
                _tempIDs.Add(card.tempID);
            }
            Thread.Sleep(250);
            inputSignal = InputType.SelectActiveUnit;
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectAbility_Input()
        {
            Thread.Sleep(250);
            string location;
            while (inputActive) ;
            inputActive = true;
            _count = int_value;
            _min = int_value2;
            _cardIDs.Clear();
            _tempIDs.Clear();
            _strings.Clear();
            intlist_input.Clear();
            Debug.Log(_abilities.Count + " abilities on standby.");
            foreach (Ability ability in _abilities)
            {
                _tempIDs.Add(ability.GetCard().tempID);
                _cardIDs.Add(ability.GetCard().id);
                location = "<>";
                //Debug.Log(_actingPlayer.GetLocation(card));
                switch (_actingPlayer.GetLocation(ability.GetCard()))
                {
                    case Location.RC:
                        location = "<RC>";
                        break;
                    case Location.VC:
                        location = "<VC>";
                        break;
                    case Location.Hand:
                        location = "<Hand>";
                        break;
                    case Location.RideDeck:
                        location = "<RideDeck>";
                        break;
                }
                _strings.Add(location);
                _upright.Add(_actingPlayer.IsUpRight(ability.GetCard()));
            }
            if (CheckForMandatoryEffects(_abilities))
                bool_value = true;
            else
                bool_value = false;
            _bools.Clear();
            foreach (Ability ability in _abilities)
            {
                if (ability.isMandatory)
                    _bools.Add(true);
                else
                    _bools.Add(false);
            }
            inputSignal = InputType.SelectAbility;
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectOption_Input()
        {
            Thread.Sleep(250);
            while (inputActive) ;
            inputActive = true;
            _strings.Clear();
            foreach (string option in _list)
            {
                _strings.Add(option);
            }
            inputSignal = InputType.SelectOption;
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectCircle_Input()
        {
            Thread.Sleep(250);
            while (inputActive) ;
            inputActive = true;
            _ints.Clear();
            foreach (int i in intlist_input)
                _ints.Add(i);
            inputSignal = InputType.SelectCircle;
            WaitForReadyToContinue();
            inputActive = false;
        }

        public void WaitForReadyToContinue()
        {
            while (!inputManager.readyToContinue) ;
            inputManager.readyToContinue = false;
            intlist_input.Clear();
            foreach (int input in inputManager.inputs)
                intlist_input.Add(input);
            Debug.Log("received input 1: " + inputManager.input1);
            Debug.Log("received input 2: " + inputManager.input2);
            int_input = inputManager.input1;
            int_input2 = inputManager.input2;
            inputManager.inputs.Clear();
            Thread.Sleep(30 / 1000);
            inputSignal = InputType.Reset;
            while (!inputManager.readyToContinue) ;
            inputManager.readyToContinue = false;
        }
    }

    public override void OnStartServer()
    {
        readyToContinue = false;
    }

    // VanguardPlayer2D's InputManager logic
    void Start()
    {
        miscellaneousButtons = new List<Button>();
        currentThread = Thread.CurrentThread;
        rockButton = GameObject.Find("RockButton").GetComponent<Button>();
        rockButton.transform.position = new Vector3(10000, 0, 0);
        paperButton = GameObject.Find("PaperButton").GetComponent<Button>();
        paperButton.transform.position = new Vector3(10000, 0, 0);
        scissorsButton = GameObject.Find("ScissorsButton").GetComponent<Button>();
        scissorsButton.transform.position = new Vector3(10000, 0, 0);
        messageBox = GameObject.Find("MessageBox");
        messageBox.transform.position = new Vector3(10000, 0, 0);
        currentMessageBoxPosition = messageBox.transform.position;
        mulliganButton = GameObject.Find("Mulligan").GetComponent<Button>();
        mulliganButton.transform.position = new Vector3(10000, 0, 0);
        yesButton = GameObject.Find("YesButton").GetComponent<Button>();
        yesButton.transform.position = new Vector3(10000, 0, 0);
        currentYesPosition = yesButton.transform.position;
        noButton = GameObject.Find("NoButton").GetComponent<Button>();
        noButton.transform.position = new Vector3(10000, 0, 0);
        currentNoPosition = noButton.transform.position;
        currentCardSelectPosition = cardSelect.transform.position;
        currentSelectionButton1Position = Globals.Instance.selectionButton1.transform.position;
        currentSelectionButton2Position = Globals.Instance.selectionButton2.transform.position;
        toggle = GameObject.Find("Toggle").GetComponent<Button>();
        toggle.transform.position = new Vector3(10000, 0, 0);
        cardButton.transform.position = new Vector3(10000, 0, 0);
        rideFromRideDeck.transform.position = new Vector3(10000, 0, 0);
        cancelButton.transform.position = new Vector3(10000, 0, 0);
        POW.transform.position = new Vector3(10000, 0, 0);
        SLD.transform.position = new Vector3(10000, 0, 0);
        currentCallCardPosition = Globals.Instance.callCard.transform.position;
        ResetMiscellaneousButtons();
        cardSelect = GameObject.Find("CardSelect").GetComponent<CardSelect>();
        cardSelect.Hide();
        PlayerHand = GameObject.Find("PlayerHand");
        selectedCards = new List<int>();
        PlayerVG.GetComponent<UnitSlotBehavior>().slot = FL.PlayerVanguard;
        PlayerFrontLeft.GetComponent<UnitSlotBehavior>().slot = FL.PlayerFrontLeft;
        PlayerFrontRight.GetComponent<UnitSlotBehavior>().slot = FL.PlayerFrontRight;
        PlayerBackLeft.GetComponent<UnitSlotBehavior>().slot = FL.PlayerBackLeft;
        PlayerBackMiddle.GetComponent<UnitSlotBehavior>().slot = FL.PlayerBackCenter;
        PlayerBackRight.GetComponent<UnitSlotBehavior>().slot = FL.PlayerBackRight;
        EnemyVG.GetComponent<UnitSlotBehavior>().slot = FL.EnemyVanguard;
        EnemyFrontLeft.GetComponent<UnitSlotBehavior>().slot = FL.EnemyFrontLeft;
        EnemyFrontRight.GetComponent<UnitSlotBehavior>().slot = FL.EnemyFrontRight;
        EnemyBackLeft.GetComponent<UnitSlotBehavior>().slot = FL.EnemyBackLeft;
        EnemyBackMiddle.GetComponent<UnitSlotBehavior>().slot = FL.EnemyBackCenter;
        EnemyBackRight.GetComponent<UnitSlotBehavior>().slot = FL.EnemyBackRight;
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer && Thread.CurrentThread == currentThread && inputManager != null && inputManager.inputSignal > 0)
        {
            int newSignal = inputManager.inputSignal;
            inputManager.inputSignal = 0;
            count = inputManager._count;
            min = inputManager._min;
            query = inputManager._query;
            bool1 = inputManager.bool_value;
            int1 = inputManager.int_value;
            int2 = inputManager.int_value2;
            string1 = inputManager.string_value;
            ints.Clear();
            foreach (int i in inputManager._ints)
                ints.Add(i);
            tempIDs.Clear();
            foreach (int tempID in inputManager._tempIDs)
                tempIDs.Add(tempID);
            tempIDs2.Clear();
            foreach (int tempID in inputManager._tempIDs2)
                tempIDs2.Add(tempID);
            cardIDs.Clear();
            foreach (string cardID in inputManager._cardIDs)
                cardIDs.Add(cardID);
            cardIDs2.Clear();
            foreach (string cardID in inputManager._cardIDs2)
                cardIDs2.Add(cardID);
            strings.Clear();
            foreach (string s in inputManager._strings)
                strings.Add(s);
            faceup.Clear();
            foreach (bool f in inputManager._faceup)
                faceup.Add(f);
            upright.Clear();
            foreach (bool u in inputManager._upright)
                upright.Add(u);
            bools.Clear();
            foreach (bool b in inputManager._bools)
                bools.Add(b);
            actingPlayer = inputManager._actingPlayer._playerID;
            RpcUpdateInputSignal(newSignal);
        }
        if (Thread.CurrentThread == currentThread && !receivedInput && inputSignal > 0)
        {
            receivedInput = true;
            if (inputSignal == InputType.Reset)
                StartCoroutine(ResetInputs());
            else
            {
                switch (inputSignal)
                {
                    case InputType.RPS:
                        Debug.Log("getting RPS");
                        GetRPSInput();
                        break;
                    case InputType.ResolveRPS:
                        ResolveRPS();
                        break;
                    case InputType.Mulligan:
                        currentRoutine = Mulligan();
                        StartCoroutine(currentRoutine);
                        break;
                    case InputType.YesNo:
                        currentRoutine = YesNo();
                        StartCoroutine(currentRoutine);
                        break;
                    case InputType.SelectFromList:
                        currentRoutine = SelectFromList();
                        StartCoroutine(currentRoutine);
                        break;
                    case InputType.SelectRidePhaseAction:
                        currentRoutine = SelectRidePhaseAction();
                        StartCoroutine(currentRoutine);
                        break;
                    case InputType.SelectMainPhaseAction:
                        currentRoutine = SelectMainPhaseAction();
                        StartCoroutine(currentRoutine);
                        break;
                    case InputType.SelectCallLocation:
                        currentRoutine = SelectCallLocation();
                        StartCoroutine(currentRoutine);
                        break;
                    case InputType.SelectBattlePhaseAction:
                        currentRoutine = SelectBattlePhaseAction();
                        StartCoroutine(currentRoutine);
                        break;
                    case InputType.SelectUnitToAttack:
                        currentRoutine = SelectUnitToAttack();
                        StartCoroutine(currentRoutine);
                        break;
                    case InputType.SelectGuardStepAction:
                        currentRoutine = SelectGuardStepAction();
                        StartCoroutine(currentRoutine);
                        break;
                    case InputType.SelectActiveUnit:
                        currentRoutine = SelectActiveUnit();
                        StartCoroutine(currentRoutine);
                        break;
                    case InputType.SelectAbility:
                        currentRoutine = SelectAbility();
                        StartCoroutine(currentRoutine);
                        break;
                    case InputType.SelectOption:
                        currentRoutine = SelectOption();
                        StartCoroutine(currentRoutine);
                        break;
                    case InputType.SelectCircle:
                        currentRoutine = SelectCircle();
                        StartCoroutine(currentRoutine);
                        break;
                }
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            bool buttonClicked = false;
            for (int i = 0; i < Buttons.transform.childCount; i++)
            {
                if (eventSystem.currentSelectedGameObject == Buttons.transform.GetChild(i).gameObject)
                {
                    buttonClicked = true;
                    break;
                }
            }
            if (!buttonClicked)
                ResetMiscellaneousButtons();
            //foreach (Button button in miscellaneousButtons)
            //{
            //    if (button.transform.position.x < 5000 && eventSystem.currentSelectedGameObject != button.gameObject)
            //    {
            //        ResetMiscellaneousButtons();
            //        break;
            //    }

            //}
        }
    }

    [ClientRpc]
    public void RpcUpdateInputSignal(int newInputSignal)
    {
        IEnumerator Dialog()
        {
            yield return new WaitForSeconds(.1f);
            while (cardFightManager.InAnimation())
                yield return null;
            Debug.Log("new input signal: " + newInputSignal);
            inputSignal = newInputSignal;
            receivedInput = false;
        }
        StartCoroutine(Dialog());
    }

    public void OnInputSignalChanged(int oldInputSignal, int newInputSignal)
    {
        Debug.Log("old input signal: " + oldInputSignal);
        Debug.Log("new input signal: " + newInputSignal);
    }

    public bool isActingPlayer()
    {
        bool value = false;
        if (isServer && actingPlayer == 1)
            value = true;
        else if (!isServer && actingPlayer == 2)
            value = true;
        else
            value = false;
        if (value)
            Debug.Log("is acting player");
        else
            Debug.Log("is not acting player");
        return value;

    }

    public class InputType
    {
        public const int Reset = 1;
        public const int RPS = 2;
        public const int ResolveRPS = 3;
        public const int Mulligan = 4;
        public const int YesNo = 5;
        public const int SelectFromList = 9;
        public const int SelectRidePhaseAction = 10;
        public const int SelectMainPhaseAction = 11;
        public const int SelectCallLocation = 12;
        public const int SelectBattlePhaseAction = 13;
        public const int SelectUnitToAttack = 14;
        public const int SelectGuardStepAction = 15;
        public const int SelectActiveUnit = 16;
        public const int SelectAbility = 17;
        public const int SelectOption = 18;
        public const int SelectCircle = 19;
    }

    IEnumerator ResetInputs()
    {
        Debug.Log("resetting inputs");
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);
        currentRoutine = null;
        player1_input = -1;
        player2_input = -1;
        input1 = 0;
        input2 = 0;
        input3 = 0;
        bool1 = false;
        clicked = false;
        if (waitForButton != null)
        {
            waitForButton.Reset();
            waitForButton.Dispose();
        }
        ResetPositions();
        UnitSlots.GetComponent<UnitSlots>().Reset();
        cardSelect.Hide();
        cardSelect.ResetItems();
        cardsAreSelectable = false;
        PlayerHand.GetComponent<Hand>().Reset();
        Globals.Instance.playerDropZone.UnMarkAsSelectable();
        if (isServer)
        {
            yield return null;
            Debug.Log("host resetting");
            playerManager.CmdChangeInput(1, -1);
        }
        else
        {
            yield return null;
            Debug.Log("remote resetting");
            playerManager.CmdChangeInput(2, -1);
        }
        yield return null;
    }

    public IM InitializeInputManager()
    {
        inputManager = new IM();
        inputManager.inputManager = this;

        return inputManager;
    }

    public void GetRPSInput()
    {
        rockButton.transform.localPosition = new Vector3(-175, 0, 0);
        paperButton.transform.localPosition = new Vector3(0, 0, 0);
        scissorsButton.transform.localPosition = new Vector3(175, 0, 0);
        int selection = -1;
        IEnumerator Dialog()
        {
            var waitForButton = new WaitForUIButtons(rockButton, scissorsButton, paperButton);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == rockButton)
                    selection = 0;
                else if (waitForButton.PressedButton == scissorsButton)
                    selection = 2;
                else if (waitForButton.PressedButton == paperButton)
                    selection = 1;
                yield return null;
            }
            waitForButton.Reset();
            OnRPSSelection(selection);
        }
        Debug.Log("waiting for button");
        StartCoroutine(Dialog());
    }

    public void OnRPSSelection(int selection)
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        rockButton.transform.position = new Vector3(10000, 0, 0);
        paperButton.transform.position = new Vector3(10000, 0, 0);
        scissorsButton.transform.position = new Vector3(10000, 0, 0);
        messageBox.transform.localPosition = new Vector3(0, 0, 0);
        if (isServer)
        {
            Debug.Log("player 1 made selection " + selection.ToString());
            playerManager.CmdChangeInput(1, selection);
        }
        else
        {
            Debug.Log("player 2 made selection " + selection.ToString());
            playerManager.CmdChangeInput(2, selection);
        }
    }

    public void ResolveRPS()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        Button player_selection;
        Button enemy_selection;
        messageBox.transform.position = new Vector3(10000, 0, 0);
        if (isServer)
        {
            if (player1_input == 0)
                player_selection = rockButton;
            else if (player1_input == 1)
                player_selection = paperButton;
            else
                player_selection = scissorsButton;
            if (player2_input == 0)
                enemy_selection = rockButton;
            else if (player2_input == 1)
                enemy_selection = paperButton;
            else
                enemy_selection = scissorsButton;
        }
        else
        {
            if (player2_input == 0)
                player_selection = rockButton;
            else if (player2_input == 1)
                player_selection = paperButton;
            else
                player_selection = scissorsButton;
            if (player1_input == 0)
                enemy_selection = rockButton;
            else if (player1_input == 1)
                enemy_selection = paperButton;
            else
                enemy_selection = scissorsButton;
        }
        player_selection.transform.localPosition = new Vector3(0, -100, 0);
        enemy_selection.transform.localPosition = new Vector3(0, 100, 0);
        IEnumerator MoveTowards()
        {
            while (player_selection.transform.localPosition != new Vector3(0, -50, 0) && enemy_selection.transform.localPosition != new Vector3(0, 50, 0))
            {
                float step = 50 * Time.deltaTime;
                player_selection.transform.localPosition = Vector3.MoveTowards(player_selection.transform.localPosition, new Vector3(0, -50, 0), step);
                enemy_selection.transform.localPosition = Vector3.MoveTowards(enemy_selection.transform.localPosition, new Vector3(0, 50, 0), step);
                if (Vector3.Distance(player_selection.transform.localPosition, new Vector3(0, -50, 0)) < 1)
                    break;
                yield return null;
            }
            yield return new WaitForSeconds(1);
            StartCoroutine(ResetInputs());
        }
        StartCoroutine(MoveTowards());
    }

    IEnumerator Mulligan()
    {
        while (cardFightManager.InAnimation())
        {
            yield return null;
            //Debug.Log("waiting");
        }
        if (isActingPlayer())
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Select cards to mulligan.";
            mulliganButton.transform.localPosition = new Vector3(0, -110, 0);
            cardsAreSelectable = true;
            int selection = -1;
            waitForButton = new WaitForUIButtons(mulliganButton);
            Debug.Log("waiting for button");
            while (selection < 0)
            {
                if (waitForButton.PressedButton == mulliganButton)
                    selection = 0;
                yield return null;
            }
            while (!NetworkClient.ready)
                yield return null;
            OnMulliganSelection(selection);
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
        }
    }

    IEnumerator YesNo()
    {
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
        toggle.transform.localPosition = Globals.Instance.TogglePosition;
        if (isActingPlayer())
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = query;
            yesButton.transform.localPosition = new Vector3(-120, -110, 0);
            noButton.transform.localPosition = new Vector3(120, -110, 0);
            int selection = -1;
            waitForButton = new WaitForUIButtons(yesButton, noButton);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == yesButton)
                {
                    selection = 0;
                    if (query == "Boost?")
                    {
                        Debug.Log("booster circle: " + int1);
                        POW.GetComponent<POWSLD>().SetCount(POW.GetComponent<POWSLD>().GetCount() +
                            UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(int1).GetComponent<UnitSlotBehavior>()._power);
                    }
                }
                else if (waitForButton.PressedButton == noButton)
                    selection = 1;
                yield return null;
            }
            waitForButton.Reset();
            while (!NetworkClient.ready)
                yield return null;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            playerManager.CmdSingleInput(selection);
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
            toggle.transform.localPosition = Globals.Instance.TogglePosition;
        }
    }

    IEnumerator SelectFromList()
    {
        Card card;
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
        toggle.transform.localPosition = Globals.Instance.TogglePosition;
        if (isActingPlayer())
        {
            cardSelect.Show();
            cardSelect.Initialize(query, min, count);
            Debug.Log("tempIDs Count: " + tempIDs.Count);
            Debug.Log("cardIDs Count: " + cardIDs.Count);
            for (int i = 0; i < tempIDs.Count; i++)
            {
                card = cardFightManager.LookUpCard(cardIDs[i]);
                if (UnitSlots.GetComponent<UnitSlots>().IsUnit(tempIDs[i]))
                    cardSelect.AddCardSelectItem(tempIDs[i], cardIDs[i], card.name, true, upright[i], false, strings[i]);
                else if (Globals.Instance.playerDamageZone.GetComponent<DamageZone>().ContainsCard(tempIDs[i]))
                    cardSelect.AddCardSelectItem(tempIDs[i], cardIDs[i], card.name, faceup[i], true, false, strings[i]);
                else
                    cardSelect.AddCardSelectItem(tempIDs[i], cardIDs[i], card.name, true, true, false, strings[i]);
            }
            int selection = -1;
            waitForButton = new WaitForUIButtons(cardSelect.SelectButton, cardSelect.CancelButton);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == cardSelect.SelectButton || waitForButton.PressedButton == cardSelect.CancelButton) 
                    selection = 0;
                yield return null;
            }
            waitForButton.Reset();
            while (!NetworkClient.ready)
                yield return null;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            playerManager.CmdChangeInputs(cardSelect.selected);
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
        }
    }

    IEnumerator SelectAbility()
    {
        Card card;
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
        toggle.transform.localPosition = Globals.Instance.TogglePosition;
        if (isActingPlayer())
        {
            cardSelect.Show();
            if (bool1)
                cardSelect.Initialize("Select ability to activate.", 1, 1);
            else
                cardSelect.Initialize("Select ability to activate.", 0, 1);
            Debug.Log("tempIDs Count: " + tempIDs.Count);
            Debug.Log("cardIDs Count: " + cardIDs.Count);
            for (int i = 0; i < tempIDs.Count; i++)
            {
                card = cardFightManager.LookUpCard(cardIDs[i]);
                cardSelect.AddCardSelectItem(tempIDs[i], cardIDs[i], card.name, true, true, bools[i], strings[i]);
            }
            int selection = -1;
            waitForButton = new WaitForUIButtons(cardSelect.SelectButton, cardSelect.CancelButton);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == cardSelect.SelectButton)
                    selection = 0;
                else if (waitForButton.PressedButton == cardSelect.CancelButton)
                    selection = 1;
                yield return null;
            }
            waitForButton.Reset();
            while (!NetworkClient.ready)
                yield return null;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            if (selection == 0)
                playerManager.CmdSingleInput(tempIDs.IndexOf(cardSelect.selected[0]));
            else if (selection == 1)
                playerManager.CmdSingleInput(tempIDs.Count);
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
        }
    }

    IEnumerator SelectRidePhaseAction()
    {
        Debug.Log("selecting ride phase action");
        List<int> list = new List<int>();
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
        toggle.transform.localPosition = Globals.Instance.TogglePosition;
        if (isActingPlayer())
        {
            for (int i = 0; i < tempIDs.Count; i++)
            {
                PlayerHand.GetComponent<Hand>().MarkAsSelectable(tempIDs[i]);
            }
            int selection = -1;
            int selection2 = -1;
            miscellaneousButtons.Clear();
            miscellaneousButtons.Add(rideFromRideDeck);
            miscellaneousButtons.Add(cardButton);
            PhaseManager.GetComponent<PhaseManager>().MainPhaseButton.interactable = true;
            PhaseManager.GetComponent<PhaseManager>().EndPhaseButton.interactable = false;
            waitForButton = new WaitForUIButtons(rideFromRideDeck, cardButton, PhaseManager.GetComponent<PhaseManager>().MainPhaseButton);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == rideFromRideDeck)
                {
                    Debug.Log("rideFromRideDeck pressed");
                    selection = RidePhaseAction.RideFromRideDeck;
                }
                else if (waitForButton.PressedButton == cardButton)
                {
                    selection = RidePhaseAction.RideFromHand;
                    selection2 = selectedCard;
                }
                else if (waitForButton.PressedButton == PhaseManager.GetComponent<PhaseManager>().MainPhaseButton)
                    selection = RidePhaseAction.End;
                yield return null;
            }
            PhaseManager.GetComponent<PhaseManager>().MainPhaseButton.interactable = false;
            waitForButton.Reset();
            while (!NetworkClient.ready)
                yield return null;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            playerManager.CmdSingleInputs(selection, selection2);
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
        }
    }

    IEnumerator SelectMainPhaseAction()
    {
        Debug.Log("selecting main phase action");
        List<int> list = new List<int>();
        selectedGameObject = null;
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
        toggle.transform.localPosition = Globals.Instance.TogglePosition;
        if (isActingPlayer())
        {
            for (int i = 0; i < tempIDs.Count; i++)
            {
                PlayerHand.GetComponent<Hand>().MarkAsSelectable(tempIDs[i]);
            }
            for (int i = 0; i < tempIDs2.Count; i++)
            {
                if (PlayerHand.GetComponent<Hand>().IsInHand(tempIDs2[i]))
                    PlayerHand.GetComponent<Hand>().MarkAsSelectable(tempIDs2[i]);
                else if (Globals.Instance.unitSlots.IsUnit(tempIDs2[i]))
                    Globals.Instance.unitSlots.MarkAsSelectable(tempIDs2[i]);
                else if (Globals.Instance.playerDropZone.ContainsCard(tempIDs2[i]))
                    Globals.Instance.playerDropZone.MarkAsSelectable();
            }
            int selection = -1;
            int selection2 = -1;
            miscellaneousButtons.Clear();
            miscellaneousButtons.Add(cardButton);
            Debug.Log("turn: " + int1);
            PhaseManager.GetComponent<PhaseManager>().BattlePhaseButton.interactable = true;
            if (int1 <= 1)
            {
                PhaseManager.GetComponent<PhaseManager>().EndPhaseButton.interactable = true;
                PhaseManager.GetComponent<PhaseManager>().BattlePhaseButton.interactable = false;
            }
            waitForButton = new WaitForUIButtons(cardButton, cardButton2, PhaseManager.GetComponent<PhaseManager>().BattlePhaseButton, PhaseManager.GetComponent<PhaseManager>().EndPhaseButton,
                Globals.Instance.soulCharge, Globals.Instance.counterCharge, Globals.Instance.damage, Globals.Instance.heal);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == cardButton)
                {
                    if (cardButton.GetComponentInChildren<Text>().text == "Call")
                    {
                        selection = MainPhaseAction.CallFromHand;
                        selection2 = selectedCard;
                    }
                    else if (cardButton.GetComponentInChildren<Text>().text == "Move")
                    {
                        selection = MainPhaseAction.MoveRearguard;
                        selection2 = selectedCard;
                    }
                    else if (cardButton.GetComponentInChildren<Text>().text == "Free")
                    {
                        selection = MainPhaseAction.CallFromPrison;
                    }
                }
                else if (waitForButton.PressedButton == cardButton2)
                {
                    if (cardButton2.GetComponentInChildren<Text>().text == "Activate")
                    {
                        if (selectedGameObject == Globals.Instance.playerDropZone.gameObject)
                            selection = MainPhaseAction.ActivateAbilityFromDrop;
                        else
                        {
                            selection = MainPhaseAction.ActivateAbility;
                            selection2 = selectedCard;
                        }
                        selectedGameObject = null;
                    }
                }
                else if ((waitForButton.PressedButton == PhaseManager.GetComponent<PhaseManager>().BattlePhaseButton) ||
                    (waitForButton.PressedButton == PhaseManager.GetComponent<PhaseManager>().EndPhaseButton))
                    selection = MainPhaseAction.End;
                else if (waitForButton.PressedButton == Globals.Instance.soulCharge)
                    selection = MainPhaseAction.SoulCharge;
                else if (waitForButton.PressedButton == Globals.Instance.counterCharge)
                    selection = MainPhaseAction.CounterCharge;
                else if (waitForButton.PressedButton == Globals.Instance.damage)
                    selection = MainPhaseAction.TakeDamage;
                else if (waitForButton.PressedButton == Globals.Instance.heal)
                    selection = MainPhaseAction.Heal;
                yield return null;
            }
            PhaseManager.GetComponent<PhaseManager>().BattlePhaseButton.interactable = false;
            PhaseManager.GetComponent<PhaseManager>().EndPhaseButton.interactable = false;
            waitForButton.Reset();
            while (!NetworkClient.ready)
                yield return null;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            playerManager.CmdSingleInputs(selection, selection2);
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
        }
    }

    IEnumerator SelectBattlePhaseAction()
    {
        List<int> list = new List<int>();
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
        toggle.transform.localPosition = Globals.Instance.TogglePosition;
        if (isActingPlayer())
        {
            for (int i = 0; i < tempIDs.Count; i++)
            {
                UnitSlots.GetComponent<UnitSlots>().MarkAsSelectable(tempIDs[i]);
            }
            int selection = -1;
            int selection2 = -1;
            miscellaneousButtons.Clear();
            miscellaneousButtons.Add(cardButton);
            Debug.Log("making end phase button interactable");
            PhaseManager.GetComponent<PhaseManager>().EndPhaseButton.interactable = true;
            waitForButton = new WaitForUIButtons(cardButton, PhaseManager.GetComponent<PhaseManager>().EndPhaseButton);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == cardButton)
                {
                    if (cardButton.GetComponentInChildren<Text>().text == "Attack")
                    {
                        selection = BattlePhaseAction.Attack;
                        selection2 = selectedCard;
                        POW.transform.localPosition = new Vector3(-382, 0, 0);
                        POW.GetComponent<POWSLD>().SetCount(UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(selectedUnit).GetComponent<UnitSlotBehavior>()._power);
                    }
                }
                else if (waitForButton.PressedButton == PhaseManager.GetComponent<PhaseManager>().EndPhaseButton)
                    selection = BattlePhaseAction.End;
                yield return null;
            }
            PhaseManager.GetComponent<PhaseManager>().EndPhaseButton.interactable = false;
            waitForButton.Reset();
            while (!NetworkClient.ready)
                yield return null;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            playerManager.CmdSingleInputs(selection, selection2);
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
        }
    }

    IEnumerator SelectCallLocation()
    {
        Debug.Log("selecting call location");
        List<int> list = new List<int>();
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
        toggle.transform.localPosition = Globals.Instance.TogglePosition;
        Globals.Instance.callCard.transform.localPosition = new Vector3(0, 200, 0);
        Globals.Instance.callCard.GetComponent<Image>().sprite = CardFightManager.LoadSprite(CardFightManager.FixFileName(string1));
        clicked = false;
        if (isActingPlayer())
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Select call location.";
            while (!clicked)
                yield return null;
            while (!NetworkClient.ready)
                yield return null;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            playerManager.CmdSingleInput(selectedUnit);
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
        }
    }

    IEnumerator SelectUnitToAttack()
    {
        Debug.Log("selecting unit to attack");
        List<int> list = new List<int>();
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
        toggle.transform.localPosition = Globals.Instance.TogglePosition;
        if (isActingPlayer())
        {
            for (int i = 0; i < tempIDs.Count; i++)
            {
                UnitSlots.GetComponent<UnitSlots>().MarkAsSelectable(tempIDs[i]);
            }
            int selection = -1;
            int selection2 = -1;
            miscellaneousButtons.Clear();
            miscellaneousButtons.Add(cardButton);
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Select unit to attack.";
            cancelButton.transform.localPosition = new Vector3(0, -110, 0);
            waitForButton = new WaitForUIButtons(cardButton, cancelButton);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == cardButton)
                {
                    if (cardButton.GetComponentInChildren<Text>().text == "Attack")
                    {
                        selection = selectedCard;
                        UnitSlots.GetComponent<UnitSlots>().Reset();
                    }
                }
                else if (waitForButton.PressedButton == cancelButton)
                {
                    selection = -1;
                    POW.transform.position = new Vector3(-10000, 0, 0);
                    POW.GetComponent<POWSLD>().SetCount(0);
                    UnitSlots.GetComponent<UnitSlots>().Reset();
                    break;
                }
                yield return null;
            }
            waitForButton.Reset();
            while (!NetworkClient.ready)
                yield return null;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            playerManager.CmdSingleInputs(selection, selection2);
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
        }
    }

    IEnumerator SelectGuardStepAction()
    {
        Debug.Log("selecting guard step action");
        List<int> list = new List<int>();
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
        toggle.transform.localPosition = Globals.Instance.TogglePosition;
        if (isActingPlayer())
        {
            for (int i = 0; i < tempIDs.Count; i++)
            {
                UnitSlots.GetComponent<UnitSlots>().MarkAsSelectable(tempIDs[i]);
            }
            for (int i = 0; i < tempIDs2.Count; i++)
            {
                if (PlayerHand.GetComponent<Hand>().IsInHand(tempIDs2[i]))
                    PlayerHand.GetComponent<Hand>().MarkAsSelectable(tempIDs2[i]);
                else if (Globals.Instance.unitSlots.IsUnit(tempIDs2[i]))
                    Globals.Instance.unitSlots.MarkAsSelectable(tempIDs2[i]);
            }
            Button selectionButton1 = GameObject.Find("SelectionButton1").GetComponent<Button>();
            Button selectionButton2 = GameObject.Find("SelectionButton2").GetComponent<Button>();
            int selection = -1;
            int selection2 = -1;
            selectionButton1.transform.localPosition = Globals.Instance.YesPosition;
            selectionButton1.GetComponentInChildren<Text>().text = "Guard";
            selectionButton2.transform.localPosition = Globals.Instance.NoPosition;
            selectionButton2.GetComponentInChildren<Text>().text = "End Guard";
            if (!bool1)
                selectionButton1.interactable = false;
            else
                selectionButton1.interactable = true;
            waitForButton = new WaitForUIButtons(selectionButton1, selectionButton2, cardButton, cardButton2);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == selectionButton1)
                    selection = GuardStepAction.Guard;
                else if (waitForButton.PressedButton == selectionButton2)
                    selection = GuardStepAction.End;
                else if (waitForButton.PressedButton == cardButton && cardButton.GetComponentInChildren<Text>().text == "Intercept")
                {
                    selection = GuardStepAction.Intercept;
                    selection2 = selectedCard;
                }
                else if (waitForButton.PressedButton == cardButton2 && cardButton2.GetComponentInChildren<Text>().text == "Activate")
                {
                    selection = GuardStepAction.BlitzOrder;
                    selection2 = selectedCard;
                }
                yield return null;
            }
            waitForButton.Reset();
            while (!NetworkClient.ready)
                yield return null;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            playerManager.CmdSingleInputs(selection, selection2);
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
        }
    }

    IEnumerator SelectActiveUnit()
    {
        Debug.Log("selecting active unit");
        List<int> list = new List<int>();
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
        toggle.transform.localPosition = Globals.Instance.TogglePosition;
        if (isActingPlayer())
        {
            for (int i = 0; i < tempIDs.Count; i++)
            {
                UnitSlots.GetComponent<UnitSlots>().MarkAsSelectable(tempIDs[i]);
            }
            int selection = -1;
            int selection2 = -1;
            miscellaneousButtons.Clear();
            miscellaneousButtons.Add(cardButton);
            waitForButton = new WaitForUIButtons(cardButton);
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = query;
            while (selection < 0)
            {
                if (waitForButton.PressedButton == cardButton)
                {
                    if (cardButton.GetComponentInChildren<Text>().text == "Select")
                    {
                        selection = selectedCard;
                        UnitSlots.GetComponent<UnitSlots>().Reset();
                    }
                }
                yield return null;
            }
            waitForButton.Reset();
            while (!NetworkClient.ready)
                yield return null;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            playerManager.CmdSingleInputs(selection, selection2);
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
        }
    }

    IEnumerator SelectOption()
    {
        Debug.Log("selecting option");
        List<int> list = new List<int>();
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
        toggle.transform.localPosition = Globals.Instance.TogglePosition;
        if (isActingPlayer())
        {
            Button selectionButton1 = GameObject.Find("SelectionButton1").GetComponent<Button>();
            Button selectionButton2 = GameObject.Find("SelectionButton2").GetComponent<Button>();
            int selection = -1;
            int selection2 = -1;
            selectionButton1.transform.localPosition = Globals.Instance.YesPosition;
            selectionButton1.GetComponentInChildren<Text>().text = strings[0];
            selectionButton2.transform.localPosition = Globals.Instance.NoPosition;
            selectionButton2.GetComponentInChildren<Text>().text = strings[1];
            waitForButton = new WaitForUIButtons(selectionButton1, selectionButton2);
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Choose an option.";
            while (selection < 0)
            {
                if (waitForButton.PressedButton == selectionButton1)
                {
                    selection = 1;
                }
                else if (waitForButton.PressedButton == selectionButton2)
                {
                    selection = 2;
                }
                yield return null;
            }
            waitForButton.Reset();
            while (!NetworkClient.ready)
                yield return null;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            playerManager.CmdSingleInputs(selection, selection2);
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
        }
    }

    IEnumerator SelectCircle()
    {
        Debug.Log("selecting circle");
        List<int> list = new List<int>();
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
        toggle.transform.localPosition = Globals.Instance.TogglePosition;
        clicked = false;
        if (isActingPlayer())
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Select " + int1 + " circle(s).";
            selectedUnit = -1;
            List<int> selectedCircles = new List<int>();
            while (selectedCircles.Count < int1)
            {
                if (selectedUnit >= 0)
                {
                    if (!ints.Contains(selectedUnit))
                        Debug.Log("invalid circle");
                    else
                    {
                        if (selectedCircles.Contains(selectedUnit))
                        {
                            UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(selectedUnit).GetComponentInChildren<UnitSelectArea>().Reset();
                            selectedCircles.Remove(selectedUnit);
                        }
                        else
                        {
                            UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(selectedUnit).GetComponentInChildren<UnitSelectArea>().MarkWithColor(Color.red);
                            selectedCircles.Add(selectedUnit);
                        }
                    }
                    clicked = false;
                    selectedUnit = -1;
                }
                yield return null;
            }
            while (!NetworkClient.ready)
                yield return null;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            playerManager.CmdChangeInputs(selectedCircles);
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
        }
    }

    public void OnMulliganSelection(int selection)
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        CardBehavior card;
        List<int> selections = new List<int>();
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        mulliganButton.transform.position = new Vector3(10000, 0, 0);
        for (int i = 0; i < PlayerHand.transform.childCount; i++)
        {
            card = PlayerHand.transform.GetChild(i).gameObject.GetComponent<CardBehavior>();
            if (card.selected)
                selections.Add(int.Parse(card.gameObject.name));
        }
        playerManager.CmdChangeInputs(selections);
        if (isServer)
        {
            Debug.Log("player 1 made selection " + selection.ToString());
        }
        else
        {
            Debug.Log("player 2 made selection " + selection.ToString());
        }
    }

    public void ResetReceive()
    {
        IEnumerator Dialog()
        {
            inputSignal = 0;
            Debug.Log("changed to false");
            while (!NetworkClient.ready)
            {
                Debug.Log("not ready");
                yield return null;
            }
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            Debug.Log("starting CmdReady");
            playerManager.CmdReady();
        }
        StartCoroutine(Dialog());
    }

    public void ResetMiscellaneousButtons()
    {
        GameObject button;
        while (Buttons.transform.childCount > 0)
        {
            button = Buttons.transform.GetChild(0).gameObject;
            button.transform.SetParent(null);
            button.transform.position = Globals.Instance.ResetPosition;
        }
        //rideFromRideDeck.transform.position = new Vector3(10000, 0, 0);
        //cardButton.transform.position = new Vector3(10000, 0, 0);
    }

    //public void OnPlayerRideDeckClicked()
    //{
    //    if (cardFightManager != null && !cardFightManager.InAnimation())
    //    {
    //        GameObject rideDeck = GameObject.Find("PlayerRideDeck");
    //        Buttons.transform.position = new Vector3(rideDeck.transform.position.x, rideDeck.transform.position.y + ((rideDeck.transform.localScale.y / (float)2) + (rideFromRideDeck.transform.localScale.y * (float)1.25)), 0);
    //    }
    //    if (inputSignal == InputType.SelectRidePhaseAction && bool1)
    //    {
    //        rideFromRideDeck.transform.SetParent(Buttons.transform);
    //    }
    //}

    public void OnPileClicked(Pile pile)
    {
        Debug.Log("pile clicked");
        Buttons.transform.position = pile.gameObject.transform.position;
        if (!cardFightManager.inAnimation && pile.gameObject.name == "PlayerRideDeck" && inputSignal == InputType.SelectRidePhaseAction && isActingPlayer() && bool1)
        {
            rideFromRideDeck.transform.SetParent(Buttons.transform);
        }
        if (!cardFightManager.inAnimation && pile.gameObject.name == "EnemyOrderZone" && inputSignal == InputType.SelectMainPhaseAction && Globals.Instance.enemyMiscStats.prisoners > 0)
        {
            cardButton.transform.SetParent(Buttons.transform);
            cardButton.GetComponentInChildren<Text>().text = "Free";
        }
        if (!cardFightManager.inAnimation && pile.gameObject.name == "PlayerDropZone" && pile.selectable && inputSignal == InputType.SelectMainPhaseAction)
        {
            cardButton2.transform.SetParent(Buttons.transform);
            cardButton2.GetComponentInChildren<Text>().text = "Activate";
            selectedGameObject = pile.gameObject;
        }
        if (!pile.gameObject.name.Contains("Deck") && cardFightManager != null && !cardFightManager.InAnimation())
        {
            if (pile.GetCards().Count == 0)
                return;
            viewButton.transform.SetParent(Buttons.transform);
            selectedGameObject = pile.gameObject;
        }
    }

    public void OnViewButtonClicked()
    {
        ResetMiscellaneousButtons();
        if (selectedGameObject == null)
            return;
        if (selectedGameObject.TryGetComponent(out Pile pile))
        {
            cardViewer.Show();
            string label = "";
            if (pile.name == "PlayerDropZone")
                label = "Player Drop Zone";
            else if (pile.name == "EnemyDropZone")
                label = "Enemy Drop Zone";
            else if (pile.name == "PlayerOrderZone")
                label = "Player Order Zone";
            else if (pile.name == "EnemyOrderZone")
                label = "Enemy Order Zone";
            cardViewer.Initialize(label, 0, 0);
            foreach (Tuple<Card, bool> item in pile.GetCardsWithFaceUp())
            {
                cardViewer.AddCardSelectItem(item.Item1.tempID, item.Item1.id, item.Item1.name, item.Item2, true, false, "");
            }
        }
        else if (selectedGameObject.GetComponent<UnitSlotBehavior>() != null)
        {
            cardViewer.Show();
            cardViewer.Initialize("Soul", 0, 0);
            foreach (Card card in selectedGameObject.GetComponent<UnitSlotBehavior>()._soul)
            {
                cardViewer.AddCardSelectItem(card.tempID, card.id, card.name, true, true, false, "");
            }
        }
        selectedGameObject = null;
    }

    public void OnCardViewerCancelClicked()
    {
        cardViewer.Hide();
        cardViewer.ResetItems();
    }

    public void OnCardClicked(GameObject card)
    {
        if (cardFightManager != null && !cardFightManager.InAnimation())
        {
            Buttons.transform.position = new Vector3(card.transform.position.x, card.transform.position.y + 100, 0);
        }
        if (inputSignal == InputType.SelectRidePhaseAction)
        {
            cardButton.transform.GetComponentInChildren<Text>().text = "Ride";
            cardButton.transform.SetParent(Buttons.transform);
            Debug.Log(card.name);
            selectedCard = Int32.Parse(card.name);
        }
        else if (inputSignal == InputType.SelectMainPhaseAction || inputSignal == InputType.SelectGuardStepAction)
        {
            Debug.Log(card.name);
            selectedCard = Int32.Parse(card.name);
            if (tempIDs.Contains(Int32.Parse(card.name)))
            {
                cardButton.transform.GetComponentInChildren<Text>().text = "Call";
                cardButton.transform.SetParent(Buttons.transform);
            }
            if (tempIDs2.Contains(Int32.Parse(card.name)))
            {
                cardButton2.transform.GetComponentInChildren<Text>().text = "Activate";
                cardButton2.transform.SetParent(Buttons.transform);
                Debug.Log(card.name + " selected for order");
            }
        }
    }

    public void OnUnitClicked(int unitSlot, GameObject unit, bool selected)
    {
        Debug.Log("clicked unitSlot: " + unitSlot);
        if (isActingPlayer() && !cardFightManager.inAnimation)
        {
            if (inputSignal == InputType.SelectCallLocation || inputSignal == InputType.SelectCircle)
            {
                selectedUnit = unitSlot;
                clicked = true;
            }
            if (cardFightManager != null && !cardFightManager.InAnimation() && unit != null)
            {
                Buttons.transform.position = new Vector3(unit.transform.position.x, unit.transform.position.y + 50, 0);
            }
            else
                return;
            if (inputSignal == InputType.SelectMainPhaseAction)
            {
                if (unit != null)
                {
                    cardButton.transform.GetComponentInChildren<Text>().text = "Move";
                    cardButton.transform.SetParent(Buttons.transform);
                    Debug.Log(unit.name);
                    selectedCard = Int32.Parse(unit.name);
                    selectedUnit = unitSlot;
                    if (selected)
                    {
                        cardButton2.transform.GetComponentInChildren<Text>().text = "Activate";
                        cardButton2.transform.SetParent(Buttons.transform);
                        Debug.Log(unit.name + " selected for ACT");
                    }
                }
            }
            if (inputSignal == InputType.SelectBattlePhaseAction || inputSignal == InputType.SelectUnitToAttack)
            {
                if (unit != null && selected)
                {
                    cardButton.transform.GetComponentInChildren<Text>().text = "Attack";
                    cardButton.transform.SetParent(Buttons.transform);
                    Debug.Log(unit.name);
                    selectedCard = Int32.Parse(unit.name);
                    selectedUnit = unitSlot;
                }
            }
            if (inputSignal == InputType.SelectActiveUnit)
            {
                if (unit != null && selected)
                {
                    cardButton.transform.GetComponentInChildren<Text>().text = "Select";
                    cardButton.transform.SetParent(Buttons.transform);
                    Debug.Log(unit.name);
                    selectedCard = Int32.Parse(unit.name);
                    selectedUnit = unitSlot;
                }
            }
            if (inputSignal == InputType.SelectGuardStepAction)
            {
                if (unit != null && selected)
                {
                    cardButton.transform.GetComponentInChildren<Text>().text = "Intercept";
                    cardButton.transform.SetParent(Buttons.transform);
                    Debug.Log(unit.name);
                    selectedCard = Int32.Parse(unit.name);
                    selectedUnit = unitSlot;
                }
            }
        }
        if (Globals.Instance.unitSlots.GetUnitSlot(unitSlot).GetComponent<UnitSlotBehavior>()._soul.Count > 0)
        {
            viewButton.transform.SetParent(Buttons.transform);
            selectedGameObject = Globals.Instance.unitSlots.GetUnitSlot(unitSlot);
            Buttons.transform.position = selectedGameObject.transform.position;
        }
    }

    public void ToggleClicked()
    {
        Debug.Log("toggle clicked");
        if (browsingField)
        {
            messageBox.transform.position = currentMessageBoxPosition;
            yesButton.transform.position = currentYesPosition;
            noButton.transform.position = currentNoPosition;
            cardSelect.transform.position = currentCardSelectPosition;
            Globals.Instance.selectionButton1.transform.position = currentSelectionButton1Position;
            Globals.Instance.selectionButton2.transform.position = currentSelectionButton2Position;
            Globals.Instance.callCard.transform.position = currentCallCardPosition;
            browsingField = false;
            toggle.GetComponentInChildren<Text>().text = "Hide Prompt";
        }
        else
        {
            currentMessageBoxPosition = messageBox.transform.position;
            currentYesPosition = yesButton.transform.position;
            currentNoPosition = noButton.transform.position;
            currentCardSelectPosition = cardSelect.transform.position;
            currentSelectionButton1Position = Globals.Instance.selectionButton1.transform.position;
            currentSelectionButton2Position = Globals.Instance.selectionButton2.transform.position;
            currentCallCardPosition = Globals.Instance.callCard.transform.position;
            messageBox.transform.position = Globals.Instance.ResetPosition;
            yesButton.transform.position = Globals.Instance.ResetPosition;
            noButton.transform.position = Globals.Instance.ResetPosition;
            cardSelect.transform.position = Globals.Instance.ResetPosition;
            Globals.Instance.selectionButton1.transform.position = Globals.Instance.ResetPosition;
            Globals.Instance.selectionButton2.transform.position = Globals.Instance.ResetPosition;
            Globals.Instance.callCard.transform.position = Globals.Instance.ResetPosition;
            ResetMiscellaneousButtons();
            selectedGameObject = null;
            OnCardViewerCancelClicked();
            browsingField = true;
            toggle.GetComponentInChildren<Text>().text = "Show Prompt";
        }
    }

    public void ResetPositions()
    {
        Debug.Log("resetting positions");
        browsingField = false;
        selectedGameObject = null;
        rockButton.transform.position = new Vector3(10000, 0, 0);
        paperButton.transform.position = new Vector3(10000, 0, 0);
        scissorsButton.transform.position = new Vector3(10000, 0, 0);
        messageBox.transform.position = new Vector3(10000, 0, 0);
        mulliganButton.transform.position = new Vector3(10000, 0, 0);
        yesButton.transform.position = new Vector3(10000, 0, 0);
        noButton.transform.position = new Vector3(10000, 0, 0);
        toggle.transform.position = new Vector3(10000, 0, 0);
        cancelButton.transform.position = new Vector3(10000, 0, 0);
        cardViewer.Hide();
        cardViewer.ResetItems();
        viewButton.transform.position = Globals.Instance.ResetPosition;
        currentMessageBoxPosition = Globals.Instance.ResetPosition;
        currentYesPosition = Globals.Instance.ResetPosition;
        currentNoPosition = Globals.Instance.ResetPosition;
        currentCardSelectPosition = Globals.Instance.ResetPosition;
        Globals.Instance.selectionButton1.transform.position = Globals.Instance.ResetPosition;
        Globals.Instance.selectionButton2.transform.position = Globals.Instance.ResetPosition;
        Globals.Instance.callCard.transform.position = Globals.Instance.ResetPosition;
        ResetMiscellaneousButtons();
        miscellaneousButtons.Clear();
    }
}
