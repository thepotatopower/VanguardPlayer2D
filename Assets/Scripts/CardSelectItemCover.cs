using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSelectItemCover : MonoBehaviour
{
    public void OnPointerEnter()
    {
        this.transform.parent.GetComponent<CardSelectItemBehavior>().OnPointerEnter();
    }

    public void OnPointerExit()
    {
        this.transform.parent.GetComponent<CardSelectItemBehavior>().OnPointerExit();
    }

    public void OnClick()
    {
        this.transform.parent.GetComponent<CardSelectItemBehavior>().OnClick();
    }
}
