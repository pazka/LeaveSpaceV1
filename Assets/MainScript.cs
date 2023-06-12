using System;
using System.Collections.Generic;
using DataProcessing.AllGP;
using DataProcessing.Generic;
using UnityEngine;
using Visuals;

public class MainScript : MonoBehaviour
{
    public VisualPool visualPool;

    private AllGPDataConverter _converter;
    private List<AllGPData> _allGpDataConverted;
    private EventHatcher<DataVisual> _eventHatcher;
    private Queue<DataVisual> _hatchedDataVisuals;
    private Queue<DataVisual> _notYetHatchedDataVisuals;
    public float LoopDuration = 300;

    public void Start()
    {
        visualPool.PreloadNObjects(30000);
        _eventHatcher = new AllGpDataEventHatcher();
        _hatchedDataVisuals = new Queue<DataVisual>();
        _notYetHatchedDataVisuals = new Queue<DataVisual>();
        _allGpDataConverted = new List<AllGPData>();

        _converter =
            FactoryDataConverter.GetInstance(FactoryDataConverter.AvailableDataManagerTypes
                .ALLGP) as AllGPDataConverter;
        if (_converter == null)
            throw new Exception("Converter is null");

        _converter.Init(1920, 1080);
        FillVisualPool();
    }

    private void FillVisualPool()
    {
        while (_converter.GetNextData() is { } data)
        {
            if (data is not AllGPData gpData)
                throw new Exception("Data is not of type AllGPData");

            _allGpDataConverted.Add(gpData);
            var dataVisual = new DataVisual(gpData, visualPool.GetOne());
            dataVisual.Visual.SetActive(false);
            dataVisual.Visual.transform.position = new Vector3(gpData.X, gpData.Y, 0);
            _notYetHatchedDataVisuals.Enqueue(dataVisual);
        }
    }

    // Update is called once per frame
    public void Update()
    {
        if (_eventHatcher == null)
            return;

        var hatched = _eventHatcher.HatchEvents(_notYetHatchedDataVisuals, Time.time / LoopDuration);
        foreach (var dataVisual in hatched)
        {
            _hatchedDataVisuals.Enqueue(dataVisual);
        }

        foreach (var dataVisual in _hatchedDataVisuals)
        {
            float originalX = dataVisual.Data.X;
            float originalY = dataVisual.Data.Y;
            float circleSize = (float)Math.Sqrt(originalX * originalX + originalY * originalY) / (float)Math.Sqrt(2);
            float timeStart = 1 - (0.2f + dataVisual.Data.T * 0.8f);
            float timePosition = dataVisual.Data.T * 1000 + timeStart * Time.time / 100;

            //Make the object go around in a circle 
            dataVisual.Visual.transform.position = new Vector3(
                (float)Math.Cos(timePosition * Math.PI) * circleSize,
                (float)Math.Sin(timePosition * Math.PI) * circleSize,
                0);
        }
    }
}