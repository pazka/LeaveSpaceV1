using System;
using System.Collections;
using System.Collections.Generic;
using DataProcessing.AllGP;
using UnityEngine;
using Random = System.Random;

public class DataVisual
{
    static Random _rnd = new Random();
    public AllGPData Data;
    public GameObject Visual;
    public string Type="basic";
    public float rnd;

    public DataVisual(AllGPData data, GameObject visual)
    {
        this.Data = data;
        this.Visual = visual;
        rnd = (float)_rnd.NextDouble();
    }
}