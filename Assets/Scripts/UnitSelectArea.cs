using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectArea : MonoBehaviour
{

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
            unitSlot.GetComponent<Image>().enabled = false;
    }
}
