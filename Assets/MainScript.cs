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
            dataVisual.Visual.transform.position = new Vector3(gpData.X , gpData.Y , 0);
            _notYetHatchedDataVisuals.Enqueue(dataVisual);
        }
    }

    // Update is called once per frame
    public void Update()
    {
        if (_eventHatcher == null)
            return;
        
        float timePassed = Time.time / LoopDuration;
        var hatched = _eventHatcher.HatchEvents(_notYetHatchedDataVisuals, timePassed);
        foreach (var dataVisual in hatched)
        {
            _hatchedDataVisuals.Enqueue(dataVisual);
        }

        foreach (var dataVisual in _hatchedDataVisuals)
        {
            var data = dataVisual.Data;
            float circleSize = float.Parse(dataVisual.Data.RawJson.PERIAPSIS);
            //transform the orignal x,y position of gameobject to go around in a circle given the time and the circleSize parmeter
            float x = Mathf.Cos(timePassed * 2 * Mathf.PI) * circleSize;
            float y = Mathf.Sin(timePassed * 2 * Mathf.PI) * circleSize;
            dataVisual.Visual.transform.position = new Vector3(x, y, 0);
        }
    }
}