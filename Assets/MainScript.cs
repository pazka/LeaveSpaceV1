using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DataProcessing;
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
    FUTURE,
    CONTEMPLATION,
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
    public float speedCoefOfMusk = 1.2f;
    public float baseSpeed = 0.002f;
    public float accelerationRate = 1.05f;
    private float _lastLoopStart = 0;
    public int[] visualDimension = new int[2] { 1920, 2160 };
    public Transform visualPosition = new RectTransform();
    public GameObject progressBar;
    public DebugVisual debugVisual;

    private GPDataConverter _converter;
    private List<IData> _allGpData;
    private EventHatcher<DataVisual> _eventHatcher;
    private Queue<DataVisual> _hatchedDataVisuals;
    private Queue<DataVisual> _notYetHatchedDataVisuals;
    private IDataExtrapolator _extrapolator;
    private AppStates _currentState = AppStates.NORMAL;
    private float _currentIterationTime = 0f;
    private float _currentDataProgress = 0f;
    private ICollection<DataVisual> _latestHatchedDataVisuals;


    public void Start()
    {
        SetValueFromConfig();
        if (Display.displays.Length > 1)
            Display.displays[1].Activate();
        else
            Debug.LogError("No second display detected");

        _eventHatcher = new GpDataEventHatcher(visualPool, accentVisualPool);
        _hatchedDataVisuals = new Queue<DataVisual>();
        _notYetHatchedDataVisuals = new Queue<DataVisual>();
        _allGpData = new List<IData>();

        _converter =
            FactoryDataConverter.GetInstance(FactoryDataConverter.AvailableDataManagerTypes
                .ALLGP) as GPDataConverter;
        if (_converter == null)
            throw new Exception("Converter is null");

        _converter.Init(visualDimension[0], visualDimension[1]);

        _extrapolator =
            FactoryDataExtrapolator.GetInstance(FactoryDataExtrapolator.AvailableDataExtrapolatorTypes.ALLGP);
        LoadDataVisuals();
        _lastLoopStart = Time.time;
    }

    private void SetValueFromConfig()
    {
        if (Configuration.GetConfig().isDev)
            return;

        loopDuration = Configuration.GetConfig().loopDuration;
        delayAfterFullLoop = Configuration.GetConfig().delayAfterFullLoop;
        speedCoef = Configuration.GetConfig().speedCoef;
        speedCoefOfMusk = Configuration.GetConfig().speedCoefOfMusk;
        baseSpeed = Configuration.GetConfig().baseSpeed;
        disappearingRate = Configuration.GetConfig().disappearingRate;
        accelerationRate = Configuration.GetConfig().accelerationRate;
    }

    private void LoadDataVisuals()
    {
        var allGpDataConverted = new List<GPData>();
        while (_converter.GetNextData() is { } data)
        {
            if (data is not GPData gpData)
                throw new Exception("Data is not of type GPData");

            allGpDataConverted.Add(gpData);
        }

        _extrapolator.InitExtrapolation(allGpDataConverted, null);
        _allGpData = _extrapolator.RetrieveExtrapolation().ToList();
        for (var i = 0; i < _allGpData.Count; i++)
        {
            var data = _allGpData[i] as GPData;
            if (data.IsFake)
            {
                _notYetHatchedDataVisuals.Enqueue(new DataVisual(data, accentVisualPool.GetOne()));
            }
            else
            {
                _notYetHatchedDataVisuals.Enqueue(new DataVisual(data, visualPool.GetOne()));
            }
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

        UpdateCurrentIterationTime();

        if (_eventHatcher == null)
            return;

        if (_currentState == AppStates.NORMAL)
        {
            ForwardVisual();
        }
        else if (_currentState == AppStates.FUTURE)
        {
            AccelerateVisual();
            ForwardVisual();
        }
        else if (_currentState == AppStates.CONTEMPLATION)
        {
            AccelerateVisual();
        }
        else if (_currentState == AppStates.RESET)
        {
            RestartVisual();
        }
        else
            throw new Exception("Unknown state");

        UpdateVisualPositions();

        UpdateCurrentDataProgress();
        CheckForStateChange();
    }

    private void UpdateCurrentIterationTime()
    {
        _currentIterationTime = (Time.time - _lastLoopStart) / loopDuration;
        progressBar.transform.localScale = new Vector3(_currentIterationTime * 1920, 10, 1);
    }

    private void UpdateCurrentDataProgress()
    {
        _currentDataProgress = 1f - (float)_allGpData.Count / _notYetHatchedDataVisuals.Count;
    }

    public void CheckForStateChange()
    {
        if (_currentState == AppStates.NORMAL && _hatchedDataVisuals.FirstOrDefault(dv => dv.Data.IsFake) != null)
        {
            // starts future display effects
            _currentState = AppStates.FUTURE;
            debugVisual.AddTextToLog("Future");
        }

        if (_currentState == AppStates.FUTURE &&
            _notYetHatchedDataVisuals.Count == 0 &&
            (Time.time - _lastLoopStart >= loopDuration)
           )
        {
            // contemplate crisis
            _currentState = AppStates.CONTEMPLATION;
            debugVisual.AddTextToLog("Contemplation");
        }

        if ((Time.time - _lastLoopStart >= loopDuration + delayAfterFullLoop))
        {
            // reset loop
            _currentState = AppStates.RESET;
            debugVisual.AddTextToLog("Reset");
        }

        if (_currentState == AppStates.RESET && _hatchedDataVisuals.Count == 0)
        {
            // starts loop
            _currentState = AppStates.NORMAL;
            _lastLoopStart = Time.time;
            debugVisual.AddTextToLog("Normal");
        }
        
    }

    public void ForwardVisual()
    {
        _latestHatchedDataVisuals = _eventHatcher.HatchEvents(_notYetHatchedDataVisuals, _currentIterationTime);

        foreach (var dataVisual in _latestHatchedDataVisuals)
        {
            _hatchedDataVisuals.Enqueue(dataVisual);
        }

        SendOscBangs();
    }

    public void AccelerateVisual()
    {
        speedCoef *= accelerationRate;
    }

    void ReturnVisualsToPool()
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
    }

    private void RestartVisual()
    {
        ReturnVisualsToPool();
    }


    private void UpdateVisualPositions()
    {
        float timeElapsed = Time.time - _lastLoopStart;
        foreach (var dataVisual in _hatchedDataVisuals)
        {
            dataVisual.UpdatePositionFromContext(timeElapsed, visualPosition);
        }
    }

    private void SendOscBangs()
    {
        pdConnector.SendOscMessage("/data_clock", _currentDataProgress);
    }
}