using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VanguardEngine;

public class Deck : MonoBehaviour
{
    public Text deckCount;
    public List<Card> deck;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (deck != null)
        {
            deckCount.text = deck.Count.ToString();
        }
    }
}
