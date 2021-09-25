using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSelect : MonoBehaviour
{
    public GameObject content;
    public GameObject CardSelectItemPrefab;
    public GameObject CardSelectPrompt;
    public Scrollbar scrollbar;
    public Button SelectButton;
    public Button CancelButton;
    public List<int> selected;
    public List<CardSelectItemBehavior> selectedItems;
    public int maxSelect = 1;
    public int minSelect = 1;
    GameObject CardSelectItem;
    List<GameObject> CardSelectItems;


    void Start()
    {
        selected = new List<int>();
        CardSelectItems = new List<GameObject>();
        selectedItems = new List<CardSelectItemBehavior>();
    }

    public void Show()
    {
        this.transform.localPosition = new Vector3(0, 0, 0);
    }

    public void Hide()
    {
        this.transform.position = new Vector3(10000, 0, 0);
    }

    public void Initialize(string prompt, int min, int max)
    {
        CardSelectPrompt.GetComponent<Text>().text = prompt;
        minSelect = min;
        maxSelect = max;
        if (minSelect == 0)
            CancelButton.interactable = true;
        else
            CancelButton.interactable = false;
        SelectButton.interactable = false;
    }

    public void AddCardSelectItem(int tempID, string cardID, string cardName, bool faceup, bool upright, bool mandatory, string location)
    {
        CardSelectItem = GameObject.Instantiate(CardSelectItemPrefab);
        //CardSelectItem.name = tempID.ToString();
        CardSelectItem.GetComponent<CardSelectItemBehavior>().tempID = tempID;
        if (!faceup && GameObject.Find(tempID.ToString()) != null && GameObject.Find(tempID.ToString()).transform.parent != GameObject.Find("PlayerHand").transform)
        {
            CardSelectItem.transform.GetChild(0).GetComponent<CardBehavior>().faceup = false;
            CardSelectItem.transform.GetChild(0).GetComponent<Image>().sprite = CardFightManager.LoadSprite(Application.dataPath + "/../cardart/FaceDownCard.jpg");
        }
        else
        {
            CardSelectItem.transform.GetChild(0).GetComponent<CardBehavior>().faceup = true;
            CardSelectItem.transform.GetChild(0).GetComponent<Image>().sprite = CardFightManager.LoadSprite(CardFightManager.FixFileName(cardID));
        }
        if (!upright && Globals.Instance.unitSlots.IsUnit(cardID))
            CardSelectItem.transform.GetComponentInChildren<CardBehavior>().transform.Rotate(0, 0, -90);
        CardSelectItem.transform.GetChild(1).GetComponent<Text>().text = cardName;
        CardSelectItem.transform.GetChild(2).GetComponent<Text>().text = location;
        if (mandatory)
            CardSelectItem.transform.GetChild(5).GetComponent<Text>().enabled = true;
        CardSelectItem.GetComponent<CardSelectItemBehavior>().cardID = cardID;
        CardSelectItems.Add(CardSelectItem);
        CardSelectItem.transform.SetParent(content.transform);
        if (CardSelectItems.Count > 3)
        {
            Debug.Log("resizing field");
            content.GetComponent<RectTransform>().offsetMin = new Vector2(content.GetComponent<RectTransform>().offsetMin.x - 300, content.GetComponent<RectTransform>().offsetMin.y);
            ///content.GetComponent<RectTransform>().offsetMax = new Vector2(content.GetComponent<RectTransform>().offsetMax.x - 150, content.GetComponent<RectTransform>().offsetMax.y);
            scrollbar.value = 0;
        }
    }

    public void ResetItems()
    {
        if (content.transform.childCount > 3)
            content.GetComponent<RectTransform>().offsetMin = new Vector2(content.GetComponent<RectTransform>().offsetMin.x + (300 * (content.transform.childCount - 3)), content.GetComponent<RectTransform>().offsetMin.y);
        for (int i = 0; i < content.transform.childCount; i++)
            GameObject.Destroy(content.transform.GetChild(i).gameObject);
        CardSelectItems.Clear();
        selected.Clear();
        selectedItems.Clear();
        minSelect = 0;
        maxSelect = 0;
        CardSelectItem = null;
        SelectButton.interactable = false;
    }

    public void ItemSelected(int tempID, CardSelectItemBehavior item)
    {
        selected.Add(tempID);
        selectedItems.Add(item);
        if (selected.Count >= minSelect)
            SelectButton.interactable = true;
        if (selected.Count > maxSelect)
        {
            selected.RemoveAt(0);
            selectedItems[0].Deselect();
            selectedItems.RemoveAt(0);
        }
        NumberItems();
    }

    public void ItemDeselected(int tempID)
    {
        selected.Remove(tempID);
        foreach (CardSelectItemBehavior item in selectedItems)
        {
            if (item.tempID == tempID)
            {
                selectedItems.Remove(item);
                break;
            }
        }
        if (selected.Count < minSelect || selected.Count == 0)
            SelectButton.interactable = false;
        NumberItems();
    }

    public bool CapacityMet()
    {
        if (selected.Count == maxSelect)
            return true;
        return false;
    }

    public void NumberItems()
    {
        foreach (CardSelectItemBehavior item in selectedItems)
        {
            item.RemoveNumber();
        }
        for (int i = 0; i < selectedItems.Count; i++)
        {
            selectedItems[i].GiveNumber(i + 1);
        }
    }
}
