using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VanguardEngine;

public class CardBehavior : MonoBehaviour
{
    public GameObject Card;
    public GameObject PlayerHand;
    public GameObject HandCard;
    public GameObject selectedCardPrefab;
    public GameObject selectedCard;
    public CardFightManager cardFightManager;
    public VisualInputManager inputManager;
    public GameObject ZoomIn;
    public Camera cam;
    public Canvas canvas;
    public Text CardName;
    public Text CardEffect;
    public Card card;
    public string cardID;
    public bool faceup = false;
    public int layer = 0;
    public bool selected = false;
    public bool selectable = false;
    public bool inAnimation = false;
    public IEnumerator coroutine;
    Vector3 position, rotation, scale, originalPosition;

    void Update()
    {

    }

    void Start()
    {
        cardFightManager = GameObject.Find("CardFightManager").GetComponent<CardFightManager>();
        inputManager = GameObject.Find("InputManager").GetComponent<VisualInputManager>();
        ZoomIn = GameObject.Find("ZoomIn");
        CardName = GameObject.Find("CardName").GetComponent<Text>();
        CardEffect = GameObject.Find("CardEffect").GetComponent<Text>();
        PlayerHand = GameObject.Find("PlayerHand");
        cam = GameObject.Find("MainCamera").GetComponent<Camera>();
        canvas = GameObject.Find("MainCanvas").GetComponent<Canvas>();
    }

    public void DisplayCard()
    {
        Card card;
        string effect;
        if (originalPosition == new Vector3(0, 0, 0))
            originalPosition = this.transform.position;
        if (this.cardID != "" && (this.faceup || (!this.faceup && this.transform.parent != GameObject.Find("EnemyHand").transform && !this.transform.parent.name.Contains("Deck"))))
        {
            if (cardFightManager == null)
                Debug.Log("cardFightManager is null");
            card = cardFightManager.LookUpCard(cardID);
            CardName.text = card.name;
            effect = "[Power: " + card.power + "] [Shield: " + card.shield + "] [Grade: " + card.grade + "]\n" + card.effect;
            CardEffect.text = effect;
            ZoomIn.GetComponent<Image>().sprite = this.GetComponent<Image>().sprite;
        }
        if (this.transform.parent.name == "PlayerHand" && inputManager.cardsAreHoverable)
        {
            //this.transform.localScale *= 3;
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = HoverCard(true);
            StartCoroutine(coroutine);
        }
    }

    public void RevertCard()
    {
        if (this.transform.parent.name == "PlayerHand" && inputManager.cardsAreHoverable)
        {
            //this.transform.localScale /= 3;
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = HoverCard(false);
            StartCoroutine(coroutine);
        }
    }

    public void ClickCard()
    {
        if (this.transform.parent.name == "PlayerHand")
        {
            GridLayout gridLayout = PlayerHand.GetComponent<GridLayout>();
            if (inputManager.cardsAreSelectable)
            {
                Debug.Log("clicked");
                if (selected)
                {
                    GameObject.Destroy(selectedCard);
                    selected = false;
                }
                else
                {
                    selectedCard = GameObject.Instantiate(selectedCardPrefab);
                    selectedCard.transform.position = this.transform.position;
                    selectedCard.transform.SetParent(GameObject.Find("MainCanvas").transform);
                    selectedCard.transform.SetSiblingIndex(PlayerHand.transform.GetSiblingIndex() - 1);
                    selected = true;
                }
            }
            if (selectable)
                inputManager.OnCardClicked(Card);
        }
    }

    IEnumerator HoverCard(bool up)
    {
        Vector3 oldPosition;
        float direction = 0;
        if (up)
        {
            //direction = PlayerHand.transform.position.y + (ConvertToUnits(RectTransformUtility.PixelAdjustRect(this.GetComponent<RectTransform>(), canvas).height) * 0.15f);
            direction = PlayerHand.transform.position.y + this.GetComponent<RectTransform>().rect.height * 0.15f;
        }
        else
        {
            if (originalPosition != null)
                direction = originalPosition.y;
        }
        Vector3 newPosition = new Vector3(this.transform.position.x, direction, 0);
        float step = 150 * Time.deltaTime;
        while (Vector3.Distance(this.transform.position, new Vector3(this.transform.position.x, direction, this.transform.position.z)) > 0.001f)
        {
            oldPosition = this.transform.position;
            this.transform.position = Vector2.MoveTowards(this.transform.position, new Vector2(this.transform.position.x, direction), step);
            if (selectedCard != null)
                selectedCard.transform.position = Vector2.MoveTowards(this.transform.position, new Vector2(this.transform.position.x, direction), step);
            if (oldPosition == this.transform.position)
                break;
            //Debug.Log("hovering");
            yield return null;
        }
        this.transform.position = new Vector2(this.transform.position.x, direction);
    }

    public void MarkAsSelectable()
    {
        GameObject.Destroy(selectedCard);
        selectedCard = GameObject.Instantiate(selectedCardPrefab);
        Debug.Log("instantiated");
        selectedCard.transform.position = this.transform.position;
        selectedCard.transform.SetParent(GameObject.Find("MainCanvas").transform);
        selectedCard.transform.SetSiblingIndex(PlayerHand.transform.GetSiblingIndex() - 1);
        selectedCard.transform.GetComponent<Image>().color = Color.cyan;
        selectable = true;
    }

    public IEnumerator Flip()
    {
        Debug.Log("cardbehavior is flipping");
        inAnimation = true;
        float step = 400 * Time.deltaTime;
        Quaternion Ninety;
        if (this.transform.parent.gameObject.GetComponent<DamageZone>() != null)
            Ninety = Quaternion.Euler(this.transform.localRotation.eulerAngles.x + 90, this.transform.localRotation.eulerAngles.y, this.transform.localRotation.eulerAngles.z);
        else
            Ninety = Quaternion.Euler(this.transform.localRotation.eulerAngles.x, this.transform.localRotation.eulerAngles.y + 90, this.transform.localRotation.eulerAngles.z);
        Quaternion Zero = Quaternion.Euler(this.transform.localRotation.eulerAngles.x, this.transform.localRotation.eulerAngles.y, this.transform.localRotation.eulerAngles.z);
        Debug.Log("current: " + this.transform.localRotation.eulerAngles.ToString());
        Debug.Log("ninety: " + Ninety.eulerAngles.ToString());
        Debug.Log("zero: " + Zero.eulerAngles.ToString());
        while (Quaternion.Angle(this.transform.localRotation, Ninety) > 0.01f)
        {
            this.transform.localRotation = Quaternion.RotateTowards(this.transform.localRotation, Ninety, step);
            yield return null;
        }
        this.transform.localRotation = Ninety;
        if (faceup)
        {
            faceup = false;
            this.GetComponent<Image>().sprite = CardFightManager.LoadSprite(Application.dataPath + "/../cardart/FaceDownCard.jpg");
        }
        else
        {
            faceup = true;
            this.GetComponent<Image>().sprite = CardFightManager.LoadSprite(CardFightManager.FixFileName(cardID));
        }
        while (Quaternion.Angle(this.transform.localRotation, Zero) > 0.01f)
        {
            this.transform.localRotation = Quaternion.RotateTowards(this.transform.localRotation, Zero, step);
            yield return null;
        }
        this.transform.localRotation = Zero;
        inAnimation = false;
    }

    public IEnumerator Flash(Color color)
    {
        for (int i = 0; i < 6; i++)
        {
            MarkAsSelectable();
            selectedCard.GetComponent<Image>().color = color;
            yield return new WaitForSecondsRealtime((float)0.10);
            Reset();
            yield return new WaitForSecondsRealtime((float)0.10);
            Debug.Log("flashing");
        }
        Reset();
        inAnimation = false;
    }

    public void Reset()
    {
        selected = false;
        selectable = false;
        GameObject.Destroy(selectedCard);
    }

    float ConvertToUnits(float p)
    {
        float ortho = cam.orthographicSize;
        float pixelH = cam.pixelHeight;
        return (p * ortho * 2) / pixelH;
    }
}
