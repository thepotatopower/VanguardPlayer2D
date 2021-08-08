using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VanguardEngine;

public class UnitSlotBehavior : MonoBehaviour
{
    public int _grade;
    public Text Grade;
    public int _soul;
    public Text Soul;
    public int _critical;
    public Text Critical;
    public int _power;
    public Text Power;
    public bool _upright = true;
    public bool _faceup = true;
    public string _cardID;
    public GameObject unit = null;
    public int slot;
    public bool inAnimation = false;
    public int _FL;

    public void Initialize(int FL)
    {
        _FL = FL;
    }

    public void AddCard(int grade, int soul, int critical, int power, bool upright, bool faceup, string cardID, GameObject card)
    {
        _grade = grade;
        _soul = soul;
        _critical = critical;
        _power = power;
        _upright = upright;
        _faceup = faceup;
        _cardID = cardID;
        GameObject.Destroy(unit);
        unit = card;
        unit.transform.SetParent(this.transform);
        unit.transform.SetAsFirstSibling();
        unit.transform.localPosition = new Vector3(0, 0, 0);
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
            unit.GetComponent<Image>().sprite = CardFightManager.LoadSprite("../art/FaceDownCard.jpg");
            Grade.enabled = false;
            Soul.enabled = false;
            Power.enabled = false;
            Critical.enabled = false;
        }
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

    public void RemoveCard(string cardID)
    {
        if (cardID == _cardID)
        {
            GameObject.Destroy(unit);
            unit = null;
            Grade.text = "";
            Critical.text = "";
            Soul.text = "";
            Power.text = "";
        }
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
            unit.GetComponent<Image>().sprite = CardFightManager.LoadSprite("../art/FaceDownCard.jpg");
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
            Ninety = Quaternion.Euler(unit.transform.localRotation.eulerAngles.x, unit.transform.localRotation.eulerAngles.y, unit.transform.localRotation.eulerAngles.z + 90);
        }
        else if (!_upright && upright)
        {
            Ninety = Quaternion.Euler(unit.transform.localRotation.eulerAngles.x, unit.transform.localRotation.eulerAngles.y, unit.transform.localRotation.eulerAngles.z - 90);
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
}
