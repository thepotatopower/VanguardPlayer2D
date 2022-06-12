using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogItem : MonoBehaviour
{
    public Image Background;
    public CardBehavior LeftCard;
    public Text LeftCardLocation;
    public Text Action;
    public Text Description;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddLeftCard(string leftCardID, string leftCardLocation, bool leftCardVisible)
    {
        if (!leftCardVisible)
        {
            LeftCard.faceup = false;
            LeftCard.GetComponent<Image>().sprite = CardFightManager.LoadSprite(Application.dataPath + "/../cardart/FaceDownCard.jpg");
        }
        else
        {
            LeftCard.faceup = true;
            LeftCard.GetComponent<Image>().sprite = CardFightManager.LoadSprite(CardFightManager.FixFileName(leftCardID));
            LeftCard.cardID = leftCardID;
        }
    }
}
