using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VanguardEngine;

public class PhaseManager : MonoBehaviour
{
    public Button DrawPhaseButton;
    public Button StandPhaseButton;
    public Button RidePhaseButton;
    public Button MainPhaseButton;
    public Button BattlePhaseButton;
    public Button EndPhaseButton;
    bool _isActingPlayer = false;
    int _turnCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        ResetAll();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ResetAll()
    {
        DrawPhaseButton.interactable = false;
        DrawPhaseButton.GetComponent<Image>().color = Color.white;
        StandPhaseButton.interactable = false;
        StandPhaseButton.GetComponent<Image>().color = Color.white;
        RidePhaseButton.interactable = false;
        RidePhaseButton.GetComponent<Image>().color = Color.white;
        MainPhaseButton.interactable = false;
        MainPhaseButton.GetComponent<Image>().color = Color.white;
        BattlePhaseButton.interactable = false;
        BattlePhaseButton.GetComponent<Image>().color = Color.white;
        EndPhaseButton.interactable = false;
        EndPhaseButton.GetComponent<Image>().color = Color.white;
    }

    public void ChangePhase(int phase, bool actingPlayer, int turn)
    {
        _isActingPlayer = actingPlayer;
        _turnCount = turn;
        if (phase == Phase.Draw)
            DrawPhase();
        else if (phase == Phase.Stand)
            StandPhase();
        else if (phase == Phase.Ride)
            RidePhase();
        else if (phase == Phase.Main)
            MainPhase();
        else if (phase == Phase.Battle)
            BattlePhase();
    }

    void DrawPhase()
    {
        ResetAll();
        Debug.Log("phase manager draw phase");
        DrawPhaseButton.GetComponent<Image>().color = Color.green;
        DrawPhaseButton.interactable = true;
    }

    void StandPhase()
    {
        ResetAll();
        StandPhaseButton.GetComponent<Image>().color = Color.green;
        StandPhaseButton.interactable = true;
    }

    void RidePhase()
    {
        ResetAll();
        RidePhaseButton.GetComponent<Image>().color = Color.green;
        RidePhaseButton.interactable = true;
        //if (_isActingPlayer)
        //    MainPhaseButton.interactable = true;
    }

    void MainPhase()
    {
        ResetAll();
        MainPhaseButton.GetComponent<Image>().color = Color.green;
        MainPhaseButton.interactable = true;
        //if (_isActingPlayer && _turnCount > 1)
        //    BattlePhaseButton.interactable = true;
        //if (_isActingPlayer && _turnCount == 1)
        //    EndPhaseButton.interactable = true;
    }

    void BattlePhase()
    {
        Debug.Log("deactivating end phase button");
        ResetAll();
        BattlePhaseButton.GetComponent<Image>().color = Color.green;
        BattlePhaseButton.interactable = true;
        //if (_isActingPlayer)
        //    EndPhaseButton.interactable = true;
    }
}
