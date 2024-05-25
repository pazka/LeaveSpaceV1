using System;
using System.Collections;
using System.Collections.Generic;
using DataProcessing.AllGP;
using Tools;
using UnityEngine;
using Random = System.Random;

public class DataVisual
{
    static Random _rnd = new Random();
    public GPData Data;
    public GameObject Visual;
    public string Type = "basic";
    public float rnd;
    private JsonConfiguration _config;

    public DataVisual(GPData data, GameObject visual)
    {
        this.Data = data;
        this.Visual = visual;
        rnd = (float)_rnd.NextDouble();
        _config = Configuration.GetConfig();
    }

    public DataVisual Clone()
    {
        return new DataVisual(Data, Visual);
    }

    public void UpdatePositionFromContext(float timeElapsed, Transform visualPosition, float accelerationFactor,
        float muskAcceleartionFactor)
    {
        float originalX = Data.X;
        float originalY = Data.Y;
        float minCircle = _config.minStartingCircleSize;
        float circleSize = minCircle +
                           (float)Math.Sqrt(originalX * originalX + originalY * originalY) / (float)Math.Sqrt(2);

        float timeOffset = 1 - (0.2f + Data.T * 0.8f) * 10000;

        float speedCoef = accelerationFactor;
        if (Data.ObjectType == ElsetObjectType.MUSK)
        {
            speedCoef *= muskAcceleartionFactor;
        }

        float timeSpeed = _config.baseSpeed + (speedCoef / 300) * Data.T;
        float timePosition = (timeElapsed) * timeSpeed;

        float x = (float)Math.Cos(timePosition * 2 * Math.PI + timeOffset) * circleSize;
        float y = (float)Math.Sin(timePosition * 2 * Math.PI + timeOffset) * circleSize;

        Visual.transform.localPosition = visualPosition.localPosition + new Vector3(x, y, 0);
    }
}