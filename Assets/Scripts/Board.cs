using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Board : MonoBehaviour
{
    public bool inAnimation = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator ZoomInOnTrigger(bool player)
    {
        GameObject trigger = null;
        if (player)
            trigger = Globals.Instance.playerTriggerZone;
        else
            trigger = Globals.Instance.enemyTriggerZone;
        float step = 2000 * Time.deltaTime;
        Vector2 targetDistance = new Vector2(0, 0);
        //targetDistance = new Vector2(this.transform.position.x - trigger.transform.position.x, this.transform.position.y - trigger.transform.position.y) * (float)(8 / 1.2 / 2);
        if (player)
            targetDistance = new Vector2(-1879, 259);
        else
            targetDistance = new Vector2(1879, -259);
        //Debug.Log("target distance: " + targetDistance);
        //Debug.Log("trigger position: " + trigger.transform.position);
        //Debug.Log("field initial position: " + this.transform.position);
        float zoomstep = step * (Vector3.Distance(this.transform.localScale, new Vector3(8, 8, 8)) / Vector3.Distance(this.transform.localPosition, targetDistance));
        while (Vector3.Distance(this.transform.localPosition, targetDistance) > 0.001f)
        {
            this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, targetDistance, step);
            this.transform.localScale = Vector3.MoveTowards(this.transform.localScale, new Vector3(8, 8, 8), zoomstep);
            yield return null;
        }
        this.transform.localPosition = targetDistance;
        this.transform.localScale = new Vector3(8, 8, 8);
        Debug.Log("field current position: " + this.transform.localPosition);
        inAnimation = false;
    }

    public IEnumerator ZoomBack()
    {
        float step = 2000 * Time.deltaTime;
        Vector2 targetDistance = new Vector2(0, 0);
        float zoomstep = step * (Vector3.Distance(this.transform.localScale, new Vector3((float)1.2, (float)1.2, (float)1.2)) / Vector3.Distance(this.transform.localPosition, targetDistance));
        while (Vector3.Distance(this.transform.localPosition, targetDistance) > 0.001f)
        {
            this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, targetDistance, step);
            this.transform.localScale = Vector3.MoveTowards(this.transform.localScale, new Vector3((float)1.2, (float)1.2, (float)1.2), zoomstep);
            yield return null;
        }
        this.transform.localPosition = new Vector2(0, 0);
        this.transform.localScale = new Vector3((float)1.2, (float)1.2, (float)1.2);
        inAnimation = false;
    }
}
