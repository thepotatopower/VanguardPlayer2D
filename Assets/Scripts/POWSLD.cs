using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class POWSLD : MonoBehaviour
{
    int _count = 0;
    public Text _text;
    public POWSLD oppositeValue;
    public bool compare = false;
    public bool isPOW = true;
    public bool auto = false;
    public int currentIndex = -1;

    // Start is called before the first frame update

    void Start()
    {
        this.transform.position = Globals.Instance.ResetPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (Globals.Instance.cardFightManager != null && auto)
        {
            if (isPOW)
            {
                if (Globals.Instance.cardFightManager._attacker > 0)
                {
                    this.transform.localPosition = Globals.Instance.POWPosition;
                    _count = Globals.Instance.unitSlots.GetUnitSlot(Globals.Instance.cardFightManager._attacker).GetComponent<UnitSlotBehavior>()._power;
                }
                else
                    this.transform.localPosition = Globals.Instance.ResetPosition;
                if (Globals.Instance.cardFightManager._booster > 0 && !compare)
                    _count += Globals.Instance.unitSlots.GetUnitSlot(Globals.Instance.cardFightManager._booster).GetComponent<UnitSlotBehavior>()._power;
            }
            else
            {
                if (Globals.Instance.cardFightManager._attacked.Count > 0)
                {
                    if (currentIndex < 0)
                        currentIndex = 0;
                    this.transform.localPosition = Globals.Instance.SLDPosition;
                    _count = Globals.Instance.unitSlots.GetUnitSlot(Globals.Instance.cardFightManager._attacked[currentIndex]).GetComponent<UnitSlotBehavior>()._shield;
                }
                else
                    this.transform.localPosition = Globals.Instance.ResetPosition;
            }
            SetCount(_count);
        }
        if (compare)
        {
            if (isPOW && oppositeValue.GetCount() > _count)
            {
                _text.color = Color.red;
                //Debug.Log("lower");
            }
            else if (!isPOW && oppositeValue.GetCount() >= _count)
            {
                _text.color = Color.red;
                //Debug.Log("lower");
            }
            else
                _text.color = Color.cyan;
        }
        else
            _text.color = Color.cyan;
    }

    public int GetCount()
    {
        return _count;
    }

    public void SetCount(int value)
    {
        int ot = 0;
        int baseValue = value;
        if (value >= 100000000)
        {
            ot = (int)(value / 100000000) * 100000000;
            baseValue -= ot;
        }
        if (value >= 1000000000)
        {
            _text.fontSize = 55;
            _text.text = "Sentinel";
        }
        else if (ot == 0)
        {
            _text.fontSize = 55;
            _text.text = baseValue.ToString();
        }
        else if (ot >= 100000000 && baseValue == 0)
        {
            _text.fontSize = 30;
            _text.text = ot.ToString();
        }
        else
        {
            _text.fontSize = 30;
            _text.text = ot.ToString() + "\n" + baseValue.ToString();
        }
        _count = value;
    }

    public void Reset()
    {
        compare = false;
        auto = false;
        this.transform.localPosition = Globals.Instance.ResetPosition;
        currentIndex = -1;
    }
}
