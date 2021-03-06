using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectArea : MonoBehaviour
{
    Color noPointer = Color.white;
    bool isSelectable = false;
    public bool inAnimation = false;

    public void OnPointerEnter()
    {
        if (this.transform.parent.TryGetComponent(out UnitSlotBehavior unitSlot))
        {
            unitSlot.GetComponent<Image>().enabled = true;
            unitSlot.GetComponent<Image>().color = Color.green;
            if (unitSlot.unit != null && unitSlot._faceup)
                GameObject.Find("CardFightManager").GetComponent<CardFightManager>().DisplayCard(unitSlot._cardID, int.Parse(unitSlot.unit.name));
            if (Globals.Instance.cardFightManager != null && Globals.Instance.cardFightManager._attacked.Contains(unitSlot._FL))
                Globals.Instance.SLD.currentIndex = Globals.Instance.cardFightManager._attacked.IndexOf(unitSlot._FL);
        }
    }

    public void OnPointerExit()
    {
        if (this.transform.parent.TryGetComponent(out UnitSlotBehavior unitSlot))
        {
            if (noPointer != Color.white)
                unitSlot.GetComponent<Image>().color = noPointer;
            else
                unitSlot.GetComponent<Image>().enabled = false;
        }
    }

    public void OnPointerClicked()
    {
        GameObject inputManager = GameObject.Find("InputManager");
        if (inputManager == null)
        {
            Debug.Log("no input manager");
            return;
        }
        if (this.transform.parent.TryGetComponent(out UnitSlotBehavior unitSlot))
        {
            Debug.Log("unit clicked");
            inputManager.GetComponent<VisualInputManager>().OnUnitClicked(unitSlot._FL, unitSlot.unit, isSelectable);
        }
    }

    public void MarkAsSelectable()
    {
        if (this.transform.parent.TryGetComponent(out UnitSlotBehavior unitSlot))
        {
            noPointer = Color.cyan;
            unitSlot.GetComponent<Image>().enabled = true;
            unitSlot.GetComponent<Image>().color = Color.cyan;
            isSelectable = true;
        }
    }

    public void MarkWithColor(Color color)
    {
        if (this.transform.parent.TryGetComponent(out UnitSlotBehavior unitSlot))
        {
            noPointer = color;
            unitSlot.GetComponent<Image>().enabled = true;
            unitSlot.GetComponent<Image>().color = color;
        }
    }

    public void Reset()
    {
        if (this.transform.parent.TryGetComponent(out UnitSlotBehavior unitSlot))
        {
            noPointer = Color.white;
            unitSlot.GetComponent<Image>().enabled = false;
            isSelectable = false;
        }
    }

    public IEnumerator Flash(Color color)
    {
        if (this.transform.parent.TryGetComponent(out UnitSlotBehavior unitSlot))
        {
            for (int i = 0; i < 6; i++)
            {
                unitSlot.GetComponent<Image>().enabled = true;
                unitSlot.GetComponent<Image>().color = color;
                yield return new WaitForSecondsRealtime((float)0.10);
                unitSlot.GetComponent<Image>().enabled = false;
                yield return new WaitForSecondsRealtime((float)0.10);
                //Debug.Log("flashing");
            }
            OnPointerExit();
        }
        inAnimation = false;
    }
}
