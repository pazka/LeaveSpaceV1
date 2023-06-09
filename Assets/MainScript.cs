using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    float LoopDuration = 300;

    void Start()
    {
        visualPool.PreloadNObjects(30000);

        _converter =
            FactoryDataConverter.GetInstance(FactoryDataConverter.AvailableDataManagerTypes
                .ALLGP) as AllGPDataConverter;
        if (_converter == null)
            throw new System.Exception("Converter is null");
        
        _converter.Init(1920, 1080);
        
        _eventHatcher = new AllGpDataEventHatcher();

        _allGpDataConverted = new List<AllGPData>();
        while (_converter.GetNextData() is { } data)
        {
            _allGpDataConverted.Add(data);
            var dataVisual = new DataVisual(data, visualPool.GetOne());
            dataVisual.Visual.SetActive(false);
            _notYetHatchedDataVisuals.Enqueue(dataVisual);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float timePassed = Time.time / LoopDuration;
        var hatched = _eventHatcher.HatchEvents(_notYetHatchedDataVisuals, timePassed);
        foreach (var dataVisual in hatched)
        {
            _hatchedDataVisuals.Enqueue(dataVisual);
        }
        
        foreach (var dataVisual in _hatchedDataVisuals)
        {
            var data = dataVisual.Data as AllGPData;
            float circleSize = float.Parse(dataVisual.Data.RawJson.PERIAPSIS);
            //transform the orignal x,y position of gameobject to go around in a circle given the time and the circleSize parmeter
            float x = Mathf.Cos(timePassed * 2 * Mathf.PI) * circleSize;
            float y = Mathf.Sin(timePassed * 2 * Mathf.PI) * circleSize;
            dataVisual.Visual.transform.position = new Vector3(x, y, 0);
        }
        
    }
}