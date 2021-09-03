using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Globals : MonoBehaviour
{
    public static Globals Instance { get; private set; }
    public CardFightManager cardFightManager;
    public VisualInputManager visualInputManager;
    public GuardianCircle guardianCircle;
    public UnitSlots unitSlots;
    public GameObject playerTriggerZone;
    public GameObject enemyTriggerZone;
    public GameObject playerDamageZone;
    public GameObject enemyDamageZone;
    public GameObject cardPrefab;
    public GameObject AbilityBox;
    public GameObject orderArea;
    public GameObject ZoomIn;
    public Pile playerOrderZone;
    public Pile enemyOrderZone;
    public MiscStats playerMiscStats;
    public MiscStats enemyMiscStats;
    public Button selectionButton1;
    public Button selectionButton2;
    public Button soulCharge;
    public Button counterCharge;
    public Button damage;
    public Button heal;
    public CardBehavior callCard;
    public Vector2 ResetPosition;
    public Vector2 POWPosition;
    public Vector2 SLDPosition;
    public Vector2 YesPosition;
    public Vector2 NoPosition;
    public Vector2 TogglePosition;
    public Vector2 AbilityBoxResetPosition;
    public Vector2 AbilityBoxSlidePosition;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null)
        {
            Debug.Log("more than one instance");
            return;
        }
        Instance = this;
        ResetPosition = new Vector2(-10000, 0);
        POWPosition = new Vector2(-382, 0);
        SLDPosition = new Vector2(382, 0);
        YesPosition = new Vector2(-120, -110);
        NoPosition = new Vector2(120, -110);
        TogglePosition = new Vector2(0, -300);
        AbilityBoxResetPosition = new Vector2(0, 600);
        AbilityBoxSlidePosition = new Vector2(0, 260);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
