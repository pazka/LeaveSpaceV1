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
using Logger = Tools.Logger;

public enum AppStates
{
    NORMAL,
    NORMAL_MUSK,
    FUTURE,
    CONTEMPLATION,
    COLLAPSE,
    END
}

public class MainScript : MonoBehaviour
{
    public Logger logger;
    public VisualPool visualPool;
    public VisualPool accentVisualPool;
    public PureDataConnector pdConnector;
    private float _lastLoopStart = 0;
    public int[] visualDimension = new int[2] { 1920, 2160 };
    public Transform visualPosition = new RectTransform();
    public GameObject progressBar;
    public ProgressBar progressBarScript;

    private System.Random _rnd = new System.Random();
    private GPDataConverter _converter;
    private List<GPData> _allOrigGpData;
    private List<GPData> _allGpData;
    private EventHatcher<DataVisual> _eventHatcher;
    private JsonConfiguration config;
    private float currentSpeed;
    private Vector3 currentCircleScale;
    private float muskApparitionTime;
    private float futureStartingTime;
    private float collapseStartingTime;
    private float endStartingTime;
    private Queue<DataVisual> _hatchedDataVisuals;
    private Queue<DataVisual> _notYetHatchedDataVisuals;
    private IDataExtrapolator _extrapolator;
    private AppStates _currentState = AppStates.NORMAL;
    private float _currentDataLoopTime;
    private float _currentFullLoopTime;
    private float _currentDataProgress;
    private ICollection<DataVisual> _latestHatchedDataVisuals;


    public void Start()
    {
        config = Configuration.GetConfig();
        currentSpeed = config.midBaseSpeed;
        if (Display.displays.Length > 1)
            Display.displays[1].Activate();

        _eventHatcher = new GpDataEventHatcher(visualPool, accentVisualPool);
        _allGpData = new List<GPData>();

        _converter =
            FactoryDataConverter.GetInstance(FactoryDataConverter.AvailableDataManagerTypes
                .ALLGP) as GPDataConverter;
        if (_converter == null)
            throw new Exception("Converter is null");

        _converter.Init(visualDimension[0], visualDimension[1]);

        _allOrigGpData = _converter.GetAllData().Cast<GPData>().ToList();

        _extrapolator =
            FactoryDataExtrapolator.GetInstance(FactoryDataExtrapolator.AvailableDataExtrapolatorTypes.ALLGP);
        _extrapolator.InitExtrapolation(_allOrigGpData, null);

        InitLoop();
    }

    private void SetupTimeCodes()
    {
        var firstMuskData = _allGpData.FirstOrDefault(d => d.ObjectType == ElsetObjectType.MUSK);
        var firstFutureData = _allGpData.FirstOrDefault(d => d.IsFake);

        muskApparitionTime = firstMuskData?.T ?? 0;
        futureStartingTime = firstFutureData?.T ?? 0;

        logger.Log($"TIMECODE : Musk apparition time : {muskApparitionTime}");
        logger.Log($"TIMECODE : Future starting time : {futureStartingTime}");
    }

    private void CheckForStateChange()
    {
        var elapsedTime = Time.time - _lastLoopStart;
        
        if (_currentState == AppStates.NORMAL && _currentDataLoopTime >= muskApparitionTime)
        {
            _currentState = AppStates.NORMAL_MUSK;
            logger.Log($"Musk : {_currentDataLoopTime} / {_currentFullLoopTime}");
        }
        else if (_currentState == AppStates.NORMAL_MUSK && _currentDataLoopTime >= futureStartingTime)
        {
            _currentState = AppStates.FUTURE;
            logger.Log($"Future : {_currentDataLoopTime} / {_currentFullLoopTime}");
        }
        else if (_currentState == AppStates.FUTURE &&
                 _notYetHatchedDataVisuals.Count == 0 &&
                 elapsedTime >= config.loopDuration)
        {
            _currentState = AppStates.CONTEMPLATION;
            logger.Log($"Contemplation : {_currentDataLoopTime} / {_currentFullLoopTime}");
        }
        else if (elapsedTime >= config.loopDuration + config.contemplationDelay &&
                 _currentState == AppStates.CONTEMPLATION)
        {
            collapseStartingTime = Time.time;
            _currentState = AppStates.COLLAPSE;
            logger.Log($"Collapse : {_currentDataLoopTime} / {_currentFullLoopTime}");
        }
        else if (_currentState == AppStates.COLLAPSE && _hatchedDataVisuals.Count == 0)
        {
            endStartingTime = Time.time;
            _currentState = AppStates.END;
            logger.Log($"End : {_currentDataLoopTime} / {_currentFullLoopTime}");
        }
        else if (_currentState == AppStates.END && Time.time - endStartingTime > config.endDuration)
        {
            InitLoop();
            _currentState = AppStates.NORMAL;
            logger.Log("Normal : " + _currentDataLoopTime);
        }
    }

    private void InitLoop()
    {
        currentSpeed = config.midBaseSpeed;
        _lastLoopStart = Time.time;
        _allGpData = (_extrapolator.RetrieveExtrapolation() as IEnumerable<GPData>).ToList();
        _hatchedDataVisuals = new Queue<DataVisual>(_allGpData.Count);
        _notYetHatchedDataVisuals = new Queue<DataVisual>(_allGpData.Count);
        foreach (var data in _allGpData.Select(t => t as GPData))
        {
            _notYetHatchedDataVisuals.Enqueue(DataVisual.IsDataAccent(data)
                ? new DataVisual(data, accentVisualPool.GetOne())
                : new DataVisual(data, visualPool.GetOne()));
        }
        LogCounts();
        SetupTimeCodes();
        UpdateCurrentIterationTime();
        _extrapolator.InitExtrapolation(_allOrigGpData, null);
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

        CheckForStateChange();

        if (_currentState == AppStates.NORMAL)
        {
            AdaptSatCircleSize();
            AccelerateSatVisual();
            ForwardVisual();
            UpdateVisualPositions();
        }
        else if (_currentState == AppStates.NORMAL_MUSK || _currentState == AppStates.FUTURE)
        {
            AccelerateMuskVisual();
            ForwardVisual();
            UpdateVisualPositions();
        }
        else if (_currentState == AppStates.CONTEMPLATION)
        {
            UpdateVisualPositions();
        }
        else if (_currentState == AppStates.COLLAPSE)
        {
            ReturnSomeVisualsToPool();
            EaseInSlowingVisual();
        }

        UpdateCurrentDataProgress();
        UpdateCurrentIterationTime();
        SendOscBangs();
    }

    private void UpdateCurrentIterationTime()
    {
        _currentDataLoopTime = (Time.time - _lastLoopStart) / (config.loopDuration);
        _currentFullLoopTime = (Time.time - _lastLoopStart) / (config.loopDuration + config.contemplationDelay);
        progressBarScript.SetT(_currentFullLoopTime);
    }

    private void UpdateCurrentDataProgress()
    {
        _currentDataProgress = 1f - ((float)_notYetHatchedDataVisuals.Count / (float)_allGpData.Count);
    }

    private void ForwardVisual()
    {
        _latestHatchedDataVisuals = _eventHatcher.HatchEvents(_notYetHatchedDataVisuals, _currentDataLoopTime);

        foreach (var dataVisual in _latestHatchedDataVisuals)
        {
            _hatchedDataVisuals.Enqueue(dataVisual);
        }
    }

    private void AdaptSatCircleSize()
    {
        var normalizedProgression = (_currentDataLoopTime) / muskApparitionTime;

        var tmpCircleSize = Mathf.Lerp(config.startStarSize, config.midStarSize, normalizedProgression);
        currentCircleScale.Set(tmpCircleSize, tmpCircleSize, 1);
    }

    private void AccelerateSatVisual()
    {
        var normalizedProgression =
            (_currentDataLoopTime) / muskApparitionTime;

        currentSpeed = Mathf.Lerp(config.startBaseSpeed, config.midBaseSpeed, normalizedProgression);
    }

    private void AccelerateMuskVisual()
    {
        var rangeOfTimeSinceMuskApparition = 1 - muskApparitionTime;
        var normalizedProgression =
            (_currentDataLoopTime - muskApparitionTime) / rangeOfTimeSinceMuskApparition;

        currentSpeed = Mathf.Lerp(config.midBaseSpeed, config.endingBaseSpeed, normalizedProgression);
    }

    private void EaseInSlowingVisual()
    {
        var returningProgress = _hatchedDataVisuals.Count / (float)_allGpData.Count;
        currentSpeed = config.endingBaseSpeed * returningProgress;
        UpdateVisualPositions();
    }

    private void ReturnSomeVisualsToPool()
    {
        var rnd = new System.Random();
        var tmpQueue = new Queue<DataVisual>(_hatchedDataVisuals.Count);

        var currentCount = _hatchedDataVisuals.Count;
        var disappearEverything = currentCount <= 300;
        var fasterDisappear = currentCount <= 10000;

        var removeCoef = fasterDisappear ? 0.05f : config.disappearingRate;

        for (var i = 0; i < currentCount; i++)
        {
            var dataVisual = _hatchedDataVisuals.Dequeue();
            if (!disappearEverything && (rnd.NextDouble() > removeCoef))
            {
                tmpQueue.Enqueue(dataVisual);
            }
            else
            {
                if (dataVisual.IsAccentVisual)
                {
                    accentVisualPool.Return(dataVisual.Visual);
                }
                else
                {
                    dataVisual.RenderRef.material.SetFloat("_Clock", 0);
                    visualPool.Return(dataVisual.Visual);
                }

                dataVisual.Data = null;
            }
        }

        _hatchedDataVisuals = tmpQueue;
    }

    private void UpdateVisualPositions()
    {
        foreach (var dataVisual in _hatchedDataVisuals)
        {
            UpdatePositionFromContext(dataVisual);
        }
    }

    private void AssignCirclePosY(DataVisual dataVisual)
    {
        if (dataVisual.CircleY < 0)
        {
            return;
        }

        var maxCircleRadius = (float)Configuration.GetConfig().maxCircleDiam / 4;
        var minCircleRadius = Configuration.GetConfig().minCircleDiam;

        float circlePosY;
        if (!dataVisual.Data.IsFake)
        {
            var normalizedProgressionForTypeofT = dataVisual.Data.T / futureStartingTime;
            circlePosY = Mathf.Lerp(maxCircleRadius, minCircleRadius, normalizedProgressionForTypeofT);
        }
        else
        {
            var apparitionOnFutureProgressScale =
                (dataVisual.Data.T - futureStartingTime) / (1 - futureStartingTime);
            circlePosY = Mathf.Lerp(0, maxCircleRadius, apparitionOnFutureProgressScale);
        }

        circlePosY += 10 * ((float)dataVisual.Random - 0.5f);

        dataVisual.CircleY = circlePosY;
        dataVisual.NormalizedCircleY = circlePosY / maxCircleRadius;
    }

    private void UpdatePositionFromContext(DataVisual dataVisual)
    {
        AssignCirclePosY(dataVisual);

        var deltaTime = Time.deltaTime;

        var originalX = dataVisual.Data.X;
        var originalY = dataVisual.Data.Y;
        var originalT = dataVisual.Data.T;
        var lastCirclePositionX = dataVisual.LastCircleX;

        var timeElapsed = Time.time - _lastLoopStart;


        // CirclePosition calculations
        var tmpDataTimeAccelerator = dataVisual.IsAccentVisual ? config.fasterMuskCoef : 1;
        var circleCenterSlower = 0.3f + 0.7f * dataVisual.NormalizedCircleY;
        var newCircleX = lastCirclePositionX +
                         deltaTime * currentSpeed * tmpDataTimeAccelerator * circleCenterSlower
            ;
        dataVisual.LastCircleX = newCircleX;

        var x = Mathf.Cos(newCircleX) * dataVisual.CircleY;
        var y = Mathf.Sin(newCircleX) * dataVisual.CircleY;

        dataVisual.Visual.transform.localPosition = visualPosition.localPosition + new Vector3(x, y, 0);
        dataVisual.Visual.transform.localScale = currentCircleScale;

        //give the clock value to the shader of the material of the object
        var currentFutureprogress = (_currentDataLoopTime - futureStartingTime) / (1 - futureStartingTime);
        if (currentFutureprogress > 0f && !dataVisual.IsAccentVisual)
        {
            dataVisual.RenderRef.material.SetFloat("_Clock", currentFutureprogress);
        }
    }

    private void SendOscBangs()
    {
        if (_currentDataLoopTime < 0.99f)
        {
            pdConnector.SendOscMessage("/data_clock", _currentDataLoopTime);
        }
        else if (_currentState == AppStates.COLLAPSE)
        {
            pdConnector.SendOscMessage("/data_clock", 0.99f);
        }
        else if (_currentState == AppStates.END)
        {
            pdConnector.SendOscMessage("/data_clock", 1f);
        }
    }

    private void LogCounts()
    {
        logger.Log($"NB original objects: {_allOrigGpData.Count()}");
        logger.Log($"NB objects: {_allGpData.Count()}");
        logger.Log(
            $"NB Real Musk data: {_allGpData.Count(gpData => !gpData.IsFake && gpData.ObjectType == ElsetObjectType.MUSK)}");
        logger.Log(
            $"NB FAKE Musk data: {_allGpData.Count(gpData => gpData.IsFake && gpData.ObjectType == ElsetObjectType.MUSK)}");
        logger.Log($"NB fake data: {_allGpData.Count(gpData => gpData.IsFake)}");
    }
}