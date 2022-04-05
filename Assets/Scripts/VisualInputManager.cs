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
    public Button cardButtonPrefab;
    //public Button cardButton;
    //public Button cardButton2;
    public Button cardButton3;
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
    public Queue<Inputs> inputQueue = new Queue<Inputs>();
    [SyncVar]
    public int count = 0;
    //[SyncVar]
    //public int player1_input = -1;
    //[SyncVar]
    //public int player2_input = -1;
    //[SyncVar(hook = nameof(OnInputSignalChanged))]
    public int inputSignal = -1;
    [SyncVar]
    public int numResponses = 0;
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
    public List<int> tempIDs = new List<int>();
    public List<int> tempIDs2 = new List<int>();
    public List<string> cardIDs = new List<string>();
    public List<string> cardIDs2 = new List<string>();
    public List<string> strings = new List<string>();
    public List<bool> faceup = new List<bool>();
    public List<bool> upright = new List<bool>();
    public List<bool> bools = new List<bool>();
    public List<int> ints = new List<int>();
    public List<bool> isMandatory = new List<bool>();
    public List<bool> canFullyResolve = new List<bool>();
    public List<string> abilityDescriptions = new List<string>();
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
    bool canActivateFromSoul = false;
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
        public Tuple<int, int> _inputSignal = new Tuple<int, int>(0, 0);
        public bool inputActive = false;
        public bool reversed = false;
        public List<int> _tempIDs2 = new List<int>();
        public List<string> _cardIDs = new List<string>();
        public List<string> _cardIDs2 = new List<string>();
        public List<string> _strings = new List<string>();
        public List<bool> _upright = new List<bool>();
        public List<bool> _faceup = new List<bool>();
        public List<bool> _bools = new List<bool>();
        public List<bool> _isMandatory = new List<bool>();
        public List<bool> _canFullyResolve = new List<bool>();
        public List<string> _abilityDescriptions = new List<string>();
        public string string_value;

        void SetInputSignal(int inputSignal)
        {
            _inputSignal = new Tuple<int, int>(inputSignal, _actingPlayer._playerID);
        }

        protected override void RPS_Input()
        {
            Debug.Log("RPS_Input started");
            SetInputSignal(InputType.RPS);
            WaitForReadyToContinue();
            inputActive = false;
            //if (inputManager.player1_input >= 0 && inputManager.player2_input >= 0)
            //{
            //    int_input = inputManager.player2_input;
            //    Debug.Log("second RPS input: " + int_input.ToString());
            //    inputSignal = InputType.ResolveRPS;
            //    while (!inputManager.readyToContinue) ;
            //    inputManager.readyToContinue = false;
            //    oSignalEvent.Set();
            //}
            //else
            //{
            //    inputSignal = InputType.RPS;
            //    Debug.Log(inputManager.readyToContinue);
            //    while (!inputManager.readyToContinue) ;
            //    inputManager.readyToContinue = false;
            //    int_input = inputManager.player1_input;
            //    Debug.Log("first RPS input: " + int_input.ToString());
            //    oSignalEvent.Set();
            //}
        }

        protected override void YesNo_Input()
        {
            Debug.Log("YesNo_Input started");
            Thread.Sleep(250);
            while (inputActive) ;
            inputActive = true;
            SetInputSignal(InputType.YesNo);
            _query = string_input;
            if (string_input == "Boost?")
                int_value = _actingPlayer.GetBooster(_actingPlayer.GetAttacker().tempID);
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectCardsToMulligan_Input()
        {
            Debug.Log("SelectCardsToMulligan_Input started");
            Thread.Sleep(250); //need to refine this later
            while (inputActive) ;
            inputActive = true;
            intlist_input.Clear();
            SetInputSignal(InputType.Mulligan);
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectFromList_Input()
        {
            SelectFromList_Input(false);
        }

        void SelectFromList_Input(bool ignoreInputActive)
        {
            Debug.Log("SelectFromList_Input started");
            Thread.Sleep(250);
            string location;
            if (!ignoreInputActive)
                while (inputActive) ;
            inputActive = true;
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
            SetInputSignal(InputType.SelectFromList);
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void ChooseOrder_Input()
        {
            Debug.Log("ChooseOrder_Input started");
            while (inputActive) ;
            inputActive = true;
            _query = "Choose order for cards.";
            _min = cardsToSelect.Count;
            _max = cardsToSelect.Count;
            //inputActive = false;
            SelectFromList_Input(true);
        }

        protected override void DisplayCards_Input()
        {
            Debug.Log("DsplayCards_Input started");
            while (inputActive) ;
            inputActive = true;
            _query = "Press Cancel to finish looking.";
            _max = 0;
            _min = 0;
            //inputActive = false;
            SelectFromList_Input(true);
        }

        protected override void SelectRidePhaseAction_Input()
        {
            Debug.Log("SelectRidePhaseAction_Input started");
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
            SetInputSignal(InputType.SelectRidePhaseAction);
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectMainPhaseAction_Input()
        {
            Debug.Log("SelectMainPhaseAction_Input started");
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
            foreach (AbilityTimingCount ability in _abilities) //ACT and Orders
            {
                _cardIDs2.Add(ability.ability.GetCard().id);
                _tempIDs2.Add(ability.ability.GetCard().tempID);
            }
            Thread.Sleep(250);
            SetInputSignal(InputType.SelectMainPhaseAction);
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectCallLocation_Input()
        {
            Debug.Log("SelectCallLocation_Input started");
            Thread.Sleep(250);
            while (inputActive) ;
            inputActive = true;
            bool proceed = false;
            while (!proceed)
            {
                string_value = card_input.id;
                SetInputSignal(InputType.SelectCallLocation);
                WaitForReadyToContinue();
                if (!_circles.Contains(int_input) && ((_tempIDs.Count > 0 && _tempIDs.Contains(int_input)) || _tempIDs.Count == 0))
                {
                    proceed = true;
                }
            }
            inputActive = false;
        }

        protected override void SelectBattlePhaseAction_Input()
        {
            Debug.Log("SelectBattlePhaseAction_Input started");
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
            SetInputSignal(InputType.SelectBattlePhaseAction);
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectUnitToAttack_Input()
        {
            Debug.Log("SelectUnitToAttack_Input started");
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
            SetInputSignal(InputType.SelectUnitToAttack);
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectGuardPhaseAction_Input()
        {
            Debug.Log("SelectGuardPhaseAction_Input started");
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
            foreach (AbilityTimingCount ability in _abilities) // Blitz Orders
            {
                _cardIDs2.Add(ability.ability.GetCard().id);
                _tempIDs2.Add(ability.ability.GetCard().tempID);
            }
            Thread.Sleep(250);
            SetInputSignal(InputType.SelectGuardStepAction);
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectActiveUnit_Input()
        {
            Debug.Log("SelectActiveUnit_Input started");
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
            SetInputSignal(InputType.SelectActiveUnit);
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectCardToGuard_Input()
        {
            Debug.Log("SelectCardToGuard_Input started");
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
            SetInputSignal(InputType.SelectActiveUnit);
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectAbility_Input()
        {
            Debug.Log("SelectAbilty_Input started");
            Thread.Sleep(250);
            string location;
            while (inputActive) ;
            inputActive = true;
            _cardIDs.Clear();
            _tempIDs.Clear();
            _strings.Clear();
            intlist_input.Clear();
            _isMandatory.Clear();
            _canFullyResolve.Clear();
            _abilityDescriptions.Clear();
            Debug.Log(_abilities.Count + " abilities on standby.");
            foreach (AbilityTimingCount ability in _abilities)
            {
                _tempIDs.Add(ability.ability.GetCard().tempID);
                _cardIDs.Add(ability.ability.GetCard().id);
                location = "<>";
                //Debug.Log(_actingPlayer.GetLocation(card));
                switch (_actingPlayer.GetLocation(ability.ability.GetCard()))
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
                _upright.Add(_actingPlayer.IsUpRight(ability.ability.GetCard()));
            }
            if (CheckForMandatoryEffects(_abilities))
                bool_value = true;
            else
                bool_value = false;
            _bools.Clear();
            foreach (AbilityTimingCount ability in _abilities)
            {
                if (ability.ability.isMandatory)
                    _isMandatory.Add(true);
                else
                    _isMandatory.Add(false);
                if (ability.ability.CanFullyResolve(ability.activation, ability.timingCount))
                    _canFullyResolve.Add(true);
                else
                    _canFullyResolve.Add(false);
                _abilityDescriptions.Add(ability.ability.Description);
            }
            SetInputSignal(InputType.SelectAbility);
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectOption_Input()
        {
            Debug.Log("SelectOption_Input started");
            Thread.Sleep(250);
            while (inputActive) ;
            inputActive = true;
            _strings.Clear();
            foreach (string option in _list)
            {
                _strings.Add(option);
            }
            SetInputSignal(InputType.SelectOption);
            WaitForReadyToContinue();
            inputActive = false;
        }

        protected override void SelectCircle_Input()
        {
            Debug.Log("SelectCircle_Input started");
            Thread.Sleep(250);
            while (inputActive) ;
            inputActive = true;
            _ints.Clear();
            foreach (int i in intlist_input)
                _ints.Add(i);
            SetInputSignal(InputType.SelectCircle);
            WaitForReadyToContinue();
            inputActive = false;
        }

        public void WaitForReadyToContinue()
        {
            Debug.Log("WaitForReadyToContinue Start");
            //while (!inputManager.readyToContinue && inputManager.inputQueue.Count < 1) ;
            while (true)
            {
                if (inputManager.readyToContinue && inputManager.inputQueue.Count > 0)
                    break;
            }
            //Debug.Log("1| Input Queue: " + inputManager.inputQueue.Count);
            inputManager.readyToContinue = false;
            //Debug.Log("2| Input Queue: " + inputManager.inputQueue.Count);
            Inputs currentInput = inputManager.inputQueue.Dequeue();
            foreach (int input in currentInput.inputs)
                intlist_input.Add(input);
            Debug.Log("received input 1: " + currentInput.input1);
            Debug.Log("received input 2: " + currentInput.input2);
            int_input = currentInput.input1;
            int_input2 = currentInput.input2;
            if (currentInput.input1 == 0)
                bool_input = false;
            else
                bool_input = true;
            //Thread.Sleep(30 / 1000);
            SetInputSignal(InputType.Reset);
            while (!inputManager.readyToContinue) ;
            inputManager.readyToContinue = false;
            Debug.Log("WaitForReadyToContinue Finished");
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
        //cardButton.transform.position = new Vector3(10000, 0, 0);
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
        waitForButton = new WaitForUIButtons();
    }

    // Update is called once per frame
    void Update()
    {
        if (Thread.CurrentThread == currentThread && inputManager != null && inputManager._inputSignal.Item1 > 0)
        {
            int newSignal = inputManager._inputSignal.Item1;
            int player = inputManager._inputSignal.Item2;
            inputManager._inputSignal = new Tuple<int, int>(0, 0);
            count = inputManager._max;
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
            isMandatory.Clear();
            foreach (bool b in inputManager._isMandatory)
                isMandatory.Add(b);
            canFullyResolve.Clear();
            foreach (bool b in inputManager._canFullyResolve)
                canFullyResolve.Add(b);
            abilityDescriptions.Clear();
            foreach (string s in inputManager._abilityDescriptions)
                abilityDescriptions.Add(s);
            UpdateInputSignal(newSignal, player);
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
                        currentRoutine = RPSInput();
                        StartCoroutine(currentRoutine);
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
            Debug.Log("clicked");
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
            {
                Debug.Log("no button clicked");
                ResetMiscellaneousButtons();
            }
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

    public void UpdateInputSignal(int newInputSignal, int player)
    {
        IEnumerator Dialog()
        {
            yield return new WaitForSeconds(.1f);
            while (cardFightManager.InAnimation())
                yield return null;
            Debug.Log("new input signal: " + newInputSignal + ", new acting player: " + player);
            inputSignal = newInputSignal;
            actingPlayer = player;
            receivedInput = false;
        }
        StartCoroutine(Dialog());
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
        //if (value)
        //    Debug.Log("is acting player");
        //else
        //    Debug.Log("is not acting player");
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
            //playerManager.CmdChangeInput(1, -1);
        }
        else
        {
            yield return null;
            Debug.Log("remote resetting");
            //playerManager.CmdChangeInput(2, -1);
        }
        readyToContinue = true;
        yield return null;
    }

    public IM InitializeInputManager()
    {
        inputManager = new IM();
        inputManager.inputManager = this;

        return inputManager;
    }

    IEnumerator RPSInput()
    {
        while (cardFightManager.InAnimation())
            yield return null;
        if (isActingPlayer())
        {
            rockButton.transform.localPosition = new Vector3(-175, 0, 0);
            paperButton.transform.localPosition = new Vector3(0, 0, 0);
            scissorsButton.transform.localPosition = new Vector3(175, 0, 0);
            int selection = -1;
            var waitForButton = new WaitForUIButtons(rockButton, scissorsButton, paperButton);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == null)
                    yield return null;
                if (waitForButton.PressedButton == rockButton)
                    selection = 0;
                else if (waitForButton.PressedButton == scissorsButton)
                    selection = 2;
                else if (waitForButton.PressedButton == paperButton)
                    selection = 1;
                yield return null;
            }
            waitForButton.Reset();
            Inputs newInput = new Inputs();
            newInput.input1 = selection;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            playerManager.CmdInputMade(isServer, newInput);
            inputQueue.Enqueue(newInput);
            //OnRPSSelection();
            readyToContinue = true;
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
            while (inputQueue.Count < 1)
                yield return null;
            //OnRPSSelection();
            readyToContinue = true;
        }
        //Debug.Log("waiting for button");
    }

    public void OnRPSSelection()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        rockButton.transform.position = new Vector3(10000, 0, 0);
        paperButton.transform.position = new Vector3(10000, 0, 0);
        scissorsButton.transform.position = new Vector3(10000, 0, 0);
        messageBox.transform.localPosition = new Vector3(0, 0, 0);
        readyToContinue = true;
        //if (isServer)
        //{
        //    Debug.Log("player 1 made selection " + selection.ToString());
        //    playerManager.CmdChangeInput(1, selection);
        //}
        //else
        //{
        //    Debug.Log("player 2 made selection " + selection.ToString());
        //    playerManager.CmdChangeInput(2, selection);
        //}
    }

    public void ResolveRPS()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        Button player_selection;
        Button enemy_selection;
        messageBox.transform.position = new Vector3(10000, 0, 0);
        //if (isServer)
        //{
        //    if (player1_input == 0)
        //        player_selection = rockButton;
        //    else if (player1_input == 1)
        //        player_selection = paperButton;
        //    else
        //        player_selection = scissorsButton;
        //    if (player2_input == 0)
        //        enemy_selection = rockButton;
        //    else if (player2_input == 1)
        //        enemy_selection = paperButton;
        //    else
        //        enemy_selection = scissorsButton;
        //}
        //else
        //{
        //    if (player2_input == 0)
        //        player_selection = rockButton;
        //    else if (player2_input == 1)
        //        player_selection = paperButton;
        //    else
        //        player_selection = scissorsButton;
        //    if (player1_input == 0)
        //        enemy_selection = rockButton;
        //    else if (player1_input == 1)
        //        enemy_selection = paperButton;
        //    else
        //        enemy_selection = scissorsButton;
        //}
        //player_selection.transform.localPosition = new Vector3(0, -100, 0);
        //enemy_selection.transform.localPosition = new Vector3(0, 100, 0);
        //IEnumerator MoveTowards()
        //{
        //    while (player_selection.transform.localPosition != new Vector3(0, -50, 0) && enemy_selection.transform.localPosition != new Vector3(0, 50, 0))
        //    {
        //        float step = 50 * Time.deltaTime;
        //        player_selection.transform.localPosition = Vector3.MoveTowards(player_selection.transform.localPosition, new Vector3(0, -50, 0), step);
        //        enemy_selection.transform.localPosition = Vector3.MoveTowards(enemy_selection.transform.localPosition, new Vector3(0, 50, 0), step);
        //        if (Vector3.Distance(player_selection.transform.localPosition, new Vector3(0, -50, 0)) < 1)
        //            break;
        //        yield return null;
        //    }
        //    yield return new WaitForSeconds(1);
        //    StartCoroutine(ResetInputs());
        //}
        //StartCoroutine(MoveTowards());
    }

    IEnumerator Mulligan()
    {
        int selection = -1;
        List<int> selections = new List<int>();
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        CardBehavior card;
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
            waitForButton.AddButton(mulliganButton);
            Debug.Log("waiting for button");
            while (selection < 0)
            {
                if (waitForButton.PressedButton == null)
                    yield return null;
                if (waitForButton.PressedButton == mulliganButton)
                    selection = 0;
                yield return null;
            }
            mulliganButton.transform.position = new Vector3(10000, 0, 0);
            for (int i = 0; i < PlayerHand.transform.childCount; i++)
            {
                card = PlayerHand.transform.GetChild(i).gameObject.GetComponent<CardBehavior>();
                if (card.selected)
                    selections.Add(int.Parse(card.gameObject.name));
            }
            Inputs newInput = new Inputs();
            newInput.inputs.AddRange(selections);
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            playerManager.CmdInputMade(isServer, newInput);
            inputQueue.Enqueue(newInput);
            readyToContinue = true;
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
            while (inputQueue.Count < 1)
                yield return null;
            readyToContinue = true;
        }
    }

    IEnumerator YesNo()
    {
        int selection = -1;
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
            waitForButton.AddButton(yesButton);
            waitForButton.AddButton(noButton);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == null)
                    yield return null;
                if (waitForButton.PressedButton == yesButton)
                {
                    selection = 1;
                    if (query == "Boost?")
                    {
                        Debug.Log("booster circle: " + int1);
                        POW.GetComponent<POWSLD>().SetCount(POW.GetComponent<POWSLD>().GetCount() +
                            UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(int1).GetComponent<UnitSlotBehavior>()._power);
                    }
                }
                else if (waitForButton.PressedButton == noButton)
                    selection = 0;
                yield return null;
            }
            waitForButton.Reset();
            while (!NetworkClient.ready)
                yield return null;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            Inputs newInput = new Inputs();
            newInput.input1 = selection;
            playerManager.CmdInputMade(isServer, newInput);
            inputQueue.Enqueue(newInput);
            readyToContinue = true;
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
            toggle.transform.localPosition = Globals.Instance.TogglePosition;
            while (inputQueue.Count < 1)
            {
                yield return null;
            }
            readyToContinue = true;
        }
    }

    IEnumerator SelectFromList()
    {
        Card card;
        List<int> selections = new List<int>();
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
        toggle.transform.localPosition = Globals.Instance.TogglePosition;
        if (isActingPlayer())
        {
            cardSelect.Show();
            cardSelect.Initialize(query, min, count);
            //Debug.Log("tempIDs Count: " + tempIDs.Count);
            //Debug.Log("cardIDs Count: " + cardIDs.Count);
            for (int i = 0; i < tempIDs.Count; i++)
            {
                card = cardFightManager.LookUpCard(cardIDs[i]);
                if (UnitSlots.GetComponent<UnitSlots>().IsUnit(tempIDs[i]))
                    cardSelect.AddCardSelectItem(tempIDs[i], cardIDs[i], card.name, true, upright[i], false, true, "", strings[i]);
                else if (Globals.Instance.playerDamageZone.GetComponent<DamageZone>().ContainsCard(tempIDs[i]))
                    cardSelect.AddCardSelectItem(tempIDs[i], cardIDs[i], card.name, faceup[i], true, false, true, "", strings[i]);
                else
                    cardSelect.AddCardSelectItem(tempIDs[i], cardIDs[i], card.name, true, true, false, true, "", strings[i]);
            }
            int selection = -1;
            waitForButton.AddButton(cardSelect.SelectButton);
            waitForButton.AddButton(cardSelect.CancelButton);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == null)
                    yield return null;
                if (waitForButton.PressedButton == cardSelect.SelectButton || waitForButton.PressedButton == cardSelect.CancelButton) 
                    selection = 0;
                yield return null;
            }
            cardSelect.transform.position = Globals.Instance.ResetPosition;
            waitForButton.Reset();
            selections.AddRange(cardSelect.selected);
            while (!NetworkClient.ready)
                yield return null;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            Inputs newInput = new Inputs();
            newInput.inputs.AddRange(selections);
            playerManager.CmdInputMade(isServer, newInput);
            inputQueue.Enqueue(newInput);
            readyToContinue = true;
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
            toggle.transform.localPosition = Globals.Instance.TogglePosition;
            while (inputQueue.Count < 1)
            {
                yield return null;
            }
            readyToContinue = true;
        }
    }

    IEnumerator SelectAbility()
    {
        Card card;
        int selection = -1;
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
            //Debug.Log("tempIDs Count: " + tempIDs.Count);
            //Debug.Log("cardIDs Count: " + cardIDs.Count);
            for (int i = 0; i < tempIDs.Count; i++)
            {
                card = cardFightManager.LookUpCard(cardIDs[i]);
                cardSelect.AddCardSelectItem(tempIDs[i], cardIDs[i], card.name, true, true, isMandatory[i], canFullyResolve[i], abilityDescriptions[i], strings[i]);
            }
            waitForButton.AddButton(cardSelect.SelectButton);
            waitForButton.AddButton(cardSelect.CancelButton);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == null)
                    yield return null;
                if (waitForButton.PressedButton == cardSelect.SelectButton)
                    selection = 0;
                else if (waitForButton.PressedButton == cardSelect.CancelButton)
                    selection = 1;
                yield return null;
            }
            cardSelect.transform.position = Globals.Instance.ResetPosition;
            waitForButton.Reset();
            if (selection == 0)
                selection = tempIDs.IndexOf(cardSelect.selected[0]);
            else if (selection == 1)
                selection = tempIDs.Count;
            Inputs newInput = new Inputs();
            newInput.input1 = selection;
            while (!NetworkClient.ready)
                yield return null;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            playerManager.CmdInputMade(isServer, newInput);
            inputQueue.Enqueue(newInput);
            readyToContinue = true;
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
            toggle.transform.localPosition = Globals.Instance.TogglePosition;
            while (inputQueue.Count < 1)
            {
                yield return null;
            }
            readyToContinue = true;
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
            PhaseManager.GetComponent<PhaseManager>().MainPhaseButton.interactable = true;
            PhaseManager.GetComponent<PhaseManager>().EndPhaseButton.interactable = false;
            waitForButton.AddButton(rideFromRideDeck);
            waitForButton.AddButton(PhaseManager.GetComponent<PhaseManager>().MainPhaseButton);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == rideFromRideDeck)
                {
                    Debug.Log("rideFromRideDeck pressed");
                    selection = RidePhaseAction.RideFromRideDeck;
                }
                else if (waitForButton.PressedButton != null && waitForButton.PressedButton.GetComponentInChildren<Text>().text == "Ride")
                {
                    selection = RidePhaseAction.RideFromHand;
                    selection2 = selectedCard;
                }
                else if (waitForButton.PressedButton != null && waitForButton.PressedButton == PhaseManager.GetComponent<PhaseManager>().MainPhaseButton)
                    selection = RidePhaseAction.End;
                yield return null;
            }
            PhaseManager.GetComponent<PhaseManager>().MainPhaseButton.interactable = false;
            waitForButton.Reset();
            while (!NetworkClient.ready)
                yield return null;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            Inputs newInput = new Inputs();
            newInput.input1 = selection;
            newInput.input2 = selection2;
            playerManager.CmdInputMade(isServer, newInput);
            inputQueue.Enqueue(newInput);
            readyToContinue = true;
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
            while (inputQueue.Count < 1)
                yield return null;
            readyToContinue = true;
        }
    }

    IEnumerator SelectMainPhaseAction()
    {
        Debug.Log("selecting main phase action");
        Debug.Log("inputQueue count: " + inputQueue.Count);
        List<int> list = new List<int>();
        selectedGameObject = null;
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
        canActivateFromSoul = false;
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
                else if (GameObject.Find("PlayerVG").GetComponent<UnitSlotBehavior>()._soul.Exists(soul => soul.tempID == tempIDs2[i]))
                    canActivateFromSoul = true;
            }
            int selection = -1;
            int selection2 = -1;
            miscellaneousButtons.Clear();
            Debug.Log("turn: " + int1);
            PhaseManager.GetComponent<PhaseManager>().BattlePhaseButton.interactable = true;
            if (int1 <= 1)
            {
                PhaseManager.GetComponent<PhaseManager>().EndPhaseButton.interactable = true;
                PhaseManager.GetComponent<PhaseManager>().BattlePhaseButton.interactable = false;
            }
            waitForButton.AddButton(PhaseManager.GetComponent<PhaseManager>().BattlePhaseButton);
            waitForButton.AddButton(PhaseManager.GetComponent<PhaseManager>().EndPhaseButton);
            waitForButton.AddButton(Globals.Instance.soulCharge);
            waitForButton.AddButton(Globals.Instance.counterCharge);
            waitForButton.AddButton(Globals.Instance.damage);
            waitForButton.AddButton(Globals.Instance.heal);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == null)
                    yield return null;
                if (waitForButton.PressedButton != null && waitForButton.PressedButton.GetComponentInChildren<Text>().text == "Call")
                {
                    selection = MainPhaseAction.CallFromHand;
                    selection2 = selectedCard;
                }
                else if (waitForButton.PressedButton != null && waitForButton.PressedButton.GetComponentInChildren<Text>().text == "Move")
                {
                    selection = MainPhaseAction.MoveRearguard;
                    selection2 = selectedCard;
                }
                else if (waitForButton.PressedButton != null && waitForButton.PressedButton.GetComponentInChildren<Text>().text == "Free")
                {
                    selection = MainPhaseAction.CallFromPrison;
                }
                if (waitForButton.PressedButton != null && waitForButton.PressedButton.GetComponentInChildren<Text>().text == "Activate")
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
                if (waitForButton.PressedButton != null && waitForButton.PressedButton.GetComponentInChildren<Text>().text == "Activate Soul")
                {
                    selection = MainPhaseAction.ActivateAbilityFromSoul;
                    selectedGameObject = null;
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
            Inputs newInput = new Inputs();
            newInput.input1 = selection;
            newInput.input2 = selection2;
            playerManager.CmdInputMade(isServer, newInput);
            inputQueue.Enqueue(newInput);
            readyToContinue = true;
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
            while (inputQueue.Count < 1)
                yield return null;
            readyToContinue = true;
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
            Debug.Log("making end phase button interactable");
            PhaseManager.GetComponent<PhaseManager>().EndPhaseButton.interactable = true;
            waitForButton.AddButton(PhaseManager.GetComponent<PhaseManager>().EndPhaseButton);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == null)
                    yield return null;
                if (waitForButton.PressedButton != null && waitForButton.PressedButton.GetComponentInChildren<Text>().text == "Attack")
                {
                    selection = BattlePhaseAction.Attack;
                    selection2 = selectedCard;
                    POW.transform.localPosition = new Vector3(-382, 0, 0);
                    POW.GetComponent<POWSLD>().SetCount(UnitSlots.GetComponent<UnitSlots>().GetUnitSlot(selectedUnit).GetComponent<UnitSlotBehavior>()._power);
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
            Inputs newInput = new Inputs();
            newInput.input1 = selection;
            newInput.input2 = selection2;
            playerManager.CmdInputMade(isServer, newInput);
            inputQueue.Enqueue(newInput);
            readyToContinue = true;
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
            while (inputQueue.Count < 1)
                yield return null;
            readyToContinue = true;
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
            Inputs newInput = new Inputs();
            newInput.input1 = selectedUnit;
            playerManager.CmdInputMade(isServer, newInput);
            inputQueue.Enqueue(newInput);
            readyToContinue = true;
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
            while (inputQueue.Count < 1)
                yield return null;
            readyToContinue = true;
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
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Select unit to attack.";
            cancelButton.transform.localPosition = new Vector3(0, -110, 0);
            waitForButton.AddButton(cancelButton);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == null)
                    yield return null;
                if (waitForButton.PressedButton != null && waitForButton.PressedButton.GetComponentInChildren<Text>().text == "Attack")
                {
                    selection = selectedCard;
                    UnitSlots.GetComponent<UnitSlots>().Reset();
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
            Inputs newInput = new Inputs();
            newInput.input1 = selection;
            newInput.input2 = selection2;
            playerManager.CmdInputMade(isServer, newInput);
            inputQueue.Enqueue(newInput);
            readyToContinue = true;
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
            while (inputQueue.Count < 1)
                yield return null;
            readyToContinue = true;
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
            waitForButton.AddButton(selectionButton1);
            waitForButton.AddButton(selectionButton2);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == null)
                    yield return null;
                if (waitForButton.PressedButton == selectionButton1)
                    selection = GuardStepAction.Guard;
                else if (waitForButton.PressedButton == selectionButton2)
                    selection = GuardStepAction.End;
                else if (waitForButton.PressedButton != null && waitForButton.PressedButton.GetComponentInChildren<Text>().text == "Intercept")
                {
                    selection = GuardStepAction.Intercept;
                    selection2 = selectedCard;
                }
                else if (waitForButton.PressedButton != null && waitForButton.PressedButton.GetComponentInChildren<Text>().text == "Activate")
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
            Inputs newInput = new Inputs();
            newInput.input1 = selection;
            newInput.input2 = selection2;
            playerManager.CmdInputMade(isServer, newInput);
            inputQueue.Enqueue(newInput);
            readyToContinue = true;
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
            while (inputQueue.Count < 1)
                yield return null;
            readyToContinue = true;
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
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = query;
            while (selection < 0)
            {
                if (waitForButton.PressedButton == null)
                    yield return null;
                if (waitForButton.PressedButton != null && waitForButton.PressedButton.GetComponentInChildren<Text>().text == "Select")
                {
                    selection = selectedCard;
                    UnitSlots.GetComponent<UnitSlots>().Reset();
                }
                yield return null;
            }
            waitForButton.Reset();
            while (!NetworkClient.ready)
                yield return null;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            Inputs newInput = new Inputs();
            newInput.input1 = selection;
            newInput.input2 = selection2;
            playerManager.CmdInputMade(isServer, newInput);
            inputQueue.Enqueue(newInput);
            readyToContinue = true;
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
            while (inputQueue.Count < 1)
                yield return null;
            readyToContinue = true;
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
            waitForButton.AddButton(selectionButton1);
            waitForButton.AddButton(selectionButton2);
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Choose an option.";
            while (selection < 0)
            {
                if (waitForButton.PressedButton == null)
                    yield return null;
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
            Inputs newInput = new Inputs();
            newInput.input1 = selection;
            newInput.input2 = selection2;
            playerManager.CmdInputMade(isServer, newInput);
            inputQueue.Enqueue(newInput);
            readyToContinue = true;
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
            while (inputQueue.Count < 1)
                yield return null;
            readyToContinue = true;
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
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Select " + count + " circle(s).";
            selectedUnit = -1;
            List<int> selectedCircles = new List<int>();
            while (selectedCircles.Count < count)
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
            Inputs newInput = new Inputs();
            newInput.inputs.AddRange(selectedCircles);
            playerManager.CmdInputMade(isServer, newInput);
            inputQueue.Enqueue(newInput);
            readyToContinue = true;
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
            while (inputQueue.Count < 1)
                yield return null;
            readyToContinue = true;
        }
    }

    //public void ResetReceive()
    //{
    //    IEnumerator Dialog()
    //    {
    //        inputSignal = 0;
    //        Debug.Log("changed to false");
    //        while (!NetworkClient.ready)
    //        {
    //            Debug.Log("not ready");
    //            yield return null;
    //        }
    //        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
    //        playerManager = networkIdentity.GetComponent<PlayerManager>();
    //        Debug.Log("starting CmdReady");
    //        playerManager.CmdReady();
    //    }
    //    StartCoroutine(Dialog());
    //}

    public List<Button> GetMiscellaneousButtons()
    {
        List<Button> buttons = new List<Button>();
        for (int i = 0; i <= Buttons.transform.childCount; i++)
            buttons.Add(Buttons.transform.GetChild(0).GetComponent<Button>());
        return buttons;
    }

    public void ResetMiscellaneousButtons()
    {
        GameObject button;
        while (Buttons.transform.childCount > 0)
        {
            button = Buttons.transform.GetChild(0).gameObject;
            button.transform.SetParent(null);
            if (button.name.Contains("cardButton"))
            {
                waitForButton.RemoveButton(button.GetComponent<Button>());
                GameObject.Destroy(button);
            }
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

    public Button CreateNewButton()
    {
        Button newButton = GameObject.Instantiate(cardButtonPrefab).GetComponent<Button>();
        waitForButton.AddButton(newButton);
        return newButton;
    }

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
            Button newButton = CreateNewButton();
            newButton.transform.SetParent(Buttons.transform);
            newButton.GetComponentInChildren<Text>().text = "Free";
        }
        if (!cardFightManager.inAnimation && pile.gameObject.name == "PlayerDropZone" && pile.selectable && inputSignal == InputType.SelectMainPhaseAction)
        {
            Button newButton = CreateNewButton();
            newButton.transform.SetParent(Buttons.transform);
            newButton.GetComponentInChildren<Text>().text = "Activate";
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
                cardViewer.AddCardSelectItem(item.Item1.tempID, item.Item1.id, item.Item1.name, item.Item2, true, false, true, "", "");
            }
        }
        else if (selectedGameObject.GetComponent<UnitSlotBehavior>() != null)
        {
            cardViewer.Show();
            cardViewer.Initialize("Soul", 0, 0);
            foreach (Card card in selectedGameObject.GetComponent<UnitSlotBehavior>()._soul)
            {
                cardViewer.AddCardSelectItem(card.tempID, card.id, card.name, true, true, false, true, "", "");
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
            Button newButton = CreateNewButton();
            newButton.transform.GetComponentInChildren<Text>().text = "Ride";
            newButton.transform.SetParent(Buttons.transform);
            Debug.Log(card.name);
            selectedCard = Int32.Parse(card.name);
        }
        else if (inputSignal == InputType.SelectMainPhaseAction || inputSignal == InputType.SelectGuardStepAction)
        {
            Debug.Log(card.name);
            selectedCard = Int32.Parse(card.name);
            if (tempIDs.Contains(Int32.Parse(card.name)))
            {
                Button newButton = CreateNewButton();
                newButton.transform.GetComponentInChildren<Text>().text = "Call";
                newButton.transform.SetParent(Buttons.transform);
            }
            if (tempIDs2.Contains(Int32.Parse(card.name)))
            {
                Button newButton = CreateNewButton();
                newButton.transform.GetComponentInChildren<Text>().text = "Activate";
                newButton.transform.SetParent(Buttons.transform);
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
                    Button newButton = CreateNewButton();
                    newButton.transform.GetComponentInChildren<Text>().text = "Move";
                    newButton.transform.SetParent(Buttons.transform);
                    Debug.Log(unit.name);
                    selectedCard = Int32.Parse(unit.name);
                    selectedUnit = unitSlot;
                    if (selected)
                    {
                        Button newButton2 = CreateNewButton();
                        newButton2.transform.GetComponentInChildren<Text>().text = "Activate";
                        newButton2.transform.SetParent(Buttons.transform);
                        Debug.Log(unit.name + " selected for ACT");
                    }
                    if (unitSlot == Globals.Instance.unitSlots.PlayerVGIndex && canActivateFromSoul)
                    {
                        Button newButton2 = CreateNewButton();
                        newButton2.transform.GetComponentInChildren<Text>().text = "Activate Soul";
                        newButton2.transform.SetParent(Buttons.transform);
                    }
                }
            }
            if (inputSignal == InputType.SelectBattlePhaseAction || inputSignal == InputType.SelectUnitToAttack)
            {
                if (unit != null && selected)
                {
                    Button newButton = CreateNewButton();
                    newButton.transform.GetComponentInChildren<Text>().text = "Attack";
                    newButton.transform.SetParent(Buttons.transform);
                    Debug.Log(unit.name);
                    selectedCard = Int32.Parse(unit.name);
                    selectedUnit = unitSlot;
                }
            }
            if (inputSignal == InputType.SelectActiveUnit)
            {
                if (unit != null && selected)
                {
                    Button newButton = CreateNewButton();
                    newButton.transform.GetComponentInChildren<Text>().text = "Select";
                    newButton.transform.SetParent(Buttons.transform);
                    Debug.Log(unit.name);
                    selectedCard = Int32.Parse(unit.name);
                    selectedUnit = unitSlot;
                }
            }
            if (inputSignal == InputType.SelectGuardStepAction)
            {
                if (unit != null && selected)
                {
                    Button newButton = CreateNewButton();
                    newButton.transform.GetComponentInChildren<Text>().text = "Intercept";
                    newButton.transform.SetParent(Buttons.transform);
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
        //Debug.Log("resetting positions");
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

public class Inputs
{
    public int input1 = -1;
    public int input2 = -1;
    public int input3 = -1;
    public List<int> inputs = new List<int>();
}
