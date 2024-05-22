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
    NORMAL_MUSK,
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
    public float endingBaseSpeed = 1.5f;
    public float startingBaseSpeed = 0.002f;
    public float speedToTCoef = 0.7f;
    public float fasterMuskCoef = 3f;
    private float _lastLoopStart = 0;
    public int[] visualDimension = new int[2] { 1920, 2160 };
    public Transform visualPosition = new RectTransform();
    public GameObject progressBar;
    public DebugVisual debugVisual;

    private GPDataConverter _converter;
    private List<IData> _allGpData;
    private EventHatcher<DataVisual> _eventHatcher;
    private float currentSpeedCoef = 0.7f;
    private float accelerationStartDataProgress = 0f;
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
        currentSpeedCoef = startingBaseSpeed;
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
        speedToTCoef = Configuration.GetConfig().speedToTCoef;
        startingBaseSpeed = Configuration.GetConfig().startingBaseSpeed;
        endingBaseSpeed = Configuration.GetConfig().endingBaseSpeed;
        fasterMuskCoef = Configuration.GetConfig().fasterMuskCoef;
        startingBaseSpeed = Configuration.GetConfig().startingBaseSpeed;
        disappearingRate = Configuration.GetConfig().disappearingRate;
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

    public void CheckForStateChange()
    {
        if (_currentState == AppStates.NORMAL &&
            _hatchedDataVisuals.FirstOrDefault(dv => dv.Data.ObjectType == ElsetObjectType.MUSK) != null)
        {
            // starts future display effects
            _currentState = AppStates.NORMAL_MUSK;
            accelerationStartDataProgress = _currentDataProgress;
            debugVisual.AddTextToLog("Musk appeared !");
        }

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
        else if (_currentState == AppStates.FUTURE || _currentState == AppStates.NORMAL_MUSK)
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
        SendOscBangs();
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

    public void ForwardVisual()
    {
        _latestHatchedDataVisuals = _eventHatcher.HatchEvents(_notYetHatchedDataVisuals, _currentIterationTime);

        foreach (var dataVisual in _latestHatchedDataVisuals)
        {
            _hatchedDataVisuals.Enqueue(dataVisual);
        }
    }

    public void AccelerateVisual()
    {
        var accelerationDataProgressRange = 1 - accelerationStartDataProgress;
        var currentAccelerationDataProgress = _currentDataProgress - accelerationStartDataProgress;
        var normalizedAccelerationDataProgress = currentAccelerationDataProgress / accelerationDataProgressRange;
        
        var coefRange = endingBaseSpeed - startingBaseSpeed;
        currentSpeedCoef = startingBaseSpeed + (endingBaseSpeed - startingBaseSpeed) * normalizedAccelerationDataProgress;
    }

    void ReturnVisualsToPool()
    {
        var nbOfObjectsToDisappear = Math.Max(_hatchedDataVisuals.Count * disappearingRate,
            Math.Min(_hatchedDataVisuals.Count, 200));

        for (var i = 0; i < nbOfObjectsToDisappear; i++)
        {
            var dataVisual = _hatchedDataVisuals.Dequeue();
            if (dataVisual.Data.IsFake)
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
        foreach (var dataVisual in _hatchedDataVisuals)
        {
            UpdatePositionFromContext(dataVisual);
        }
    }

    private void UpdatePositionFromContext(DataVisual dataVisual)
    {
        float timeElapsed = Time.time - _lastLoopStart;
        float originalX = dataVisual.Data.X;
        float originalY = dataVisual.Data.Y;
        float minCircle = Configuration.GetConfig().minStartingCircleSize;
        float circleSize = minCircle +
                           (float)Math.Sqrt(originalX * originalX + originalY * originalY) / (float)Math.Sqrt(2);

        float timeOffset = 1 - (0.2f + dataVisual.Data.T * 0.8f) * 10000;

        float speedToTCoef = Configuration.GetConfig().speedToTCoef;
        if (dataVisual.Data.ObjectType == ElsetObjectType.MUSK)
        {
            speedToTCoef *= fasterMuskCoef;
        }

        float timeSpeed = currentSpeedCoef + (speedToTCoef / 300) * dataVisual.Data.T;
        float timePosition = (timeElapsed) * timeSpeed;

        float x = (float)Math.Cos(timePosition * 2 * Math.PI + timeOffset) * circleSize;
        float y = (float)Math.Sin(timePosition * 2 * Math.PI + timeOffset) * circleSize;

        dataVisual.Visual.transform.localPosition = visualPosition.localPosition + new Vector3(x, y, 0);
    }

    private void SendOscBangs()
    {
        pdConnector.SendOscMessage("/data_clock", _currentDataProgress);
    }
}