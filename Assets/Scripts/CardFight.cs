using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VanguardEngine;
using UnityEngine.UI;

public class CardFight : MonoBehaviour
{
    // Start is called before the first frame update
    VanguardEngine.CardFight cardFight;
    public GameObject Field;
    public GameObject PlayerHand;
    public GameObject cardPrefab;
    public GameObject DeckZone;
    public GameObject HandCard;
    public CardBehavior cardBehavior;
    public GameObject PlayerDeckZone;
    public GameObject EnemyDeckZone;
    public List<Card> playerDeck;
    public List<Card> enemyDeck;

    void Start()
    {
        InputManager inputManager = new InputManager();
        cardFight = new VanguardEngine.CardFight();
        //cardFight.Initialize(Application.dataPath + "/../testDeck.txt", Application.dataPath + "/../testDeck.txt", inputManager, "Data Source=" + Application.dataPath + "/../cards.db;Version=3;");
        playerDeck = cardFight._player1._field.PlayerDeck;
        enemyDeck = cardFight._player1._field.EnemyDeck;
        PlayerDeckZone.GetComponent<Deck>().deck = playerDeck;
        Draw();
    }

    public void Draw()
    {
        Card drawnCard = playerDeck[0];
        playerDeck.RemoveAt(0);
        GameObject newCard = GameObject.Instantiate(cardPrefab);
        newCard.transform.SetParent(PlayerHand.transform);
        newCard.GetComponent<Image>().sprite = LoadSprite(FixFileName(drawnCard.id));
    }

    public Sprite LoadSprite(string filename)
    {
        if (System.IO.File.Exists(filename))
        {
            byte[] bytes = System.IO.File.ReadAllBytes(filename);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }
        else
        {
            Debug.Log(filename + "doesn't exist.");
            return null;
        }
    }
    void Update()
    {
        
    }

    public string FixFileName(string input)
    {
        int first = input.IndexOf('-');
        string firstHalf = input.Substring(0, first);
        string secondHalf = input.Substring(first + 1, input.Length - (first + 1));
        int second = secondHalf.IndexOf('/');
        return ("../art/" + firstHalf + secondHalf.Substring(0, second) + "_" + secondHalf.Substring(second + 1, secondHalf.Length - (second + 1)) + ".png").ToLower();
    }
}
