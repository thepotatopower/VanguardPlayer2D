using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSelectItemBehavior : MonoBehaviour
{
    VisualInputManager inputManager;
    CardSelect cardSelect;
    public bool selected = false;
    public Color32 defaultColor;
    
    void Start()
    {
        inputManager = GameObject.Find("InputManager").GetComponent<VisualInputManager>();
        cardSelect = GameObject.Find("CardSelect").GetComponent<CardSelect>();
        defaultColor = this.GetComponent<Image>().color;
    }

    public void OnPointerEnter()
    {
        this.transform.GetChild(0).GetComponent<CardBehavior>().DisplayCard();
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
            cardSelect.ItemDeselected(int.Parse(this.name));
        }
        else
        {
            if (!cardSelect.CapacityMet())
            {
                this.GetComponent<Image>().color = new Color32(92, 233, 255, 255);
                selected = true;
                cardSelect.ItemSelected(int.Parse(this.name));
            }
        }
    }
}
