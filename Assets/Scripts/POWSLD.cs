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

    // Start is called before the first frame update

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
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
        if (ot == 0)
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
}
