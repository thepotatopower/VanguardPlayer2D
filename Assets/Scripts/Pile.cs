using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VanguardEngine;
using Mirror;

public class Pile : NetworkBehaviour
{
    public PlayerManager playerManager;
    public Text pileCount;
    public List<Card> pile = new List<Card>();
    public List<Sprite> sprites = new List<Sprite>();
    bool addToTop = false;
    int _count = 0;

    // Update is called once per frame
    void Update()
    {
        //if (deck != null)
        //{
        //    deckCount.text = deck.Count.ToString();
        //}
    }

    public void UpdateCount(int count) //change count value
    {
        _count = count;
        pileCount.text = _count.ToString();
    }

    public void CountChange(int count) //increment count by specific value
    {
        _count += count;
        pileCount.text = _count.ToString();
    }

    public void AddCard(Card card, Sprite sprite)
    {
        pile.Add(card);
        sprites.Add(sprite);
        if (pile.Count > 0)
        {
            if (addToTop)
                ChangeSprite(sprites[sprites.Count - 1]);
            else
                ChangeSprite(LoadSprite(Application.dataPath + "/../cardart/FaceDownCard.jpg"));
        }
        CountChange(1);
    }

    public void RemoveCard(Card card)
    {
        int index = pile.IndexOf(card);
        if (index >= 0)
        {
            pile.RemoveAt(index);
            sprites.RemoveAt(index);
        }
        CountChange(-1);
        if (_count == 0)
            ChangeSprite(Resources.Load<Sprite>("invisible-png-4-png-image-invisible-png-300_240"));
    }

    public void SetAddToTop()
    {
        addToTop = true;
    }

    public bool AddToTop()
    {
        return addToTop;
    }

    public void ChangeSprite(Sprite sprite)
    {
        this.GetComponent<Image>().sprite = sprite;
    }

    public static Sprite LoadSprite(string filename)
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
}
