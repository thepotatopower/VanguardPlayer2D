using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VanguardEngine;
using Mirror;

public class Deck : NetworkBehaviour
{
    public PlayerManager playerManager;
    public Text deckCount;
    public List<Card> deck;
    int _count = 0;

    // Update is called once per frame
    void Update()
    {
        //if (deck != null)
        //{
        //    deckCount.text = deck.Count.ToString();
        //}
    }

    public void UpdateCount(int count)
    {
        _count = count;
        deckCount.text = _count.ToString();
    }

    public void CountChange(int count)
    {
        _count += count;
        deckCount.text = _count.ToString();
    }
}
