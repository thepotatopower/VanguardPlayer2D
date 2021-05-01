using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VanguardEngine;
using UnityEngine.UI;
using Mirror;
using System.Threading;

public class VisualInputManager : NetworkBehaviour
{ 
    public Button rockButton;
    public Button scissorsButton;
    public Button paperButton;
    public GameObject messageBox;
    public IM inputManager;
    public Thread currentThread;
    public PlayerManager playerManager;
    [SyncVar]
    public int player1_input = -1;
    [SyncVar]
    public int player2_input = -1;
    [SyncVar]
    public int inputSignal = -1;
    [SyncVar]
    public int numResponses = 0;
    [SyncVar]
    public bool readyToContinue = false;
    bool reversed = false;
    public bool receivedInput = false;

    //VanguardEngine's InputManager logic
    public class IM : InputManager
    {
        public VisualInputManager inputManager;

        protected override void RPS_Input()
        {
            while (inputManager.inputSignal > 0) ;
            if (inputManager.player1_input >= 0 && inputManager.player2_input >= 0)
            {
                inputManager.inputSignal = InputType.ResolveRPS;
                int_input = inputManager.player2_input;
                Debug.Log("second RPS input: " + int_input.ToString());
                oSignalEvent.Set();
                return;
            }
            inputManager.inputSignal = InputType.RPS;
            //NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            //PlayerManager playerManager = networkIdentity.GetComponent<PlayerManager>();
            //int selection;
            //if (inputManager.ReceivedInputs())
            //{
            //    //RpcResolveRPS();
            //    selection = inputManager.player2_input;
            //    inputManager.ResetInputs();
            //    int_input = selection;
            //    oSignalEvent.Set();
            //}
            //inputManager.ResetInputs();
            //playerManager.CmdInitiateRPS();
            while (!inputManager.readyToContinue) ;
            inputManager.readyToContinue = false;
            int_input = inputManager.player1_input;
            Debug.Log("first RPS input: " + int_input.ToString());
            oSignalEvent.Set();
        }
    }

    // VanguardPlayer2D's InputManager logic
    void Start()
    {
        currentThread = Thread.CurrentThread;
        rockButton = GameObject.Find("RockButton").GetComponent<Button>();
        rockButton.transform.position = new Vector3(10000, 0, 0);
        paperButton = GameObject.Find("PaperButton").GetComponent<Button>();
        paperButton.transform.position = new Vector3(10000, 0, 0);
        scissorsButton = GameObject.Find("ScissorsButton").GetComponent<Button>();
        scissorsButton.transform.position = new Vector3(10000, 0, 0);
        messageBox = GameObject.Find("MessageBox");
        messageBox.transform.position = new Vector3(10000, 0, 0);
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
    }

    public void ResetInputs()
    {
        player1_input = -1;
        player2_input = -1;
        rockButton.transform.position = new Vector3(10000, 0, 0);
        paperButton.transform.position = new Vector3(10000, 0, 0);
        scissorsButton.transform.position = new Vector3(10000, 0, 0);
        messageBox.transform.position = new Vector3(10000, 0, 0);
    }

    public bool ReceivedInputs()
    {
        if (player1_input > 0 || player2_input > 0)
            return true;
        return false;
    }

    public IM InitializeInputManager(PlayerManager pm)
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
            Debug.Log("waiting for button");
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
            OnSelectionMade(selection);
        }
        Debug.Log("waiting for button");
        StartCoroutine(Dialog());
    }

    public void OnSelectionMade(int selection)
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
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
        rockButton.transform.position = new Vector3(10000, 0, 0);
        paperButton.transform.position = new Vector3(10000, 0, 0);
        scissorsButton.transform.position = new Vector3(10000, 0, 0);
        messageBox.transform.localPosition = new Vector3(0, 0, 0);
    }

    public void ResolveRPS()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        playerManager.CmdChangeInput(1, 0);
        ResetInputs();
        //Button player_selection;
        //Button enemy_selection;
        //messageBox.SetActive(false);
        //if (isServer)
        //{
        //    if (player1_input == 0)
        //        player_selection = rockButton;
        //    else if (player1_input == 1)
        //        player_selection = scissorsButton;
        //    else
        //        player_selection = paperButton;
        //    if (player2_input == 0)
        //        enemy_selection = rockButton;
        //    else if (player2_input == 1)
        //        enemy_selection = scissorsButton;
        //    else
        //        enemy_selection = paperButton;
        //}
        //else
        //{
        //    if (player2_input == 0)
        //        player_selection = rockButton;
        //    else if (player2_input == 1)
        //        player_selection = scissorsButton;
        //    else
        //        player_selection = paperButton;
        //    if (player1_input == 0)
        //        enemy_selection = rockButton;
        //    else if (player1_input == 1)
        //        enemy_selection = scissorsButton;
        //    else
        //        enemy_selection = paperButton;
        //}
        //player_selection.transform.localPosition = new Vector3(0, -50, 0);
        //enemy_selection.transform.localPosition = new Vector3(0, 50, 0);
        //IEnumerator MoveTowards()
        //{
        //    while (player_selection.transform.position != new Vector3(0, 0, 0) && enemy_selection.transform.position != new Vector3(0, 0, 0))
        //    {
        //        float step = 1 * Time.deltaTime;
        //        player_selection.transform.position = Vector3.MoveTowards(player_selection.transform.position, new Vector3(0, 0, 0), step);
        //        player_selection.transform.position = Vector3.MoveTowards(player_selection.transform.position, new Vector3(0, 0, 0), step);
        //        yield return null;
        //    }
        //    yield return new WaitForSeconds(1);
        //}
        //StartCoroutine(MoveTowards());
        //player_selection.transform.position = new Vector3(10000, 0, 0);
        //enemy_selection.transform.position = new Vector3(10000, 0, 0);
    }
}
