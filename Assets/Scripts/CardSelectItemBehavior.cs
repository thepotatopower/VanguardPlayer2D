using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSelectItemBehavior : MonoBehaviour
{
    VisualInputManager inputManager;
    CardFightManager cardFightManager;
    CardSelect cardSelect;
    public string cardID;
    public int tempID;
    public bool selected = false;
    public Color32 defaultColor;
    
    void Start()
    {
        inputManager = GameObject.Find("InputManager").GetComponent<VisualInputManager>();
        cardFightManager = GameObject.Find("CardFightManager").GetComponent<CardFightManager>();
        cardSelect = GameObject.Find("CardSelect").GetComponent<CardSelect>();
        defaultColor = this.GetComponent<Image>().color;
    }

    public void OnPointerEnter()
    {
        cardFightManager.DisplayCard(cardID);
        if (!selected)
            this.GetComponent<Image>().color = new Color32(195, 243, 250, 255);
    }

    public void OnPointerExit()
    {
        if (!selected)
            this.GetComponent<Image>().color = defaultColor;
    }

    public void OnClick()
    {
        if (selected)
        {
            this.GetComponent<Image>().color = new Color32(195, 243, 250, 0);
            selected = false;
            cardSelect.ItemDeselected(tempID);
            Globals.Instance.unitSlots.Reset(tempID);
        }
        else
        {
            if (!cardSelect.CapacityMet())
            {
                this.GetComponent<Image>().color = new Color32(92, 233, 255, 255);
                selected = true;
                cardSelect.ItemSelected(tempID);
                Globals.Instance.unitSlots.MarkAsSelectable(tempID);
            }
        }
    }
}
