using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugVisual : MonoBehaviour
{
    public TextMeshPro debugText;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void AddTextToLog(string text)
    {
        if (!gameObject.activeSelf)
            return;
        
        var nextText = text + "\n" + debugText.text; 
        if (nextText.Length > 500)
            nextText = nextText.Substring(0, 1000);
        debugText.SetText(nextText);
    }
}