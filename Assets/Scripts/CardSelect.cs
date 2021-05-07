using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSelect : MonoBehaviour
{
    public GameObject content;
    public GameObject CardSelectItemPrefab;
    public GameObject CardSelectPrompt;
    public Button SelectButton;
    public List<int> selected;
    public int maxSelect = 1;
    public int minSelect = 1;
    GameObject CardSelectItem;
    List<GameObject> CardSelectItems;


    void Start()
    {
        selected = new List<int>();
        CardSelectItems = new List<GameObject>();
    }

    public void Show()
    {
        this.transform.localPosition = new Vector3(0, 0, 0);
    }

    public void Hide()
    {
        this.transform.position = new Vector3(10000, 0, 0);
    }

    public void Initialize(string prompt, int capacity)
    {
        CardSelectPrompt.GetComponent<Text>().text = prompt;
        maxSelect = capacity;
    }

    public void AddCardSelectItem(int tempID, string cardID, string cardName)
    {
        CardSelectItem = GameObject.Instantiate(CardSelectItemPrefab);
        CardSelectItem.name = tempID.ToString();
        CardSelectItem.transform.GetChild(0).GetComponent<Image>().sprite = CardFightManager.LoadSprite(CardFightManager.FixFileName(cardID));
        CardSelectItem.transform.GetChild(1).GetComponent<Text>().text = cardName;
        CardSelectItems.Add(CardSelectItem);
        CardSelectItem.transform.SetParent(content.transform);
        if (CardSelectItems.Count > 3)
        {
            content.GetComponent<RectTransform>().offsetMin = new Vector2(content.GetComponent<RectTransform>().offsetMin.x - 150, content.GetComponent<RectTransform>().offsetMin.y);
            content.GetComponent<RectTransform>().offsetMax = new Vector2(content.GetComponent<RectTransform>().offsetMax.x - 150, content.GetComponent<RectTransform>().offsetMax.y);
        }
    }

    public void ItemSelected(int tempID)
    {
        selected.Add(tempID);
        if (selected.Count == minSelect)
            SelectButton.interactable = true;
    }

    public void ItemDeselected(int tempID)
    {
        selected.Remove(tempID);
        if (selected.Count < minSelect)
            SelectButton.interactable = false;
    }

    public bool CapacityMet()
    {
        if (selected.Count == maxSelect)
            return true;
        return false;
    }
}
