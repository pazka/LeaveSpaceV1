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
    private float currentSpeed;
    private float muskApparitionTime;
    private float endStartingTime;
    private float futureStartingTime;
    private float endDuration;
    private float loopDuration = 30;
    private float contemplationDelay = 5;
    private float disappearingRate = 0.08f;
    private float startingBaseSpeed = 0.01f;
    private float endingBaseSpeed = 0.1f;
    private float fasterMuskCoef = 3f;
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
        SetValueFromConfig();
        currentSpeed = startingBaseSpeed;
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

    private void SetValueFromConfig()
    {
        if (Configuration.GetConfig().isDev)
            return;

        loopDuration = Configuration.GetConfig().loopDuration;
        contemplationDelay = Configuration.GetConfig().contemplationDelay;
        startingBaseSpeed = Configuration.GetConfig().startingBaseSpeed;
        endingBaseSpeed = Configuration.GetConfig().endingBaseSpeed;
        fasterMuskCoef = Configuration.GetConfig().fasterMuskCoef;
        startingBaseSpeed = Configuration.GetConfig().startingBaseSpeed;
        disappearingRate = Configuration.GetConfig().disappearingRate;
        endDuration = Configuration.GetConfig().endDuration;
    }

    private void LoadDataVisuals()
    {
        foreach (var data in _allGpData.Select(t => t as GPData))
        {
            _notYetHatchedDataVisuals.Enqueue(data.IsFake
                ? new DataVisual(data, accentVisualPool.GetOne())
                : new DataVisual(data, visualPool.GetOne()));
        }
    }

    private void SetupTimeCodes()
    {
        var firstMuskData = _allGpData.FirstOrDefault(d => d.ObjectType == ElsetObjectType.MUSK);
        var firstFutureData = _allGpData.FirstOrDefault(d => d.IsFake);

        muskApparitionTime = firstMuskData?.T ?? 0;
        futureStartingTime = firstFutureData?.T ?? 0;
    }

    private void CheckForStateChange()
    {
        var elapsedTime = Time.time - _lastLoopStart;
        if (_currentState == AppStates.NORMAL && _currentDataLoopTime >= muskApparitionTime)
        {
            _currentState = AppStates.NORMAL_MUSK;
            logger.Log("Musk appeared !");
        }
        else if (_currentState == AppStates.NORMAL_MUSK && _currentDataLoopTime >= futureStartingTime)
        {
            _currentState = AppStates.FUTURE;
            logger.Log("Future");
        }
        else if (_currentState == AppStates.FUTURE &&
                 _notYetHatchedDataVisuals.Count == 0 &&
                 elapsedTime >= loopDuration)
        {
            _currentState = AppStates.CONTEMPLATION;
            logger.Log("Contemplation");
        }
        else if (elapsedTime >= loopDuration + contemplationDelay &&
                 _currentState == AppStates.CONTEMPLATION)
        {
            _currentState = AppStates.COLLAPSE;
            logger.Log("Reset");
        }
        else if (_currentState == AppStates.COLLAPSE && _hatchedDataVisuals.Count == 0)
        {
            endStartingTime = Time.time;
            _currentState = AppStates.END;
            logger.Log("End");
        }
        else if (_currentState == AppStates.END && Time.time - endStartingTime > endDuration)
        {
            InitLoop();
            _currentState = AppStates.NORMAL;
            logger.Log("Normal");
        }
    }

    private void InitLoop()
    {
        currentSpeed = startingBaseSpeed;
        _lastLoopStart = Time.time;
        _allGpData = (_extrapolator.RetrieveExtrapolation() as IEnumerable<GPData>).ToList();
        LogCounts();
        _hatchedDataVisuals = new Queue<DataVisual>(_allGpData.Count);
        _notYetHatchedDataVisuals = new Queue<DataVisual>(_allGpData.Count);
        SetupTimeCodes();
        UpdateCurrentIterationTime();
        LoadDataVisuals();
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
        
        UpdateCurrentIterationTime();

        if (_currentState == AppStates.NORMAL)
        {
            ForwardVisual();
            UpdateVisualPositions();
        }
        else if (_currentState == AppStates.FUTURE || _currentState == AppStates.NORMAL_MUSK)
        {
            AccelerateVisual();
            ForwardVisual();
            UpdateVisualPositions();
        }
        else if (_currentState == AppStates.CONTEMPLATION)
        {
            UpdateVisualPositions();
        }
        else if (_currentState == AppStates.COLLAPSE)
        {
            EaseInSlowingVisual();
            ReturnSomeVisualsToPool();
        }

        SendOscBangs();
        UpdateCurrentDataProgress();
    }

    private void EaseInSlowingVisual()
    {
        currentSpeed *= 0.95f;
        if (currentSpeed < endingBaseSpeed / 15)
            currentSpeed = 0;

        if (currentSpeed != 0)
            UpdateVisualPositions();
    }

    private void UpdateCurrentIterationTime()
    {
        _currentDataLoopTime = (Time.time - _lastLoopStart) / (loopDuration);
        _currentFullLoopTime = (Time.time - _lastLoopStart) / (loopDuration + contemplationDelay);
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

    private void AccelerateVisual()
    {
        var rangeOfTimeSinceMuskApparition = 1 - muskApparitionTime;
        var normalizedProgression =
            (_currentDataLoopTime - muskApparitionTime) / rangeOfTimeSinceMuskApparition;

        currentSpeed = Mathf.Lerp(startingBaseSpeed, endingBaseSpeed, normalizedProgression);
    }

    private void ReturnSomeVisualsToPool()
    {
        var rnd = new System.Random();
        var tmpQueue = new Queue<DataVisual>(_hatchedDataVisuals.Count);

        var currentCount = _hatchedDataVisuals.Count;
        var disappearEverything = currentCount <= 300;

        for (var i = 0; i < currentCount; i++)
        {
            var dataVisual = _hatchedDataVisuals.Dequeue();
            if (!disappearEverything && (rnd.NextDouble() > disappearingRate))
            {
                tmpQueue.Enqueue(dataVisual);
            }
            else
            {
                dataVisual.Visual.GetComponent<Renderer>().material.SetFloat("_Clock", 0);
                if (dataVisual.Data.IsFake)
                {
                    accentVisualPool.Return(dataVisual.Visual);
                }
                else
                {
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

    private void UpdatePositionFromContext(DataVisual dataVisual)
    {
        var originalX = dataVisual.Data.X;
        var originalY = dataVisual.Data.Y;
        var originalT = dataVisual.Data.T;
        var timeElapsed = Time.time - _lastLoopStart;

        var maxCircleRadius = (float)Configuration.GetConfig().maxCircleDiam / 4;
        var minCircleRadius = Configuration.GetConfig().minCircleDiam / 4;

        float circleRadius;
        var currentFutureprogression = (_currentDataLoopTime - futureStartingTime) / (1 - futureStartingTime);

        if (!dataVisual.Data.IsFake)
        {
            var normalizedProgressionForTypeofT = originalT / futureStartingTime;
            circleRadius = Mathf.Lerp(maxCircleRadius, minCircleRadius, normalizedProgressionForTypeofT);
        }
        else
        {
            var apparitionOnFutureProgressScale =
                (originalT - futureStartingTime) / (1 - futureStartingTime);
            circleRadius = Mathf.Lerp(0, maxCircleRadius, apparitionOnFutureProgressScale);
        }

        circleRadius += 10 * ((float)dataVisual.Random - 0.5f);
        var normalizedCircleRadius = circleRadius / maxCircleRadius;

        var tmpDataTimeAccelerator = dataVisual.Data.ObjectType == ElsetObjectType.MUSK ? fasterMuskCoef : 1;
        var timeOffset = (dataVisual.Data.T) * 100000;
        var timeSpeed = currentSpeed * tmpDataTimeAccelerator;
        var timePosition = timeElapsed * timeSpeed * normalizedCircleRadius + timeOffset;

        var x = Mathf.Cos(timePosition) * circleRadius;
        var y = Mathf.Sin(timePosition) * circleRadius;

        if (float.IsNaN(x) || float.IsNaN(y))
            Debug.LogError("NAN");

        dataVisual.Visual.transform.localPosition = visualPosition.localPosition + new Vector3(x, y, 0);
        //give the clock value to the shader of the material of the object
        if (currentFutureprogression > 0f)
        {
            dataVisual.Visual.GetComponent<Renderer>().material.SetFloat("_Clock", currentFutureprogression);
        }
    }

    private void SendOscBangs()
    {
        pdConnector.SendOscMessage("/data_clock", _currentDataProgress);
    }

    private void LogCounts()
    {
        logger.Log($"Amount of objects: {_allGpData.Count()}");
        logger.Log(
            $"Amount of Real Musk data: {_allGpData.Count(gpData => !gpData.IsFake && gpData.ObjectType == ElsetObjectType.MUSK)}");
        logger.Log(
            $"Amount of FAKE Musk data: {_allGpData.Count(gpData => gpData.IsFake && gpData.ObjectType == ElsetObjectType.MUSK)}");
        logger.Log($"Amount of fake data: {_allGpData.Count(gpData => gpData.IsFake)}");
    }
}