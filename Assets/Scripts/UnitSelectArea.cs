using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectArea : MonoBehaviour
{
    Color noPointer = Color.white;
    bool isSelectable = false;

    public void OnPointerEnter()
    {
        if (this.transform.parent.TryGetComponent(out UnitSlotBehavior unitSlot))
        {
            unitSlot.GetComponent<Image>().enabled = true;
            if (unitSlot.unit != null && unitSlot._faceup)
                GameObject.Find("CardFightManager").GetComponent<CardFightManager>().DisplayCard(unitSlot._cardID);
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
            inputManager.GetComponent<VisualInputManager>().UnitClicked(unitSlot._FL, unitSlot.unit, isSelectable);
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

    public void Reset()
    {
        if (this.transform.parent.TryGetComponent(out UnitSlotBehavior unitSlot))
        {
            noPointer = Color.white;
            unitSlot.GetComponent<Image>().enabled = false;
            isSelectable = false;
        }
    }
}
