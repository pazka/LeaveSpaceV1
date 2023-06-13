using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using DataProcessing.AllGP;
using DataProcessing.Generic;
using Tools;
using Unity.VisualScripting;
using UnityEngine;
using Visuals;

public enum AppStates
{
    NORMAL,
    RESET
}

public class MainScript : MonoBehaviour
{
    public VisualPool visualPool;
    public VisualPool accentVisualPool;
    public float LoopDuration = 1000;
    public int delayAfterFullLoop = 60;
    public float disappearingRate = 0.2f;
    private float _lastLoopStart = 0;

    private AllGPDataConverter _converter;
    private List<AllGPData> _allGpDataConverted;
    private EventHatcher<DataVisual> _eventHatcher;
    private Queue<DataVisual> _hatchedDataVisuals;
    private Queue<DataVisual> _notYetHatchedDataVisuals;
    private AppStates _currentState = AppStates.NORMAL;


    public void Start()
    {
        SetValueFromConfig();
        _eventHatcher = new AllGpDataEventHatcher(visualPool, accentVisualPool);
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
        _lastLoopStart = Time.time;
    }

    private void SetValueFromConfig()
    {
        LoopDuration = Configuration.GetConfig().loopDuration;
    }

    private void FillVisualPool()
    {
        while (_converter.GetNextData() is { } data)
        {
            if (data is not AllGPData gpData)
                throw new Exception("Data is not of type AllGPData");

            _allGpDataConverted.Add(gpData);
            var dataVisual = new DataVisual(gpData, null);
            _notYetHatchedDataVisuals.Enqueue(dataVisual);
        }
    }

    // Update is called once per frame
    public void Update()
    {
        //check if key is pressed
        if (Input.GetKeyDown(KeyBindings.Quit))
        {
            Application.Quit();
        }

        if (_eventHatcher == null)
            return;

        if (_currentState == AppStates.NORMAL)
            ForwardVisual();
        else if (_currentState == AppStates.RESET)
            RestartVisual();
        else
            throw new Exception("Unknown state");
    }

    public void ForwardVisual()
    {
        var hatched = _eventHatcher.HatchEvents(_notYetHatchedDataVisuals, (Time.time - _lastLoopStart) / LoopDuration);

        foreach (var dataVisual in hatched)
        {
            _hatchedDataVisuals.Enqueue(dataVisual);
        }

        UpdateVisualPositions();
        if(_notYetHatchedDataVisuals.Count == 0 && (Time.time - _lastLoopStart >= LoopDuration + delayAfterFullLoop))
        {
            _currentState = AppStates.RESET;
        }
    }

    private void RestartVisual()
    {
        var nbOfObjectsToDisappear = Math.Max(_hatchedDataVisuals.Count * disappearingRate,
            Math.Min(_hatchedDataVisuals.Count, 200));

        for (var i = 0; i < nbOfObjectsToDisappear; i++)
        {
            var dataVisual = _hatchedDataVisuals.Dequeue();
            if (dataVisual.Type == "accent")
                accentVisualPool.Return(dataVisual.Visual);
            else
                visualPool.Return(dataVisual.Visual);

            _notYetHatchedDataVisuals.Enqueue(dataVisual);
        }

        UpdateVisualPositions();
        if(_hatchedDataVisuals.Count == 0)
        {
            _currentState = AppStates.NORMAL;
            _lastLoopStart = Time.time;
        }
    }

    private void UpdateVisualPositions()
    {
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