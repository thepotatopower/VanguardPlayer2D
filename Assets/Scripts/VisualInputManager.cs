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
    public Button toggleCardSelect;
    public Button cardButton;
    public Button rideFromRideDeck;
    public Button cancelButton;
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
    public CardSelect cardSelect;
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
    [SyncVar]
    public int inputSignal = -1;
    [SyncVar]
    public int numResponses = 0;
    [SyncVar]
    public int input1 = 0;
    [SyncVar]
    public int input2 = 0;
    [SyncVar]
    public bool readyToContinue = false;
    [SyncVar]
    public bool reversed = false;
    [SyncVar]
    public int min = 0;
    [SyncVar]
    public string query = "";
    [SyncVar]
    public bool bool1 = false;
    [SyncVar]
    public int int1 = 0;
    public SyncList<int> inputs = new SyncList<int>();
    public SyncList<int> tempIDs = new SyncList<int>();
    public SyncList<string> cardIDs = new SyncList<string>();
    public SyncList<string> strings = new SyncList<string>();
    public bool receivedInput = false;
    public bool cardsAreSelectable = false;
    public bool cardsAreHoverable = true;
    public List<int> selectedCards;
    public UnityEvent m_myEvent;
    public EventSystem eventSystem;
    int selectedCard = -1;
    int selectedUnit = -1;
    List<Button> miscellaneousButtons;
    bool clicked = false;
    WaitForUIButtons waitForButton;

    //VanguardEngine's InputManager logic
    public class IM : InputManager
    {
        public VisualInputManager inputManager;

        public override void SwapPlayers()
        {
            if (inputManager.reversed)
                inputManager.reversed = false;
            else
                inputManager.reversed = true;
            Debug.Log("swapped");
            base.SwapPlayers();
        }

        protected override void RPS_Input()
        {
            Debug.Log("RPS_Input started");
            if (inputManager.player1_input >= 0 && inputManager.player2_input >= 0)
            {
                int_input = inputManager.player2_input;
                Debug.Log("second RPS input: " + int_input.ToString());
                inputManager.inputSignal = InputType.ResolveRPS;
                while (!inputManager.readyToContinue) ;
                inputManager.readyToContinue = false;
                oSignalEvent.Set();
            }
            else
            {
                inputManager.inputSignal = InputType.RPS;
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
            inputManager.inputSignal = InputType.YesNo;
            inputManager.query = string_input;
            if (string_input == "Boost?")
                inputManager.int1 = _player1.GetBooster(_player1.GetAttacker().tempID);
            while (!inputManager.readyToContinue) ;
            inputManager.readyToContinue = false;
            if (inputManager.player1_input == 0)
                bool_input = true;
            else
                bool_input = false;
            Debug.Log("selection made: " + int_input);
            inputManager.inputSignal = InputType.Reset;
            while (!inputManager.readyToContinue) ;
            inputManager.readyToContinue = false;
        }

        protected override void SelectCardsToMulligan_Input()
        {
            Thread.Sleep(250); //need to refine this later
            intlist_input.Clear();
            inputManager.inputSignal = InputType.Mulligan;
            while (!inputManager.readyToContinue) ;
            inputManager.readyToContinue = false;
            foreach (int input in inputManager.inputs)
                intlist_input.Add(input);
            inputManager.inputs.Clear();
            inputManager.inputSignal = InputType.Reset;
            while (!inputManager.readyToContinue) ;
            inputManager.readyToContinue = false;
            oSignalEvent.Set();
        }

        protected override void SelectFromList_Input()
        {
            Thread.Sleep(250);
            string location;
            inputManager.count = int_value;
            inputManager.min = int_value2;
            inputManager.query = _query;
            inputManager.cardIDs.Clear();
            inputManager.tempIDs.Clear();
            inputManager.strings.Clear();
            intlist_input.Clear();
            foreach (Card card in cardsToSelect)
            {
                inputManager.tempIDs.Add(card.tempID);
                inputManager.cardIDs.Add(card.id);
                location = "<>";
                switch (_player1.GetLocation(card))
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
                }
                inputManager.strings.Add(location);
            }
            inputManager.inputSignal = InputType.SelectFromList;
            WaitForReadyToContinue();
            oSignalEvent.Set();
        }

        protected override void SelectRidePhaseAction_Input()
        {
            inputManager.cardIDs.Clear();
            inputManager.tempIDs.Clear();
            inputManager.bool1 = false;
            if (_player1.CanRideFromHand())
            {
                foreach (Card card in _player1.GetRideableCards(false))
                {
                    inputManager.cardIDs.Add(card.id);
                    inputManager.tempIDs.Add(card.tempID);
                }
            }
            if (_player1.CanRideFromRideDeck())
                inputManager.bool1 = true;
            Thread.Sleep(250);
            inputManager.inputSignal = InputType.SelectRidePhaseAction;
            WaitForReadyToContinue();
            oSignalEvent.Set();
        }

        protected override void SelectMainPhaseAction_Input()
        {
            inputManager.cardIDs.Clear();
            inputManager.tempIDs.Clear();
            inputManager.bool1 = false;
            if (_player1.CanCallRearguard())
            {
                foreach (Card card in _player1.GetCallableRearguards())
                {
                    inputManager.cardIDs.Add(card.id);
                    inputManager.tempIDs.Add(card.tempID);
                }
            }
            Thread.Sleep(250);
            inputManager.inputSignal = InputType.SelectMainPhaseAction;
            WaitForReadyToContinue();
            oSignalEvent.Set();
        }

        protected override void SelectCallLocation_Input()
        {
            Thread.Sleep(250);
            bool proceed = false;
            while (!proceed)
            {
                inputManager.inputSignal = InputType.SelectCallLocation;
                WaitForReadyToContinue();
                if (!_ints.Contains(int_input) && ((_ints2.Count > 0 && _ints2.Contains(int_input)) || _ints2.Count == 0))
                {
                    proceed = true;
                }
            }
        }

        protected override void SelectBattlePhaseAction_Input()
        {
            inputManager.cardIDs.Clear();
            inputManager.tempIDs.Clear();
            inputManager.bool1 = false;
            if (_player1.CanAttack())
            {
                foreach (Card card in _player1.GetCardsToAttackWith())
                {
                    inputManager.cardIDs.Add(card.id);
                    inputManager.tempIDs.Add(card.tempID);
                }
            }
            Thread.Sleep(250);
            inputManager.inputSignal = InputType.SelectBattlePhaseAction;
            WaitForReadyToContinue();
            oSignalEvent.Set();
        }

        protected override void SelectUnitToAttack_Input()
        {
            inputManager.cardIDs.Clear();
            inputManager.tempIDs.Clear();
            inputManager.bool1 = false;
            foreach (Card card in _player1.GetPotentialAttackTargets())
            {
                inputManager.cardIDs.Add(card.id);
                inputManager.tempIDs.Add(card.tempID);
            }
            Thread.Sleep(250);
            inputManager.inputSignal = InputType.SelectUnitToAttack;
            WaitForReadyToContinue();
            oSignalEvent.Set();
        }

        protected override void SelectGuardPhaseAction_Input()
        {
            if (_player1.CanGuard())
                inputManager.bool1 = true;
            else
                inputManager.bool1 = false;
            Thread.Sleep(250);
            inputManager.inputSignal = InputType.SelectGuardStepAction;
            WaitForReadyToContinue();
        }

        public void WaitForReadyToContinue()
        {
            while (!inputManager.readyToContinue) ;
            inputManager.readyToContinue = false;
            foreach (int input in inputManager.inputs)
                intlist_input.Add(input);
            Debug.Log("received input 1: " + inputManager.input1);
            Debug.Log("received input 2: " + inputManager.input2);
            int_input = inputManager.input1;
            int_input2 = inputManager.input2;
            inputManager.inputs.Clear();
            inputManager.inputSignal = InputType.Reset;
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
        mulliganButton = GameObject.Find("Mulligan").GetComponent<Button>();
        mulliganButton.transform.position = new Vector3(10000, 0, 0);
        yesButton = GameObject.Find("YesButton").GetComponent<Button>();
        yesButton.transform.position = new Vector3(10000, 0, 0);
        noButton = GameObject.Find("NoButton").GetComponent<Button>();
        noButton.transform.position = new Vector3(10000, 0, 0);
        toggleCardSelect = GameObject.Find("ToggleCardSelect").GetComponent<Button>();
        toggleCardSelect.transform.position = new Vector3(10000, 0, 0);
        cardButton.transform.position = new Vector3(10000, 0, 0);
        rideFromRideDeck.transform.position = new Vector3(10000, 0, 0);
        cancelButton.transform.position = new Vector3(10000, 0, 0);
        POW.transform.position = new Vector3(10000, 0, 0);
        SLD.transform.position = new Vector3(10000, 0, 0);
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
        if (Thread.CurrentThread == currentThread && !receivedInput && inputSignal > 0)
        {
            switch (inputSignal)
            {
                case InputType.Reset:
                    receivedInput = true;
                    ResetInputs();
                    break;
                case InputType.RPS:
                    Debug.Log("getting RPS");
                    receivedInput = true;
                    GetRPSInput();
                    break;
                case InputType.ResolveRPS:
                    receivedInput = true;
                    ResolveRPS();
                    break;
                case InputType.Mulligan:
                    receivedInput = true;
                    StartCoroutine(Mulligan());
                    break;
                case InputType.YesNo:
                    receivedInput = true;
                    StartCoroutine(YesNo());
                    break;
                case InputType.SelectCardsFromHand:
                    receivedInput = true;
                    StartCoroutine(SelectCardsFromHand());
                    break;
                case InputType.SelectFromList:
                    receivedInput = true;
                    StartCoroutine(SelectFromList());
                    break;
                case InputType.SelectRidePhaseAction:
                    receivedInput = true;
                    StartCoroutine(SelectRidePhaseAction());
                    break;
                case InputType.SelectMainPhaseAction:
                    receivedInput = true;
                    StartCoroutine(SelectMainPhaseAction());
                    break;
                case InputType.SelectCallLocation:
                    receivedInput = true;
                    StartCoroutine(SelectCallLocation());
                    break;
                case InputType.SelectBattlePhaseAction:
                    receivedInput = true;
                    StartCoroutine(SelectBattlePhaseAction());
                    break;
                case InputType.SelectUnitToAttack:
                    receivedInput = true;
                    StartCoroutine(SelectUnitToAttack());
                    break;
                case InputType.SelectGuardStepAction:
                    receivedInput = true;
                    StartCoroutine(SelectGuardStepAction());
                    break;
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            foreach (Button button in miscellaneousButtons)
            {
                if (button.transform.position.x < 5000 && eventSystem.currentSelectedGameObject != button.gameObject)
                {
                    ResetMiscellaneousButtons();
                    break;
                }

            }
        }
    }

    public bool isActingPlayer()
    {
        if (isServer)
        {
            if (reversed)
                return false;
            return true;
        }
        else
        {
            if (reversed)
                return true;
            return false;
        }
    }

    public class InputType
    {
        public const int Reset = 1;
        public const int RPS = 2;
        public const int ResolveRPS = 3;
        public const int Mulligan = 4;
        public const int YesNo = 5;
        public const int SelectCardsFromHand = 8;
        public const int SelectFromList = 9;
        public const int SelectRidePhaseAction = 10;
        public const int SelectMainPhaseAction = 11;
        public const int SelectCallLocation = 12;
        public const int SelectBattlePhaseAction = 13;
        public const int SelectUnitToAttack = 14;
        public const int SelectGuardStepAction = 15;
    }

    public void ResetInputs()
    {
        player1_input = -1;
        player2_input = -1;
        bool1 = false;
        clicked = false;
        if (waitForButton != null)
        {
            waitForButton.Reset();
            waitForButton.Dispose();
        }
        rockButton.transform.position = new Vector3(10000, 0, 0);
        paperButton.transform.position = new Vector3(10000, 0, 0);
        scissorsButton.transform.position = new Vector3(10000, 0, 0);
        messageBox.transform.position = new Vector3(10000, 0, 0);
        mulliganButton.transform.position = new Vector3(10000, 0, 0);
        yesButton.transform.position = new Vector3(10000, 0, 0);
        noButton.transform.position = new Vector3(10000, 0, 0);
        toggleCardSelect.transform.position = new Vector3(10000, 0, 0);
        cancelButton.transform.position = new Vector3(10000, 0, 0);
        Globals.Instance.selectionButton1.transform.position = Globals.Instance.ResetPosition;
        Globals.Instance.selectionButton2.transform.position = Globals.Instance.ResetPosition;
        ResetMiscellaneousButtons();
        miscellaneousButtons.Clear();
        cardSelect.Hide();
        cardSelect.ResetItems();
        cardsAreSelectable = false;
        PlayerHand.GetComponent<PlayerHand>().Reset();
        if (isServer)
        {
            Debug.Log("host resetting");
            playerManager.CmdChangeInput(1, -1);
        }
        else
        {
            Debug.Log("remote resetting");
            playerManager.CmdChangeInput(2, -1);
        }
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
            ResetInputs();
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
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            playerManager = networkIdentity.GetComponent<PlayerManager>();
            playerManager.CmdSingleInput(selection);
        }
        else
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Waiting for opponent...";
        }
    }

    IEnumerator SelectCardsFromHand()
    {
        Card card;
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
        if (isActingPlayer())
        {
            cardSelect.Show();
            cardSelect.Initialize("Discard " + count + " card(s).", count, count);
            for (int i = 0; i < tempIDs.Count; i++)
            {
                card = cardFightManager.LookUpCard(cardIDs[i]);
                cardSelect.AddCardSelectItem(tempIDs[i], cardIDs[i], card.name);
            }
            toggleCardSelect.transform.localPosition = new Vector3(0, -300, 0);
            int selection = -1;
            waitForButton = new WaitForUIButtons(cardSelect.SelectButton);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == cardSelect.SelectButton)
                    selection = 0;
                yield return null;
            }
            waitForButton.Reset();
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

    IEnumerator SelectFromList()
    {
        Card card;
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
        if (isActingPlayer())
        {
            cardSelect.Show();
            cardSelect.Initialize(query, min, count);
            for (int i = 0; i < tempIDs.Count; i++)
            {
                card = cardFightManager.LookUpCard(cardIDs[i]);
                cardSelect.AddCardSelectItem(tempIDs[i], cardIDs[i], card.name);
            }
            toggleCardSelect.transform.localPosition = new Vector3(0, -300, 0);
            int selection = -1;
            waitForButton = new WaitForUIButtons(cardSelect.SelectButton);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == cardSelect.SelectButton)
                    selection = 0;
                yield return null;
            }
            waitForButton.Reset();
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

    IEnumerator SelectRidePhaseAction()
    {
        Debug.Log("selecting ride phase action");
        List<int> list = new List<int>();
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
        if (isActingPlayer())
        {
            for (int i = 0; i < tempIDs.Count; i++)
            {
                PlayerHand.GetComponent<PlayerHand>().MarkAsSelectable(tempIDs[i]);
            }
            int selection = -1;
            int selection2 = -1;
            miscellaneousButtons.Clear();
            miscellaneousButtons.Add(rideFromRideDeck);
            miscellaneousButtons.Add(cardButton);
            waitForButton = new WaitForUIButtons(rideFromRideDeck, cardButton, PhaseManager.GetComponent<PhaseManager>().MainPhaseButton);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == rideFromRideDeck)
                    selection = RidePhaseAction.RideFromRideDeck;
                else if (waitForButton.PressedButton == cardButton)
                {
                    selection = RidePhaseAction.RideFromHand;
                    selection2 = selectedCard;
                }
                else if (waitForButton.PressedButton == PhaseManager.GetComponent<PhaseManager>().MainPhaseButton)
                    selection = RidePhaseAction.End;
                yield return null;
            }
            waitForButton.Reset();
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
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
        if (isActingPlayer())
        {
            for (int i = 0; i < tempIDs.Count; i++)
            {
                PlayerHand.GetComponent<PlayerHand>().MarkAsSelectable(tempIDs[i]);
            }
            int selection = -1;
            int selection2 = -1;
            miscellaneousButtons.Clear();
            miscellaneousButtons.Add(cardButton);
            waitForButton = new WaitForUIButtons(cardButton, PhaseManager.GetComponent<PhaseManager>().BattlePhaseButton, PhaseManager.GetComponent<PhaseManager>().EndPhaseButton);
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
                }
                else if ((waitForButton.PressedButton == PhaseManager.GetComponent<PhaseManager>().BattlePhaseButton) ||
                    (waitForButton.PressedButton == PhaseManager.GetComponent<PhaseManager>().EndPhaseButton))
                    selection = MainPhaseAction.End;
                yield return null;
            }
            waitForButton.Reset();
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
        Debug.Log("selecting battle phase action");
        List<int> list = new List<int>();
        while (cardFightManager.InAnimation())
        {
            yield return null;
        }
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
                        UnitSlots.GetComponent<UnitSlots>().Reset();
                    }
                }
                else if (waitForButton.PressedButton == PhaseManager.GetComponent<PhaseManager>().EndPhaseButton)
                    selection = BattlePhaseAction.End;
                yield return null;
            }
            waitForButton.Reset();
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
        clicked = false;
        if (isActingPlayer())
        {
            messageBox.transform.localPosition = new Vector3(0, 0, 0);
            messageBox.transform.GetChild(0).GetComponent<Text>().text = "Select call location.";
            while (!clicked)
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
        if (isActingPlayer())
        {
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
            waitForButton = new WaitForUIButtons(selectionButton1, selectionButton2);
            while (selection < 0)
            {
                if (waitForButton.PressedButton == selectionButton1)
                    selection = GuardStepAction.Guard;
                else if (waitForButton.PressedButton == selectionButton2)
                    selection = GuardStepAction.End;
                yield return null;
            }
            waitForButton.Reset();
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
        inputSignal = 0;
        receivedInput = false;
        Debug.Log("changed to false");
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        playerManager.CmdReady();
    }

    public void ResetMiscellaneousButtons()
    {
        rideFromRideDeck.transform.position = new Vector3(10000, 0, 0);
        cardButton.transform.position = new Vector3(10000, 0, 0);
    }

    public void SpawnRideFromRideDeckButton()
    {
        if (inputSignal == InputType.SelectRidePhaseAction && bool1)
        {
            GameObject rideDeck = GameObject.Find("PlayerRideDeck");
            rideFromRideDeck.transform.position = new Vector3(rideDeck.transform.position.x, rideDeck.transform.position.y + ((rideDeck.transform.localScale.y / (float)2) + (rideFromRideDeck.transform.localScale.y * (float)1.25)), 0);
        }
    }

    public void CardClicked(GameObject card)
    {
        if (inputSignal == InputType.SelectRidePhaseAction && miscellaneousButtons.Contains(cardButton))
        {
            cardButton.transform.GetComponentInChildren<Text>().text = "Ride";
            cardButton.transform.position = new Vector3(card.transform.position.x, card.transform.position.y + 100, 0);
            Debug.Log(card.name);
            selectedCard = Int32.Parse(card.name);
        }
        else if (inputSignal == InputType.SelectMainPhaseAction && miscellaneousButtons.Contains(cardButton))
        {
            cardButton.transform.GetComponentInChildren<Text>().text = "Call";
            cardButton.transform.position = new Vector3(card.transform.position.x, card.transform.position.y + 100, 0);
            Debug.Log(card.name);
            selectedCard = Int32.Parse(card.name);
        }
    }

    public void UnitClicked(int unitSlot, GameObject unit, bool selected)
    {
        if (inputSignal == InputType.SelectCallLocation)
        {
            selectedUnit = unitSlot;
            clicked = true;
        }
        else if (inputSignal == InputType.SelectMainPhaseAction)
        {
            if (unit != null)
            {
                cardButton.transform.GetComponentInChildren<Text>().text = "Move";
                cardButton.transform.position = new Vector3(unit.transform.position.x, unit.transform.position.y + 50, 0);
                Debug.Log(unit.name);
                selectedCard = Int32.Parse(unit.name);
                selectedUnit = unitSlot;
            }
        }
        else if (inputSignal == InputType.SelectBattlePhaseAction || inputSignal == InputType.SelectUnitToAttack)
        {
            if (unit != null && selected)
            {
                cardButton.transform.GetComponentInChildren<Text>().text = "Attack";
                cardButton.transform.position = new Vector3(unit.transform.position.x, unit.transform.position.y + 50, 0);
                Debug.Log(unit.name);
                selectedCard = Int32.Parse(unit.name);
                selectedUnit = unitSlot;
            }
        }
    }
}
