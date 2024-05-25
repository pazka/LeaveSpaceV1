using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainTesttGeoScript : MonoBehaviour
{
    public Scrollbar scrollbar;
    public GameObject cube;
    public float _currentvalue = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _currentvalue += 0.001f;
        
        if (_currentvalue > 1)
        {
            _currentvalue = 0;
        }
        var cubePosition = cube.transform.position;
        
        //X out of sin
        cubePosition.x = 1000 * Mathf.Sin(_currentvalue * 2.0f * Mathf.PI );
        
        //Y out of cos
        cubePosition.y = 1000 * Mathf.Cos(_currentvalue * 2.0f * Mathf.PI );
        
        cube.transform.position = cubePosition;
    }
}
