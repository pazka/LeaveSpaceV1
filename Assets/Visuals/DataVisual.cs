using System;
using System.Collections;
using System.Collections.Generic;
using DataProcessing.AllGP;
using Tools;
using UnityEngine;
using Random = System.Random;

public class DataVisual
{
    public GPData Data;
    public GameObject Visual;
    public readonly double Random;

    public DataVisual(GPData data, GameObject visual)
    {
        this.Data = data;
        this.Visual = visual;
        Random = new Random().NextDouble();
    }

    public DataVisual Clone()
    {
        return new DataVisual(Data, Visual);
    }
}