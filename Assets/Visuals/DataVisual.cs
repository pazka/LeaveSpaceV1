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

    public DataVisual(GPData data, GameObject visual)
    {
        this.Data = data;
        this.Visual = visual;
        Random = new Random().NextDouble();
        LastCircleX = data.T * 10000;
        CircleY = 0;
        RenderRef = Visual.GetComponent<Renderer>();
        IsAccentVisual = IsDataAccent(data);
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