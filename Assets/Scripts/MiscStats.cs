using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VanguardEngine;
using System;

public class MiscStats : MonoBehaviour
{
    public Text miscTextPrefab;
    public Text prisonersText;
    public Text worldText;
    public Text songText;
    public bool hasPrison = false;
    public int prisoners = 0;

    void Update()
    {

    }

    public void SetPrison()
    {
        hasPrison = true;
        prisoners = 0;
        prisonersText = GameObject.Instantiate(miscTextPrefab);
        prisonersText.text = "Prisoners: 0";
        prisonersText.transform.SetParent(this.transform);
    }

    public void Imprison()
    {
        prisoners++;
        prisonersText.text = "Prisoners: " + prisoners;
    }

    public void Free()
    {
        prisoners--;
        prisonersText.text = "Prisoners: " + prisoners;
    }

    public void SetWorld(List<Card> cards)
    {
        int world = 0;
        foreach (Card card in cards)
        {
            if (card.orderType == 5)
                world++;
            else
            {
                world = 0;
                break;
            }
        }
        if (world == 0)
        {
            if (worldText != null)
            {
                worldText.transform.SetParent(null);
                GameObject.Destroy(worldText);
            }
        }
        else
        {
            if (worldText == null)
            {
                worldText = GameObject.Instantiate(miscTextPrefab);
                worldText.transform.SetParent(this.transform);
            }
            if (world == 1)
                worldText.text = "Dark Night";
            else if (world >= 2)
                worldText.text = "Abyssal Dark Night";
        }
    }

    public void UpdateSongText()
    {
        if (songText == null)
        {
            songText = GameObject.Instantiate(miscTextPrefab);
            songText.transform.SetParent(this.transform);
        }
        songText.text = "Songs: ";
        List<Tuple<Card, bool>> cards;
        if (this.name.Contains("Player"))
            cards = Globals.Instance.playerOrderZone.GetCardsWithFaceUp();
        else
            cards = Globals.Instance.enemyOrderZone.GetCardsWithFaceUp();
        int songs = 0;
        int faceups = 0;
        foreach (Tuple<Card, bool> item in cards)
        {
            if (item.Item1.orderType == OrderType.Song)
            {
                songs++;
                if (item.Item2)
                    faceups++;
            }
        }
        songText.text += faceups + "/" + songs;
    }
}
