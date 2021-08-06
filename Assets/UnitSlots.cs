using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VanguardEngine;
using System;

public class UnitSlots : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject[] _unitSlots;
    int _playerID;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(int playerID)
    {
        _unitSlots = new GameObject[20];
        _playerID = playerID;
        if (playerID == 1)
        {
            _unitSlots[FL.PlayerFrontLeft] = GameObject.Find("PlayerFrontLeft");
            _unitSlots[FL.PlayerFrontRight] = GameObject.Find("PlayerFrontRight");
            _unitSlots[FL.PlayerBackLeft] = GameObject.Find("PlayerBackLeft");
            _unitSlots[FL.PlayerBackRight] = GameObject.Find("PlayerBackRight");
            _unitSlots[FL.PlayerBackCenter] = GameObject.Find("PlayerBackCenter");
            _unitSlots[FL.PlayerVanguard] = GameObject.Find("PlayerVG");
            _unitSlots[FL.EnemyFrontLeft] = GameObject.Find("EnemyFrontLeft");
            _unitSlots[FL.EnemyFrontRight] = GameObject.Find("EnemyFrontRight");
            _unitSlots[FL.EnemyBackLeft] = GameObject.Find("EnemyBackLeft");
            _unitSlots[FL.EnemyBackRight] = GameObject.Find("EnemyBackRight");
            _unitSlots[FL.EnemyBackCenter] = GameObject.Find("EnemyBackCenter");
            _unitSlots[FL.EnemyVanguard] = GameObject.Find("EnemyVG");
        }
        else
        {
            _unitSlots[FL.PlayerFrontLeft] = GameObject.Find("EnemyFrontLeft");
            _unitSlots[FL.PlayerFrontRight] = GameObject.Find("EnemyFrontRight");
            _unitSlots[FL.PlayerBackLeft] = GameObject.Find("EnemyBackLeft");
            _unitSlots[FL.PlayerBackRight] = GameObject.Find("EnemyBackRight");
            _unitSlots[FL.PlayerBackCenter] = GameObject.Find("EnemyBackCenter");
            _unitSlots[FL.PlayerVanguard] = GameObject.Find("EnemyVG");
            _unitSlots[FL.EnemyFrontLeft] = GameObject.Find("PlayerFrontLeft");
            _unitSlots[FL.EnemyFrontRight] = GameObject.Find("PlayerFrontRight");
            _unitSlots[FL.EnemyBackLeft] = GameObject.Find("PlayerBackLeft");
            _unitSlots[FL.EnemyBackRight] = GameObject.Find("PlayerBackRight");
            _unitSlots[FL.EnemyBackCenter] = GameObject.Find("PlayerBackCenter");
            _unitSlots[FL.EnemyVanguard] = GameObject.Find("PlayerVG");
        }
        for (int i = 0; i < _unitSlots.Length; i++)
        {
            if (_unitSlots[i] != null)
            {
                _unitSlots[i].GetComponent<UnitSlotBehavior>()._FL = i;
                Debug.Log("slot " + i + " is " + _unitSlots[i].GetComponent<UnitSlotBehavior>()._FL);
            }
        }
    }

    public GameObject GetUnitSlot(int fl)
    {
        return _unitSlots[fl];
    }

    public void SwapUnitSlots(int previousFL, int currentFL)
    {
        CardFightManager cardFightManager = GameObject.Find("CardFightManager").GetComponent<CardFightManager>();
        UnitSlotBehavior previous = _unitSlots[previousFL].GetComponent<UnitSlotBehavior>();
        UnitSlotBehavior current = _unitSlots[currentFL].GetComponent<UnitSlotBehavior>();
        if (current.unit == null)
        {
            Debug.Log("current is null");
            current.AddCard(previous._grade, previous._soul, previous._critical, previous._power, previous._rightup, previous._faceup, previous._cardID, cardFightManager.CreateNewCard(previous._cardID, Int32.Parse(previous.unit.name)));
            previous.RemoveCard(previous._cardID);
            return;
        }
        else if (previous.unit == null)
        {
            previous.AddCard(current._grade, current._soul, current._critical, current._power, current._rightup, current._faceup, current._cardID, cardFightManager.CreateNewCard(current._cardID, Int32.Parse(current.unit.name)));
            current.RemoveCard(current._cardID);
            return;
        }
        else if (previous.unit == null && current.unit == null)
            return;
        int tempGrade = previous._grade;
        int tempCritical = previous._critical;
        int tempSoul = previous._soul;
        int tempPower = previous._power;
        string tempCardID = previous._cardID;
        bool tempFaceUp = previous._faceup;
        bool tempUpRight = previous._rightup;
        string tempID = previous.unit.name;
        previous.AddCard(current._grade, current._soul, current._critical, current._power, current._rightup, current._faceup, current._cardID, cardFightManager.CreateNewCard(current._cardID, Int32.Parse(current.unit.name)));
        current.AddCard(tempGrade, tempSoul, tempCritical, tempPower, tempUpRight, tempFaceUp, tempCardID, cardFightManager.CreateNewCard(tempCardID, Int32.Parse(tempID)));
    }

    public void Hide(int FL)
    {
        _unitSlots[FL].SetActive(false);
    }

    public void Show(int FL)
    {
        _unitSlots[FL].SetActive(true);
    }
}
