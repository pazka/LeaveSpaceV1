using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using DataProcessing.AllGP;
using DataProcessing.Generic;
using SoundProcessing;
using Tools;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
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
    public PureDataConnector pdConnector;
    public float loopDuration = 30;
    public float delayAfterFullLoop = 30;
    public float disappearingRate = 0.02f;
    public float speedCoef = 0.7f;
    public float baseSpeed = 0.002f;
    private float _lastLoopStart = 0;

    private AllGPDataConverter _converter;
    private List<AllGPData> _allGpDataConverted;
    private EventHatcher<DataVisual> _eventHatcher;
    private Queue<DataVisual> _hatchedDataVisuals;
    private Queue<DataVisual> _notYetHatchedDataVisuals;
    private AppStates _currentState = AppStates.NORMAL;
    private float currentIterationTime = 0f;


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
        if (Configuration.GetConfig().isDev)
            return;

        loopDuration = Configuration.GetConfig().loopDuration;
        delayAfterFullLoop = Configuration.GetConfig().delayAfterFullLoop;
        speedCoef = Configuration.GetConfig().speedCoef;
        baseSpeed = Configuration.GetConfig().baseSpeed;
        disappearingRate = Configuration.GetConfig().disappearingRate;
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
    
    private void UpdateCurrentIterationTime()
    {
        currentIterationTime = (Time.time - _lastLoopStart) / loopDuration;
    }

    public void ForwardVisual()
    {
        UpdateCurrentIterationTime();
        var hatched = _eventHatcher.HatchEvents(_notYetHatchedDataVisuals, currentIterationTime);

        foreach (var dataVisual in hatched)
        {
            _hatchedDataVisuals.Enqueue(dataVisual);
        }

        SendOscBangs(hatched);
        UpdateVisualPositions();
        if (_notYetHatchedDataVisuals.Count == 0 && (Time.time - _lastLoopStart >= loopDuration + delayAfterFullLoop))
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
        if (_hatchedDataVisuals.Count == 0)
        {
            _currentState = AppStates.NORMAL;
            _lastLoopStart = Time.time;
        }
    }

    private void UpdateVisualPositions()
    {
        float timeElapsed = Time.time - _lastLoopStart;
        foreach (var dataVisual in _hatchedDataVisuals)
        {
            float originalX = dataVisual.Data.X;
            float originalY = dataVisual.Data.Y;
            float circleSize = (float)Math.Sqrt(originalX * originalX + originalY * originalY) / (float)Math.Sqrt(2);
            float timeOffset = 1 - (0.2f + dataVisual.Data.T * 0.8f) * 10000;
            float timeSpeed = baseSpeed + (speedCoef / 300) * dataVisual.Data.T;
            float timePosition = (timeElapsed) * timeSpeed;

            float x = (float)Math.Cos(timePosition * 2 * Math.PI + timeOffset) * circleSize;
            float y = (float)Math.Sin(timePosition * 2 * Math.PI + timeOffset) * circleSize;

            dataVisual.Visual.transform.localPosition = new Vector3(x, y, 0);
        }
    }
    
    private void SendOscBangs(ICollection<DataVisual> hatchedData)
    {
        pdConnector.SendOscMessage("/data_clock", currentIterationTime);

        foreach (var dataVisual in hatchedData)
        {
            pdConnector.SendOscMessage("/data_bang", 1);
            
            if (dataVisual.Type == "accent")
                pdConnector.SendOscMessage("/data_accent_bang", 1);
        }
    }
}