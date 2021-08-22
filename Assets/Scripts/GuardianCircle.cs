using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GuardianCircle : MonoBehaviour
{
    public GameObject guardianCircle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddCard(GameObject card, int tempID)
    {
        card.transform.SetParent(this.transform);
        card.transform.Rotate(new Vector3(0, 0, -90));
        card.name = tempID.ToString();
    }

    public GameObject RemoveCard(int tempID)
    {
        GameObject returnedCard;
        for (int i = 0; i < this.transform.childCount; i++)
        {
            if (tempID == Int32.Parse(this.transform.GetChild(i).name))
            {
                returnedCard = this.transform.GetChild(i).gameObject;
                returnedCard.transform.SetParent(GameObject.Find("Field").transform);
                returnedCard.transform.localPosition = this.transform.localPosition;
                Debug.Log("gc found card");
                return returnedCard;
            }
        }
        Debug.Log("gc could not find card: " + tempID);
        return null;
    }
}
