using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VanguardEngine;
using UnityEngine.UI;

public class VisualInputManager : MonoBehaviour
{
    public Button rockButton;
    public Button scissorsButton;
    public Button paperButton;
    public IM inputManager;

    public class IM : InputManager
    {
        public Button rockButton;
        public Button scissorsButton;
        public Button paperButton;

        public override int RPS()
        {
            rockButton.transform.position = new Vector3(-175, 0, 0);
            scissorsButton.transform.position = new Vector3(0, 0, 0);
            paperButton.transform.position = new Vector3(175, 0, 0);
            int selection = 0;
            IEnumerator Dialog()
            {
                var waitForButton = new WaitForUIButtons(rockButton, scissorsButton, paperButton);
                yield return waitForButton.Reset();
                if (waitForButton.PressedButton == rockButton)
                    selection = 0;
                else if (waitForButton.PressedButton == scissorsButton)
                    selection = 1;
                else if (waitForButton.PressedButton == paperButton)
                    selection = 2;
            }
            Dialog();
            //perform animation
            return selection;
        }
    }

    public IM InitialzeInputManager()
    {
        inputManager = new IM();
        inputManager.rockButton = rockButton;
        inputManager.scissorsButton = scissorsButton;
        inputManager.paperButton = paperButton;
        return inputManager;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
