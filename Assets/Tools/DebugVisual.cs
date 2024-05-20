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

        debugText.text = text + "\n" + debugText.text;
    }
}