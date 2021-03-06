using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using VanguardEngine;

public class UnitSlotBehavior : MonoBehaviour
{
    public int _grade;
    public Text Grade;
    public Text Soul;
    public int _critical;
    public Text Critical;
    public int _power;
    public int _originalPower;
    public int _shield;
    public Text Power;
    public bool _upright = true;
    public bool _faceup = true;
    public string _cardID = "";
    public GameObject unit = null;
    public int slot;
    public bool inAnimation = false;
    public int _FL;
    public List<Card> _soul;
    public GameObject leftArm;
    public GameObject rightArm;
    public GameObject cardStates;
    public GameObject cardStatePrefab;
    bool showStats = false;

    private void Start()
    {
        _soul = new List<Card>();
    }

    private void Update()
    {
        if (showStats)
        {
            if (Globals.Instance.cardFightManager._recordedUnitValues.ContainsKey(_FL))
            {
                //_power = Globals.Instance.cardFightManager._recordedUnitValues[_FL].currentPower;
                //_critical = Globals.Instance.cardFightManager._recordedUnitValues[_FL].currentCritical;
            }
            Power.text = _power.ToString();
            if (_power > _originalPower)
                Power.color = Color.cyan;
            else if (_power < _originalPower)
                Power.color = Color.red;
            else
                Power.color = Color.white;
            if (_power >= 100000000)
                Power.fontSize = 40;
            else
                Power.fontSize = 66;
            Critical.text = "C:" + _critical.ToString();
            if (_critical < 1)
                Critical.color = Color.red;
            else if (_critical > 1)
                Critical.color = Color.cyan;
            else
                Critical.color = Color.white;
            Soul.text = "S:" + _soul.Count;
        }
        else
        {
            Power.text = "";
            Critical.text = "";
            Soul.text = "";
            Grade.text = "";
        }
    }

    public void Initialize(int FL)
    {
        _FL = FL;
    }

    public void AddCard(int grade, int critical, int power, int originalPower, bool upright, bool faceup, string cardID, GameObject card)
    {
        showStats = true;
        _grade = grade;
        _critical = critical;
        _power = power;
        _originalPower = originalPower;
        _upright = upright;
        _faceup = faceup;
        _cardID = cardID;
        if (unit != null)
        {
            unit.transform.SetParent(null);
            GameObject.Destroy(unit);
        }
        unit = card;
        Debug.Log("adding card: " + unit.name + ", cardID: " + cardID);
        unit.transform.SetParent(this.transform);
        Debug.Log("parent of card: " + unit.transform.parent.name);
        unit.transform.SetAsFirstSibling();
        unit.transform.localPosition = new Vector3(0, 0, 0);
        unit.transform.localScale = Vector3.one / (float)1.1;
        unit.transform.localRotation = Quaternion.Euler(Vector3.zero);
        if (!upright)
            unit.transform.Rotate(new Vector3(0, 0, -90));
        if (this.transform.name.Contains("Enemy")) 
            unit.transform.Rotate(new Vector3(0, 0, 180));
        Grade.text = "G" + _grade;
        Soul.text = "S:" + _soul;
        Power.text = _power.ToString();
        Critical.text = "C:" + _critical;
        if (_faceup)
        {
            unit.GetComponent<Image>().sprite = CardFightManager.LoadSprite(CardFightManager.FixFileName(_cardID));
            Grade.enabled = true;
            Soul.enabled = true;
            Power.enabled = true;
            Critical.enabled = true;
        }
        else
        {
            unit.GetComponent<Image>().sprite = CardFightManager.LoadSprite(Application.dataPath + "/../cardart/FaceDownCard.jpg");
            Grade.enabled = false;
            Soul.enabled = false;
            Power.enabled = false;
            Critical.enabled = false;
        }
    }

    public void AddArm(bool left, string cardID, GameObject card)
    {
        Debug.Log("adding arm");
        GameObject arm;
        if (left)
            arm = leftArm;
        else
            arm = rightArm;
        if (arm != null)
        {
            arm.transform.SetParent(null);
            GameObject.Destroy(arm);
        }
        arm = card;
        arm.transform.SetParent(this.transform);
        arm.transform.SetSiblingIndex(0);
        if (this.transform.name.Contains("Enemy"))
            arm.transform.Rotate(new Vector3(0, 0, 180));
        if (left)
        {
            if (this.transform.name.Contains("Enemy"))
                arm.transform.localPosition = new Vector2(unit.transform.localPosition.x - unit.GetComponent<RectTransform>().sizeDelta.x * unit.transform.localScale.x, unit.transform.localPosition.y);
            else
                arm.transform.localPosition = new Vector2(unit.transform.localPosition.x + unit.GetComponent<RectTransform>().sizeDelta.x * unit.transform.localScale.x, unit.transform.localPosition.y);
        }
        else
        {
            if (this.transform.name.Contains("Enemy"))
                arm.transform.localPosition = new Vector2(unit.transform.localPosition.x + unit.GetComponent<RectTransform>().sizeDelta.x * unit.transform.localScale.x, unit.transform.localPosition.y);
            else
                arm.transform.localPosition = new Vector2(unit.transform.localPosition.x - unit.GetComponent<RectTransform>().sizeDelta.x * unit.transform.localScale.x, unit.transform.localPosition.y);
        }
        if (left)
            leftArm = arm;
        else
            rightArm = arm;
    }

    public GameObject RemoveArm(int tempID)
    {
        Debug.Log("removing arm");
        GameObject arm;
        if (leftArm != null && leftArm.name == tempID.ToString())
            arm = leftArm;
        else if (rightArm != null && rightArm.name == tempID.ToString())
            arm = rightArm;
        else
            return null;
        Vector2 currentPosition = arm.transform.position;
        arm.transform.SetParent(GameObject.Find("Field").transform);
        arm.transform.position = currentPosition;
        return arm;
    }

    public void ChangeUnit(GameObject card)
    {
        unit.transform.SetParent(null);
        unit = card;
        unit.transform.SetParent(this.transform);
        unit.transform.SetAsFirstSibling();
        unit.transform.localPosition = new Vector3(0, 0, 0);
        if (this.transform.name.Contains("Enemy"))
            unit.transform.Rotate(new Vector3(0, 0, 180));
    }

    public GameObject RemoveCard(int tempID)
    {
        GameObject removedCard = null;
        if (unit != null && tempID == Int32.Parse(unit.name))
        {
            showStats = false;
            Debug.Log("UnitSlotBehavior is removing " + tempID.ToString());
            _soul.Clear();
            removedCard = unit;
            Vector3 currentPosition = removedCard.transform.position;
            unit.transform.SetParent(GameObject.Find("Field").transform);
            unit.transform.position = currentPosition;
            unit = null;
        }
        while (cardStates.transform.childCount > 0)
        {
            GameObject child = cardStates.transform.GetChild(0).gameObject;
            child.transform.SetParent(null);
            GameObject.Destroy(child);
        }
        return removedCard;
    }

    public void DisableCard()
    {
        showStats = false;
    }


    public IEnumerator Flip()
    {
        inAnimation = true;
        float step = 400 * Time.deltaTime;
        Quaternion Ninety = Quaternion.Euler(unit.transform.localRotation.eulerAngles.x, unit.transform.localRotation.eulerAngles.y + 90, unit.transform.localRotation.eulerAngles.z);
        Quaternion Zero = Quaternion.Euler(unit.transform.localRotation.eulerAngles.x, unit.transform.localRotation.eulerAngles.y, unit.transform.localRotation.eulerAngles.z);
        Debug.Log("current: " + unit.transform.localRotation.eulerAngles.ToString());
        Debug.Log("ninety: " + Ninety.eulerAngles.ToString());
        Debug.Log("zero: " + Zero.eulerAngles.ToString());
        while (Quaternion.Angle(unit.transform.localRotation, Ninety) > 0.01f)
        {
            unit.transform.localRotation = Quaternion.RotateTowards(unit.transform.localRotation, Ninety, step);
            yield return null;
        }
        unit.transform.localRotation = Ninety;
        if (_faceup)
        {
            _faceup = false;
            unit.GetComponent<Image>().sprite = CardFightManager.LoadSprite("../cardart/FaceDownCard.jpg");
            Grade.enabled = false;
            Soul.enabled = false;
            Power.enabled = false;
            Critical.enabled = false;
        }
        else
        {
            _faceup = true;
            unit.GetComponent<Image>().sprite = CardFightManager.LoadSprite(CardFightManager.FixFileName(_cardID));
            Grade.enabled = true;
            Soul.enabled = true;
            Power.enabled = true;
            Critical.enabled = true;
        }
        while (Quaternion.Angle(unit.transform.localRotation, Zero) > 0.01f)
        {
            unit.transform.localRotation = Quaternion.RotateTowards(unit.transform.localRotation, Zero, step);
            yield return null;
        }
        unit.transform.localRotation = Zero;
        inAnimation = false;
    }

    public IEnumerator Rotate(bool upright)
    {
        Debug.Log("unit slot behavior rotate");
        inAnimation = true;
        float step = 400 * Time.deltaTime;
        Quaternion Ninety = Quaternion.Euler(unit.transform.localRotation.eulerAngles.x, unit.transform.localRotation.eulerAngles.y + 90, unit.transform.localRotation.eulerAngles.z);
        Quaternion Zero = Quaternion.Euler(unit.transform.localRotation.eulerAngles.x, unit.transform.localRotation.eulerAngles.y, unit.transform.localRotation.eulerAngles.z);
        if (_upright && !upright)
        {
            Ninety = Quaternion.Euler(unit.transform.localRotation.eulerAngles.x, unit.transform.localRotation.eulerAngles.y, unit.transform.localRotation.eulerAngles.z - 90);
        }
        else if (!_upright && upright)
        {
            Ninety = Quaternion.Euler(unit.transform.localRotation.eulerAngles.x, unit.transform.localRotation.eulerAngles.y, unit.transform.localRotation.eulerAngles.z + 90);
        }
        else
        {
            Debug.Log("already rotated");
            inAnimation = false;
            yield break;
        }
        while (Quaternion.Angle(unit.transform.localRotation, Ninety) > 0.01f)
        {
            unit.transform.localRotation = Quaternion.RotateTowards(unit.transform.localRotation, Ninety, step);
            yield return null;
        }
        unit.transform.localRotation = Ninety;
        _upright = upright;
        inAnimation = false;
    }

    public void RemoveFromSoul(Card card)
    {
        foreach (Card c in _soul)
        {
            if (c.tempID == card.tempID)
            {
                _soul.Remove(c);
                return;
            }
        }
    }

    public bool ExistsInSoul(int tempID)
    {
        foreach (Card card in _soul)
        {
            if (tempID == card.tempID)
                return true;
        }
        return false;
    }

    public bool IsUnit(int tempID)
    {
        if (unit != null && Int32.Parse(unit.name) == tempID)
            return true;
        return false;
    }

    public void UpdateCardState(int cardState, bool exists)
    {
        string cardStateString = "";
        if (cardState == CardState.Friend)
            cardStateString = "Friend";
        else
            return;
        if (exists)
            AddCardState(cardStateString);
        else
            RemoveCardState(cardStateString);
    }

    public void RemoveCardState(string cardState)
    {
        GameObject child;
        for (int i = 0; i < cardStates.transform.childCount; i++)
        {
            child = cardStates.transform.GetChild(i).gameObject;
            if (child.GetComponent<Text>().text == cardState)
            {
                child.transform.SetParent(null);
                GameObject.Destroy(child);
                break;
            }
        }
    }

    public void AddCardState(string cardState)
    {
        GameObject child;
        for (int i = 0; i < cardStates.transform.childCount; i++)
        {
            child = cardStates.transform.GetChild(i).gameObject;
            if (child.GetComponent<Text>().text == cardState)
                return;
        }
        child = GameObject.Instantiate(cardStatePrefab);
        child.GetComponent<Text>().text = cardState;
        child.transform.SetParent(cardStates.transform);
    }
}
