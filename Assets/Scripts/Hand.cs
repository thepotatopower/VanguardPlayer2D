using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Hand : MonoBehaviour
{
    public GameObject hand;
    public bool inAnimation = false;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void MarkAsSelectable(int tempID)
    {
        for (int i = 0; i < hand.transform.childCount; i++)
        {
            if (tempID.ToString() == hand.transform.GetChild(i).name)
                hand.transform.GetChild(i).GetComponent<CardBehavior>().MarkAsSelectable();
        }
    }

    public void Reset()
    {
        Debug.Log("resetting hand");
        for (int i = 0; i < hand.transform.childCount; i++)
        {
            hand.transform.GetChild(i).GetComponent<CardBehavior>().Reset();
        }
    }

    public IEnumerator Shuffle()
    {
        inAnimation = true;
        List<GameObject> originalOrder = new List<GameObject>();
        for (int i = 0; i < this.transform.childCount; i++)
            originalOrder.Add(this.transform.GetChild(i).gameObject);
        List<int> newOrder = new List<int>();
        System.Random random = new System.Random();
        int number;
        while (newOrder.Count < originalOrder.Count)
        {
            number = random.Next(originalOrder.Count);
            while (newOrder.Contains(number))
            {
                number = random.Next(originalOrder.Count);
                Debug.Log(number);
                if (!newOrder.Contains(number))
                    break;
                yield return null;
            }
            newOrder.Add(number);
            yield return null;
        }
        for (int i = 0; i < originalOrder.Count; i++)
        {
            originalOrder[i].transform.SetSiblingIndex(newOrder[i]);
        }
        inAnimation = false;
    }

    public bool IsInHand(int tempID)
    {
        for (int i = 0; i < hand.transform.childCount; i++)
        {
            if (Int32.Parse(hand.transform.GetChild(i).name) == tempID)
                return true;
        }
        return false;
    }
}
