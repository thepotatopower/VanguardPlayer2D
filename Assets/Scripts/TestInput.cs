using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestInput : MonoBehaviour
{
    public Button rockButton;
    public Button paperButton;
    public Button scissorsButton;
    public int selection;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Dialog());
        Debug.Log("selection: " + selection.ToString());
    }

    IEnumerator Dialog()
    {
        var waitForButton = new WaitForUIButtons(rockButton, scissorsButton, paperButton);
        while (selection < 0)
        {
            if (waitForButton.PressedButton == rockButton)
                selection = 0;
            else if (waitForButton.PressedButton == scissorsButton)
                selection = 1;
            else if (waitForButton.PressedButton == paperButton)
                selection = 2;
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
