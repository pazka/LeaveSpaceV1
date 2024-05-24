using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    private Vector3 originalScale;
    public GameObject progressBar;
    
    // Start is called before the first frame update
    void Start()
    {
        originalScale = new Vector3(progressBar.transform.localScale.x, progressBar.transform.localScale.y, progressBar.transform.localScale.z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetT(float t)
    {
        progressBar.transform.localScale = new Vector3(originalScale.x * t, 10, 1);
    }
}
