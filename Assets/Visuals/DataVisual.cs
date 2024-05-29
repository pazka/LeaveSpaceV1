using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Globalization;
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
    public Renderer RenderRef;
    public readonly double Random;
    public float LastCircleX;
    public float CircleY;
    public float NormalizedCircleY;
    public bool IsAccentVisual;
    private JsonConfiguration config;

    public DataVisual(GPData data, GameObject visual)
    {
        Random = new Random().NextDouble();
        LastCircleX = data.T * 10000;
        CircleY = 0;
        RenderRef = visual.GetComponent<Renderer>();
        IsAccentVisual = IsDataAccent(data);
        config = Configuration.GetConfig();
        
        this.Data = data;
        this.Visual = visual;

        if(IsAccentVisual)
        {
            this.Visual.transform.localScale = new Vector3(config.muskStarSize,config.muskStarSize,1);
        }else
        {
            this.Visual.transform.localScale = new Vector3(config.satStarSize,config.satStarSize,1);
        }
    }

    static public bool IsDataAccent(GPData data)
    {
        return data.ObjectType == ElsetObjectType.MUSK;
    }

    public DataVisual Clone()
    {
        return new DataVisual(Data, Visual);
    }
}