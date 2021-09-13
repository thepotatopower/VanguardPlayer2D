using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckBuilderCard : MonoBehaviour, IPointerClickHandler
{
    public string cardID = "";
    public DeckBuilderManager deckBuilderManager;

    // Start is called before the first frame update
    void Start()
    {
        deckBuilderManager = GameObject.Find("DeckBuilderManager").GetComponent<DeckBuilderManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter()
    {
        deckBuilderManager.OnPointerEnterCard(cardID);
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Right)
            deckBuilderManager.OnPointerClick(this, true);
        else if (pointerEventData.button == PointerEventData.InputButton.Left)
            deckBuilderManager.OnPointerClick(this, false);
    }
}
