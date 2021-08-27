using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PileSelectArea : MonoBehaviour
{
    public Pile pile;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter()
    {
        pile.OnPointerEnter();
    }

    public void OnPointerClick()
    {
        Globals.Instance.visualInputManager.OnPileClicked(pile);
    }
}
