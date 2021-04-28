using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VanguardEngine;

public class CardBehavior : MonoBehaviour
{
    public GameObject Card;
    public GameObject PlayerHand;
    public GameObject HandCard;
    Vector3 position, rotation, scale;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DrawCard(string tempID)
    {
        StartCoroutine(RotateImage());
    }

    IEnumerator RotateImage()
    {
        float moveSpeed = 0.3f;
        float y = 180;
        //position = Card.transform.localPosition;
        //rotation = Card.transform.localRotation.eulerAngles;
        //scale = Card.transform.localScale;
        //Card.transform.parent = PlayerHand.transform;
        //Card.transform.localPosition = position;
        //Card.transform.localRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
        //Card.transform.localScale = new Vector3(0.7f, 1, 0.01f);
        Card.transform.SetParent(HandCard.transform);
        while (Card.transform.localRotation.eulerAngles != HandCard.transform.localRotation.eulerAngles)
        {
            Card.transform.localRotation = Quaternion.Slerp(Card.transform.localRotation, Quaternion.Euler(HandCard.transform.localRotation.eulerAngles.x, HandCard.transform.localRotation.eulerAngles.y, HandCard.transform.localRotation.eulerAngles.z), moveSpeed * Time.time);
            Card.transform.localPosition = Vector3.MoveTowards(Card.transform.localPosition, HandCard.transform.localPosition, Time.time * moveSpeed);
            yield return null;
        }
        Card.transform.localRotation = Quaternion.Euler(HandCard.transform.localRotation.eulerAngles.x, HandCard.transform.localRotation.eulerAngles.y, HandCard.transform.localRotation.eulerAngles.z);
        yield return null;
    }
}
