using System.Collections;
using System.Collections.Generic;
using DataProcessing.AllGP;
using UnityEngine;

public class DataVisual
{
    public AllGPData Data;
    public GameObject Visual;

    public DataVisual(AllGPData data, GameObject visual)
    {
        this.Data = data;
        this.Visual = visual;
    }
}