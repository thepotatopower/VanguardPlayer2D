using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VanguardEngine;
using Mirror;
using System;

public class Pile : NetworkBehaviour
{
    public PlayerManager playerManager;
    public Text pileCount;
    List<Card> pile = new List<Card>();
    List<bool> _faceup = new List<bool>();
    public CardBehavior topCard;
    public bool selectable = false;
    public bool isPublic = true;
    int _count = 0;

    // Update is called once per frame

    private void Start()
    {
        pileCount.text = "0";
    }

    void Update()
    {
        //if (deck != null)
        //{
        //    deckCount.text = deck.Count.ToString();
        //}
    }

    public void AddCard(Card card, bool faceup)
    {
        AddCard(card, faceup, true);
    }

    public void AddCard(Card card, bool faceup, bool update)
    {
        pile.Insert(0, card);
        _faceup.Insert(0, faceup);
        Debug.Log(card.tempID + " added to " + this.name);
        if (faceup)
            Debug.Log("is face up");
        else
            Debug.Log("is face down");
        if (update)
            UpdateVisuals();
    }

    public void RemoveCard(Card card)
    {
        pile.Remove(card);
        foreach (Card c in pile)
        {
            if (c.tempID == card.tempID)
            {
                _faceup.RemoveAt(pile.IndexOf(c));
                pile.Remove(c);
                break;
            }
        }
        UpdateVisuals();
    }

    public void OnPointerEnter()
    {
        if (isPublic && topCard != null)
        {
            Globals.Instance.cardFightManager.DisplayCard(topCard.card.id, topCard.card.tempID);
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

    public void MarkAsSelectable()
    {
        if (topCard != null)
            topCard.MarkAsSelectable();
        selectable = true;
    }

    public void UnMarkAsSelectable()
    {
        if (topCard != null)
            topCard.Reset();
        selectable = false;
    }

    public List<Card> GetCards()
    {
        return new List<Card>(pile);
    }

    public List<Tuple<Card, bool>> GetCardsWithFaceUp()
    {
        List<Tuple<Card, bool>> cards = new List<Tuple<Card, bool>>();
        for (int i = 0; i < pile.Count; i++)
        {
            if (_faceup.Count > i)
                cards.Add(new Tuple<Card, bool>(pile[i], _faceup[i]));
            else
                cards.Add(new Tuple<Card, bool>(pile[i], true));
        }
        return cards;
    }

    public void Flipped(int tempID, bool facedUp)
    {
        if (pile.Exists(card => card.tempID == tempID))
        {
            _faceup[pile.FindIndex(card => card.tempID == tempID)] = facedUp;
            UpdateVisuals();
        }
    }

    public void UpdateVisuals()
    {
        UpdateVisuals(false);
    }

    public void UpdateVisuals(bool allFaceDown)
    {
        if (topCard != null)
            GameObject.Destroy(topCard.gameObject);
        topCard = null;
        if (pile.Count > 0)
        {
            topCard = Globals.Instance.cardFightManager.CreateNewCard(pile[0].id, pile[0].tempID).GetComponent<CardBehavior>();
            topCard.transform.parent = this.transform;
            topCard.transform.localPosition = Vector2.zero;
            topCard.card = pile[0];
            if (this.name.Contains("Enemy"))
                topCard.transform.Rotate(0, 0, 180);
            topCard.transform.SetAsFirstSibling();
            if (_faceup[0] && !allFaceDown && this.name != "PlayerDeck" && this.name != "EnemyDeck")
                topCard.GetComponent<Image>().sprite = CardFightManager.LoadSprite(CardFightManager.FixFileName(pile[0].id));
            else
                topCard.GetComponent<Image>().sprite = CardFightManager.LoadSprite(Application.dataPath + "/../cardart/FaceDownCard.jpg");
            topCard.name = "TopCard";
        }
        pileCount.text = pile.Count.ToString();
        if (name == "PlayerOrderZone")
            Globals.Instance.playerMiscStats.UpdateSongText();
        if (name == "EnemyOrderZone")
            Globals.Instance.enemyMiscStats.UpdateSongText();
    }
}
