using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiscStats : MonoBehaviour
{
    public Text miscTextPrefab;
    public Text prisonersText;
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
}
