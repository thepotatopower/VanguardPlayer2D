using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VanguardEngine;
using Mirror;

public class Pile : NetworkBehaviour
{
    public PlayerManager playerManager;
    public Text pileCount;
    public List<Card> pile = new List<Card>();
    public CardBehavior topCard;
    public bool addToTop = false;
    int _count = 0;

    // Update is called once per frame

    private void Start()
    {
        topCard = GameObject.Instantiate(Globals.Instance.cardPrefab).GetComponent<CardBehavior>();
        topCard.transform.parent = this.transform;
        topCard.transform.localPosition = Vector2.zero;
        if (this.name.Contains("Enemy"))
            topCard.transform.Rotate(0, 0, 180);
        topCard.transform.SetAsFirstSibling();
        if (addToTop)
            topCard.GetComponent<Image>().enabled = false;
        topCard.cardID = "";
    }

    void Update()
    {
        //if (deck != null)
        //{
        //    deckCount.text = deck.Count.ToString();
        //}
    }

    public void InitializeCount(int count) //change count value
    {
        _count = count;
        pileCount.text = _count.ToString();
        if (_count > 0)
        {
            topCard.GetComponent<Image>().enabled = true;
        }
    }

    public void UpdateCount(int count) //increment count by specific value
    {
        _count += count;
        pileCount.text = _count.ToString();
    }

    public void AddCard(Card card)
    {
        pile.Insert(0, card);
        UpdateCount(1);
        topCard.GetComponent<Image>().enabled = true;
        topCard.cardID = card.id;
        if (!addToTop)
            topCard.GetComponent<Image>().sprite = CardFightManager.LoadSprite(Application.dataPath + "/../cardart/FaceDownCard.jpg");
        else
            topCard.GetComponent<Image>().sprite = CardFightManager.LoadSprite(CardFightManager.FixFileName(card.id));
    }

    public void RemoveCard(Card card)
    {
        pile.Remove(card);
        foreach (Card c in pile)
        {
            if (c.tempID == card.tempID)
            {
                pile.Remove(c);
                break;
            }
        }
        UpdateCount(-1);
        if (_count > 0)
        {
            if (addToTop && pile.Count > 0)
            {
                topCard.cardID = pile[0].id;
                topCard.GetComponent<Image>().sprite = CardFightManager.LoadSprite(CardFightManager.FixFileName(topCard.cardID));
            }
            else
            {
                topCard.cardID = "";
                topCard.GetComponent<Image>().sprite = CardFightManager.LoadSprite(Application.dataPath + "/../cardart/FaceDownCard.jpg");
            }
        }
        else
            topCard.GetComponent<Image>().enabled = false;
    }

    public void OnPointerEnter()
    {
        if (addToTop && topCard.GetComponent<Image>().enabled)
        {
            Globals.Instance.cardFightManager.DisplayCard(topCard.cardID);
        }
    }

    public bool ContainsCard(int tempID)
    {
        foreach (Card card in pile)
        {
            if (card.tempID == tempID)
                return true;
        }
        return false;
    }
}
