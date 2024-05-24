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
    RESET
}

public class MainScript : MonoBehaviour
{
    public VisualPool visualPool;
    public VisualPool accentVisualPool;
    public PureDataConnector pdConnector;
    public float loopDuration = 30;
    public float contemplationDelay = 5;
    public float disappearingRate = 0.05f;
    public float endingBaseSpeed = 0.001f;
    public float startingBaseSpeed = 0.0002f;
    public float dataTimeAccelerator = 0.00002f;
    public float fasterMuskCoef = 3f;
    private float _lastLoopStart = 0;
    public int[] visualDimension = new int[2] { 1920, 2160 };
    public Transform visualPosition = new RectTransform();
    public GameObject progressBar;
    public DebugVisual debugVisual;
    public ProgressBar progressBarScript;

    private System.Random _rnd = new System.Random();
    private GPDataConverter _converter;
    private List<GPData> _allOrigGpData;
    private List<GPData> _allGpData;
    private EventHatcher<DataVisual> _eventHatcher;
    private float currentSpeed = 0.7f;
    private float muskApparitionTime = 0f;
    private float resetStartingTime = 0f;
    private float futureStartingTime = 0f;
    private Queue<DataVisual> _hatchedDataVisuals;
    private Queue<DataVisual> _notYetHatchedDataVisuals;
    private IDataExtrapolator _extrapolator;
    private AppStates _currentState = AppStates.NORMAL;
    private float _currentDataApparitionTime = 0f;
    private float _currentFullLoopTime = 0f;
    private float _currentDataProgress = 0f;
    private ICollection<DataVisual> _latestHatchedDataVisuals;


    public void Start()
    {
        SetValueFromConfig();
        currentSpeed = startingBaseSpeed;
        if (Display.displays.Length > 1)
            Display.displays[1].Activate();

        _eventHatcher = new GpDataEventHatcher(visualPool, accentVisualPool);
        _hatchedDataVisuals = new Queue<DataVisual>();
        _notYetHatchedDataVisuals = new Queue<DataVisual>();
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
        dataTimeAccelerator = Configuration.GetConfig().dataTimeAccelerator;
        startingBaseSpeed = Configuration.GetConfig().startingBaseSpeed;
        endingBaseSpeed = Configuration.GetConfig().endingBaseSpeed;
        fasterMuskCoef = Configuration.GetConfig().fasterMuskCoef;
        startingBaseSpeed = Configuration.GetConfig().startingBaseSpeed;
        disappearingRate = Configuration.GetConfig().disappearingRate;
    }

    private void LoadDataVisuals()
    {
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

    public void SetupTimeCodes()
    {
        var firstMuskData = _allGpData.FirstOrDefault(d => d.ObjectType == ElsetObjectType.MUSK);
        var firstFutureData = _allGpData.FirstOrDefault(d => d.IsFake);

        muskApparitionTime = firstMuskData != null ? firstMuskData.T : 0;
        futureStartingTime = firstFutureData != null ? firstFutureData.T : 0;
    }

    public void CheckForStateChange()
    {
        DataVisual tmpData;
        if (_currentState == AppStates.NORMAL && _currentDataApparitionTime >= muskApparitionTime)
        {
            _currentState = AppStates.NORMAL_MUSK;
            debugVisual.AddTextToLog("Musk appeared !");
        }
        else if (_currentState == AppStates.NORMAL_MUSK && _currentDataApparitionTime >= futureStartingTime)
        {
            _currentState = AppStates.FUTURE;
            debugVisual.AddTextToLog("Future");
        }
        else if (_currentState == AppStates.FUTURE &&
                 _notYetHatchedDataVisuals.Count == 0 &&
                 (Time.time - _lastLoopStart) >= loopDuration)
        {
            _currentState = AppStates.CONTEMPLATION;
            debugVisual.AddTextToLog("Contemplation");
        }
        else if (Time.time - _lastLoopStart >= loopDuration + contemplationDelay &&
                 _currentState == AppStates.CONTEMPLATION)
        {
            resetStartingTime = Time.time;
            _currentState = AppStates.RESET;
            debugVisual.AddTextToLog("Reset");
        }
        else if (_currentState == AppStates.RESET && _hatchedDataVisuals.Count == 0)
        {
            _currentState = AppStates.NORMAL;
            InitLoop();
            debugVisual.AddTextToLog("Normal");
        }
    }

    private void InitLoop()
    {
        currentSpeed = startingBaseSpeed;
        _lastLoopStart = Time.time;
        _allGpData = (_extrapolator.RetrieveExtrapolation() as IEnumerable<GPData>).ToList();
        LogCounts();
        SetupTimeCodes();
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

        UpdateCurrentIterationTime();

        if (_eventHatcher == null)
            return;

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
        else if (_currentState == AppStates.RESET)
        {
            EaseInSlowingVisual();
            ReturnSomeVisualsToPool();
        }
        else
            throw new Exception("Unknown state");

        SendOscBangs();
        UpdateCurrentDataProgress();
        CheckForStateChange();
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
        _currentDataApparitionTime = (Time.time - _lastLoopStart) / (loopDuration);
        _currentFullLoopTime = (Time.time - _lastLoopStart) / (loopDuration + contemplationDelay);
        progressBarScript.SetT(_currentFullLoopTime);
    }

    private void UpdateCurrentDataProgress()
    {
        _currentDataProgress = 1f - ((float)_notYetHatchedDataVisuals.Count / (float)_allGpData.Count);
    }

    public void ForwardVisual()
    {
        _latestHatchedDataVisuals = _eventHatcher.HatchEvents(_notYetHatchedDataVisuals, _currentDataApparitionTime);

        foreach (var dataVisual in _latestHatchedDataVisuals)
        {
            _hatchedDataVisuals.Enqueue(dataVisual);
        }
    }

    public void AccelerateVisual()
    {
        var rangeOfTimeSinceMuskApparition = 1 - muskApparitionTime;
        var normalizedProgression =
            (_currentDataApparitionTime - muskApparitionTime) / rangeOfTimeSinceMuskApparition;

        currentSpeed = Mathf.Lerp(startingBaseSpeed, endingBaseSpeed, normalizedProgression);
    }

    void ReturnSomeVisualsToPool()
    {
        var nbOfObjectsToDisappear = Mathf.Clamp(
            _hatchedDataVisuals.Count,
            (float)_allGpData.Count / 500,
            _hatchedDataVisuals.Count * disappearingRate);

        var currentCount = _hatchedDataVisuals.Count;

        for (var i = 0; i < nbOfObjectsToDisappear || i < currentCount; i++)
        {
            var dataVisual = _hatchedDataVisuals.Dequeue();
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

    private void UpdateVisualPositions()
    {
        foreach (var dataVisual in _hatchedDataVisuals)
        {
            UpdatePositionFromContext(dataVisual);
        }
    }

    private void UpdatePositionFromContext(DataVisual dataVisual)
    {
        float timeElapsed = _currentFullLoopTime;
        float originalX = dataVisual.Data.X;
        float originalY = dataVisual.Data.Y;
        float originalT = dataVisual.Data.T;

        float maxCircleRadius = (float)Configuration.GetConfig().maxCircleDiam / 4;
        float minCircleRadius = Configuration.GetConfig().minCircleDiam / 4;

        float circleRadius;
        float noralizedProgressionForTypeofT;

        if (!dataVisual.Data.IsFake)
        {
            noralizedProgressionForTypeofT = originalT / futureStartingTime;
            circleRadius = Mathf.Lerp(maxCircleRadius, minCircleRadius, noralizedProgressionForTypeofT);
        }
        else
        {
            var normalizedProgression =
                (originalT - futureStartingTime) / (1 - futureStartingTime);
            circleRadius = Mathf.Lerp(0, maxCircleRadius, normalizedProgression);
        }

        circleRadius += 10 * ((float)_rnd.NextDouble() - 0.5f);

        float timeOffset = (dataVisual.Data.T) * 10000;
        var tmpDataTimeAccelerator = dataTimeAccelerator;

        if (dataVisual.Data.ObjectType == ElsetObjectType.MUSK)
        {
            tmpDataTimeAccelerator *= fasterMuskCoef;
        }

        float timeSpeed = currentSpeed + ((tmpDataTimeAccelerator) * dataVisual.Data.T);
        float timePosition = ((timeElapsed + timeOffset) + timeSpeed) * (1 - dataVisual.Data.T);

        float x = Mathf.Cos(timePosition) * circleRadius;
        float y = Mathf.Sin(timePosition) * circleRadius;

        if (float.IsNaN(x) || float.IsNaN(y))
            Debug.LogError("NAN");
        dataVisual.Visual.transform.localPosition = visualPosition.localPosition + new Vector3(x, y, 0);
    }

    private void SendOscBangs()
    {
        pdConnector.SendOscMessage("/data_clock", _currentDataProgress);
    }

    private void LogCounts()
    {
        debugVisual.AddTextToLog($"Amount of objects: {_allGpData.Count()}");
        debugVisual.AddTextToLog(
            $"Amount of Real Musk data: {_allGpData.Count(gpData => !gpData.IsFake && gpData.ObjectType == ElsetObjectType.MUSK)}");
        debugVisual.AddTextToLog(
            $"Amount of FAKE Musk data: {_allGpData.Count(gpData => gpData.IsFake && gpData.ObjectType == ElsetObjectType.MUSK)}");
        debugVisual.AddTextToLog($"Amount of fake data: {_allGpData.Count(gpData => gpData.IsFake)}");
    }
}