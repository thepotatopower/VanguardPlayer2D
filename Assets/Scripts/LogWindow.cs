using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogWindow : MonoBehaviour
{
    public LogItem logItemPrefab;
    public List<LogItem> LogItems = new List<LogItem>();
    public GameObject Content;
    public GameObject Viewport;
    public Scrollbar scrollbar;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddLogItem(bool playerAction, string description)
    {
        LogItem newLogItem = GameObject.Instantiate(logItemPrefab);
        newLogItem.LeftCard.gameObject.SetActive(false);
        newLogItem.LeftCardLocation.gameObject.SetActive(false);
        newLogItem.Action.gameObject.SetActive(false);
        newLogItem.Description.gameObject.SetActive(true);
        Debug.Log("description: " + description);
        newLogItem.Description.text = description;
        if (playerAction)
            newLogItem.Background.color = Color.cyan;
        LogItems.Add(newLogItem);
        newLogItem.transform.SetParent(Content.transform);
        scrollbar.value = 0;
    }

    public void AddLogItem(bool playerAction, string leftCardID, string leftCardLocation, bool leftCardVisible, string action, string rightCardID, string rightCardLocation)
    {
        LogItem newLogItem = GameObject.Instantiate(logItemPrefab);
        newLogItem.AddLeftCard(leftCardID, leftCardLocation, leftCardVisible);
        newLogItem.LeftCardLocation.text = leftCardLocation;
        newLogItem.Action.text = action;
        if (playerAction)
            newLogItem.Background.color = Color.cyan;
        LogItems.Add(newLogItem); 
        newLogItem.transform.SetParent(Content.transform);
        scrollbar.value = 0;
    }
}
