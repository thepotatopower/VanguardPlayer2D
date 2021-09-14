using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using Dapper;
using System.Linq;
using VanguardEngine;
using System.IO;

public class DeckBuilderManager : MonoBehaviour
{
    public DeckBuilderCard cardPrefab;
    public GameObject CardSearchContent;
    public GameObject MyDeck;
    public GameObject ZoomIn;
    public Text CardName;
    public Text CardEffect;
    public Dropdown NationDropdown;
    public InputField NameInput;
    public Button SaveButton;
    public InputField SaveInput;
    public Button OpenButton;
    public Dropdown OpenDropdown;
    Dictionary<string, Card> cardCatalog = new Dictionary<string, Card>();
    IEnumerator currentCoroutine = null;
    Dictionary<string, int> copies = new Dictionary<string, int>();
    int sentinels = 0;
    int triggers = 0;
    int heals = 0;
    bool overTrigger = false;
    List<Card> mainDeck = new List<Card>();
    Card[] rideDeck = new Card[4];
    List<IEnumerator> actionQueue = new List<IEnumerator>();
    bool performingAction = false;
    string SQLpath;
    string deckDirectory;

    // Start is called before the first frame update
    void Start()
    {
        SQLpath = "Data Source=" + Application.dataPath + "/../cards.db;Version=3;";
        for (int i = 0; i < 4; i++)
        {
            GameObject blankCard = CreateNewCard("");
            blankCard.GetComponent<Image>().enabled = false;
            blankCard.transform.SetParent(MyDeck.transform);
        }
        deckDirectory = "C:/Users/Jason/Desktop/VanguardEngine/VanguardEngine/Properties/";
        string[] filePaths = Directory.GetFiles(deckDirectory);
        foreach (string filePath in filePaths)
        {
            if (LoadCards.GenerateList(filePath, LoadCode.WithRideDeck) != null)
            {
                OpenDropdown.options.Add(new Dropdown.OptionData(filePath.Substring(deckDirectory.Length)));
            }
        }
        if (OpenDropdown.options.Count > 0)
            OpenButton.interactable = true;
        StartCoroutine(PerformActions());
    }

    // Update is called once per frame
    void Update()
    {
        int rideDeckCount = 0;
        foreach (Card card in rideDeck)
        {
            if (card != null)
                rideDeckCount++;
        }
        if (rideDeckCount + mainDeck.Count == 50)
            SaveButton.interactable = true;
        else
            SaveButton.interactable = false;
    }

    IEnumerator PerformActions()
    {
        while (true)
        {
            if (actionQueue.Count > 0)
            {
                while (performingAction)
                    yield return null;
                performingAction = true;
                StartCoroutine(actionQueue[0]);
                actionQueue.RemoveAt(0);
            }
            yield return null;
        }
    }

    public void SearchButtonClicked()
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        string query = "SELECT * FROM data";
        if (NationDropdown.value != 0)
        {
            if (!query.Contains("WHERE"))
                query += " WHERE";
            else
                query += " AND";
            switch (NationDropdown.value)
            {
                case 1:
                    query += " nation=" + Nation.DragonEmpire;
                    break;
                case 2:
                    query += " nation=" + Nation.DarkStates;
                    break;
                case 3:
                    query += " nation=" + Nation.KeterSanctuary;
                    break;
                case 4:
                    query += " nation=" + Nation.Stoicheia;
                    break;
                case 5:
                    query += " nation=" + Nation.BrandtGate;
                    break;
                case 6:
                    query += " nation=" + Nation.LyricalMonasterio;
                    break;
            }
        }
        if (NameInput.text != "")
        {
            if (!query.Contains("WHERE"))
                query += " WHERE";
            else
                query += " AND";
            query += " name LIKE '%" + NameInput.text + "%'";
        }
        Debug.Log(query);
        List<Card> cards = LoadCards.Search(query, SQLpath);
        while (CardSearchContent.transform.childCount > 0)
        {
            GameObject toDestroy = CardSearchContent.transform.GetChild(0).gameObject;
            toDestroy.transform.SetParent(null);
            GameObject.Destroy(toDestroy);
        }
        IEnumerator Dialog()
        {
            foreach (Card card in cards)
            {
                cardCatalog[card.id] = card;
                GameObject newCard = CreateNewCard(card.id);
                newCard.transform.SetParent(CardSearchContent.transform);
                int rows = CardSearchContent.transform.childCount / 10;
                if (CardSearchContent.transform.childCount % 10 > 0)
                    rows++;
                //CardSearchContent.GetComponent<RectTransform>().offsetMax = new Vector2(CardSearchContent.GetComponent<RectTransform>().offsetMax.x, rows * 106);
                CardSearchContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, rows * 106);
                yield return null;
            }
            yield return null;
        }
        currentCoroutine = Dialog();
        StartCoroutine(currentCoroutine);
    }

    public void OnPointerEnterCard(string cardID)
    {
        if (cardID != "")
        {
            ZoomIn.GetComponent<Image>().sprite = CardFightManager.LoadSprite(CardFightManager.FixFileName(cardID));
            if (cardCatalog.ContainsKey(cardID))
            {
                CardName.text = cardCatalog[cardID].name;
                CardEffect.text = cardCatalog[cardID].effect;
            }
        }
    }

    public void OnPointerClick(DeckBuilderCard clickedObject, bool right)
    {
        IEnumerator Dialog()
        {
            string cardID = clickedObject.cardID;
            Card clickedCard = LookUpCard(cardID);
            if ((right && clickedObject.transform.parent == CardSearchContent.transform) || (!right && clickedObject.transform.parent == MyDeck.transform))
            {
                Debug.Log("adding to main");
                AddCard(clickedCard, true);
            }
            else if (!right && clickedObject.transform.parent == CardSearchContent.transform)
            {
                Debug.Log("adding to ride");
                AddCard(clickedCard, false);
            }
            else if (right && clickedObject.transform.parent == MyDeck.transform)
            {
                int index = 0;
                if (mainDeck.Contains(LookUpCard(cardID)))
                {
                    index = mainDeck.IndexOf(LookUpCard(cardID));
                    mainDeck.RemoveAt(index);
                    GameObject toBeRemoved = MyDeck.transform.GetChild(index + 4).gameObject;
                    toBeRemoved.transform.SetParent(null);
                    GameObject.Destroy(toBeRemoved);
                }
                else if (rideDeck.Contains(LookUpCard(cardID)))
                {
                    index = 0;
                    for (; index < rideDeck.Length; index++)
                    {
                        if (rideDeck[index] == clickedCard)
                            break;
                    }
                    rideDeck[index] = null;
                    GameObject toBeRemoved = MyDeck.transform.GetChild(index).gameObject;
                    toBeRemoved.transform.SetParent(null);
                    GameObject.Destroy(toBeRemoved);
                    GameObject blankCard = CreateNewCard("");
                    blankCard.GetComponent<Image>().enabled = false;
                    blankCard.transform.SetParent(MyDeck.transform);
                    blankCard.transform.SetSiblingIndex(index);
                }
                if (copies.ContainsKey(clickedCard.id) && copies[clickedCard.id] > 0)
                    copies[clickedCard.id]--;
                if (clickedCard.unitType == UnitType.Sentinel)
                    sentinels--;
                if (clickedCard.unitType == UnitType.Trigger)
                    triggers--;
                if (clickedCard.trigger == Trigger.Heal)
                    heals--;
                if (clickedCard.trigger == Trigger.Over)
                    overTrigger = false;
            }
            performingAction = false;
            yield break;
        }
        actionQueue.Add(Dialog());
    }

    public void AddCard(Card clickedCard, bool main)
    {
        string cardID = clickedCard.id;
        int rideDeckCount = 0;
        foreach (Card card in rideDeck)
        {
            if (card != null)
                rideDeckCount++;
        }
        //if (main && mainDeck.Count >= 46)
        //    return;
        //if (!main && clickedCard.grade > 3)
        //    return;
        //if (!main && clickedCard.unitType == UnitType.NotUnit)
        //    return;
        //if (copies.ContainsKey(cardID) && copies[cardID] >= 4)
        //    return;
        //if (clickedCard.unitType != UnitType.Trigger && mainDeck.Count + rideDeckCount - triggers >= 34)
        //    return;
        //if (clickedCard.unitType == UnitType.Sentinel && sentinels >= 4)
        //    return;
        //if (clickedCard.unitType == UnitType.Trigger && triggers >= 16)
        //    return;
        //if (clickedCard.trigger == Trigger.Heal && heals >= 4)
        //    return;
        //if (clickedCard.trigger == Trigger.Over && overTrigger)
        //    return;
        //foreach (Card card in rideDeck)
        //{
        //    if (card != null && (card.nation != -1 && clickedCard.nation != card.nation && clickedCard.nation != -1))
        //        return;
        //}
        //foreach (Card card in mainDeck)
        //{
        //    if (card != null && (card.nation != -1 && clickedCard.nation != card.nation && clickedCard.nation != -1))
        //            return;
        //}
        if (main)
            InsertInMainDeck(clickedCard);
        else
            InsertInRideDeck(clickedCard);
        if (!copies.ContainsKey(cardID))
            copies[cardID] = 0;
        copies[cardID]++;
        if (clickedCard.unitType == UnitType.Sentinel)
            sentinels++;
        if (clickedCard.unitType == UnitType.Trigger)
            triggers++;
        if (clickedCard.trigger == Trigger.Heal)
            heals++;
        if (clickedCard.trigger == Trigger.Over)
            overTrigger = true;
        GameObject newCardObject = CreateNewCard(cardID);
        if (main)
        {
            newCardObject.transform.SetParent(MyDeck.transform);
            newCardObject.transform.SetSiblingIndex(mainDeck.IndexOf(clickedCard) + 4);
        }
        else
        {
            int index = 0;
            for (; index < 4; index++)
            {
                if (rideDeck[index] == clickedCard)
                    break;
            }
            GameObject toBeRemoved = MyDeck.transform.GetChild(index).gameObject;
            toBeRemoved.transform.SetParent(null);
            GameObject.Destroy(toBeRemoved);
            newCardObject.transform.SetParent(MyDeck.transform);
            newCardObject.transform.SetSiblingIndex(index);
        }
    }

    public void InsertInRideDeck(Card newCard)
    {
        if (newCard.grade < 4)
            rideDeck[newCard.grade] = newCard;
    }

    public void InsertInMainDeck(Card newCard)
    {
        if (mainDeck.Count == 0)
        {
            mainDeck.Add(newCard);
            return;
        }
        if (newCard.unitType == UnitType.Trigger)
        {
            int t;
            for (t = 0; t < mainDeck.Count; t++)
            {
                if (mainDeck[t].unitType == UnitType.Trigger)
                    break;
            }
            if (t > mainDeck.Count)
            {
                mainDeck.Add(newCard);
                return;
            }
            for (; t < mainDeck.Count; t++)
            {
                if (newCard.trigger < mainDeck[t].trigger)
                {
                    mainDeck.Insert(t, newCard);
                    return;
                }
                if (newCard.trigger == mainDeck[t].trigger)
                    break;
            }
            for (; t < mainDeck.Count; t++)
            {
                if (newCard.trigger < mainDeck[t].trigger)
                {
                    mainDeck.Insert(t, newCard);
                    return;
                }
                if (string.Compare(newCard.name, mainDeck[t].name) == -1)
                {
                    mainDeck.Insert(t, newCard);
                    return;
                }
            }
            mainDeck.Add(newCard);
            return;
        }
        int triggerIndex = 0;
        for (int i = 0; i < mainDeck.Count; i++)
        {
            if (mainDeck[i].trigger != -1)
            {
                triggerIndex = i;
                break;
            }
        }
        for (int i = 0; i < triggerIndex; i++)
        {
            if (newCard.grade < mainDeck[i].grade)
            {
                mainDeck.Insert(i, newCard);
                return;
            }
            if (newCard.grade == mainDeck[i].grade)
            {
                for (int j = i; j < triggerIndex; j++)
                {
                    if (newCard.grade < mainDeck[j].grade)
                    {
                        mainDeck.Insert(j, newCard);
                        return;
                    }
                    if (string.Compare(newCard.name, mainDeck[j].name) == -1)
                    {
                        mainDeck.Insert(j, newCard);
                        return;
                    }
                }
                break;
            }
        }
        mainDeck.Insert(triggerIndex, newCard);
    }

    public Card LookUpCard(string cardID)
    {
        //Debug.Log("looking up " + cardID + "...");
        if (cardCatalog.ContainsKey(cardID))
            return cardCatalog[cardID];
        List<string> cardIDs = new List<string>();
        cardIDs.Add(cardID);
        List<Card> cards = LoadCards.GenerateCardsFromList(cardIDs, SQLpath);
        cardCatalog[cardID] = cards[0];
        return cardCatalog[cardID];
    }

    public GameObject CreateNewCard(string cardID)
    {
        GameObject newCard = GameObject.Instantiate(cardPrefab.gameObject);
        newCard.GetComponent<DeckBuilderCard>().cardID = cardID;
        newCard.GetComponent<Image>().sprite = CardFightManager.LoadSprite(CardFightManager.FixFileName(cardID));
        return newCard;
    }

    public void SaveButtonClicked()
    {
        string filename;
        if (SaveInput.text != "")
            filename = SaveInput.text;
        else
            filename = "testDeck";
        StreamWriter file = new StreamWriter("C:/Users/Jason/Desktop/VanguardEngine/VanguardEngine/Properties/" + filename + ".txt");
        file.Write("#RideDeck");
        foreach (Card card in rideDeck)
        {
            file.Write("\n" + card.id);
        }
        file.Write("\n#MainDeck");
        foreach (Card card in mainDeck)
        {
            file.Write("\n" + card.id);
        }
        file.Close();
    }

    public void OpenButtonClicked()
    {
        IEnumerator Dialog()
        {
            ClearDeck();
            List<Card> cards = LoadCards.GenerateCardsFromList(LoadCards.GenerateList(deckDirectory + OpenDropdown.options[OpenDropdown.value].text, LoadCode.WithRideDeck), SQLpath);
            for (int i = 0; i < 4; i++)
            {
                AddCard(LookUpCard(cards[i].id), false);
                yield return null;
            }
            for (int i = 4; i < cards.Count; i++)
            {
                AddCard(LookUpCard(cards[i].id), true);
                yield return null;
            }
            performingAction = false;
        }
        actionQueue.Add(Dialog());
    }

    public void ClearDeck()
    {
        for (int i = 0; i < MyDeck.transform.childCount; i++)
        {
            GameObject.Destroy(MyDeck.transform.GetChild(i).gameObject);
        }
        MyDeck.transform.DetachChildren();
        mainDeck.Clear();
        for (int i = 0; i < 4; i++)
        {
            rideDeck[i] = null;
            GameObject blankCard = CreateNewCard("");
            blankCard.GetComponent<Image>().enabled = false;
            blankCard.transform.SetParent(MyDeck.transform);
        }
        triggers = 0;
        heals = 0;
        sentinels = 0;
        overTrigger = false;
        copies.Clear();
    }

    public void ClearButtonClicked()
    {
        IEnumerator Dialog()
        {
            ClearDeck();
            performingAction = false;
            yield break;
        }
        actionQueue.Add(Dialog());
    }
}
