using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using VanguardEngine;

public class PlayerManager : NetworkBehaviour
{
    VanguardEngine.CardFight cardFight;
    public GameObject Field;
    public GameObject PlayerHand;
    public GameObject cardPrefab;
    public GameObject DeckZone;
    public GameObject HandCard;
    public CardBehavior cardBehavior;
    public GameObject PlayerDeckZone;
    public GameObject EnemyDeckZone;
    public List<Card> player1_deck;
    public List<Card> player2_deck;

    public override void OnStartClient()
    {
        base.OnStartClient();
        PlayerDeckZone = GameObject.Find("PlayerDeckZone");
        EnemyDeckZone = GameObject.Find("EnemyDeckZone");
        LoadCards loadCards = new LoadCards();
        if (hasAuthority)
            player1_deck = loadCards.GenerateCards(Application.dataPath + "/../testDeck.txt", "Data Source=" + Application.dataPath + "/../cards.db;Version=3;");
        else
        {
            if (player1_deck == null)
                Debug.Log("player 1 deck error");
            else
                Debug.L
        }
    }

    [Server]
    public override void OnStartServer()
    {
        InputManager inputManager = new InputManager();
        cardFight = new VanguardEngine.CardFight();
        cardFight.Initialize(Application.dataPath + "/../testDeck.txt", Application.dataPath + "/../testDeck.txt", inputManager, "Data Source=" + Application.dataPath + "/../cards.db;Version=3;");
        player1_deck = cardFight._player1._field.PlayerDeck;
        player2_deck = cardFight._player2._field.PlayerDeck;
        PlayerDeckZone.GetComponent<Deck>().deck = playerDeck;
        Draw();
    }

    [Command]
    public void CmdInitialize()
    {

    }

    [Command]
    public void CmdDraw()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
