using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleCardSelect : MonoBehaviour
{
    public GameObject CardSelect;

    public void OnClick()
    {
        if (CardSelect.transform.position == new Vector3(10000, 0, 0))
            CardSelect.transform.localPosition = new Vector3(0, 0, 0);
        else
            CardSelect.transform.position = new Vector3(10000, 0, 0);
    }
}
