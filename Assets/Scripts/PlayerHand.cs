using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    public GameObject playerHand;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void MarkAsSelectable(int tempID)
    {
        for (int i = 0; i < playerHand.transform.childCount; i++)
        {
            if (tempID.ToString() == playerHand.transform.GetChild(i).name)
                playerHand.transform.GetChild(i).GetComponent<CardBehavior>().MarkAsSelectable();
        }
    }

    public void Reset()
    {
        for (int i = 0; i < playerHand.transform.childCount; i++)
        {
            playerHand.transform.GetChild(i).GetComponent<CardBehavior>().Reset();
        }
    }
}
