using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSelectItemBehavior : MonoBehaviour
{
    VisualInputManager inputManager;
    CardFightManager cardFightManager;
    public CardSelect cardSelect;
    public string cardID;
    public int tempID;
    public bool selected = false;
    public Color32 defaultColor;
    public GameObject NumberCircle;
    public Text Number;
    public string description = "";
    
    void Start()
    {
        inputManager = GameObject.Find("InputManager").GetComponent<VisualInputManager>();
        cardFightManager = GameObject.Find("CardFightManager").GetComponent<CardFightManager>();
        //cardSelect = GameObject.Find("CardSelect").GetComponent<CardSelect>();
        defaultColor = this.GetComponent<Image>().color;
    }

    public void OnPointerEnter()
    {
        cardFightManager.DisplayCard(cardID, tempID);
        if (!selected)
            this.GetComponent<Image>().color = new Color32(195, 243, 250, 255);
        cardSelect.description = description;
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
            cardSelect.ItemDeselected(tempID);
            Deselect();
        }
        else
        {
            //if (!cardSelect.CapacityMet())
            //{
            //    this.GetComponent<Image>().color = new Color32(92, 233, 255, 255);
            //    selected = true;
            //    cardSelect.ItemSelected(tempID, this);
            //    Globals.Instance.unitSlots.MarkAsSelectable(tempID);
            //}
            this.GetComponent<Image>().color = new Color32(92, 233, 255, 255);
            selected = true;
            cardSelect.ItemSelected(tempID, this);
            Globals.Instance.unitSlots.MarkAsSelectable(tempID);
        }
    }

    public void Deselect()
    {
        this.GetComponent<Image>().color = new Color32(195, 243, 250, 0);
        selected = false;
        Globals.Instance.unitSlots.Reset(tempID);
        NumberCircle.SetActive(false);
    }

    public void GiveNumber(int number)
    {
        NumberCircle.SetActive(true);
        Number.text = number.ToString();
    }

    public void RemoveNumber()
    {
        if (NumberCircle.activeSelf)
            NumberCircle.SetActive(false);
    }
}
