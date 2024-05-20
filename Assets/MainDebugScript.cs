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


public class MainDebugScript : MonoBehaviour
{
    public VisualPool visualPool;
    public VisualPool accentVisualPool;

    private GPDataConverter _converter;
    private IEnumerable<GPData> _allGpData;
    private IDataExtrapolator _extrapolator;
    private ICollection<DataVisual> _latestHatchedDataVisuals;


    public void Start()
    {
        if (Display.displays.Length > 1)
            Display.displays[1].Activate();
        else
            Debug.LogError("No second display detected");

        _allGpData = new List<GPData>();

        _converter =
            FactoryDataConverter.GetInstance(FactoryDataConverter.AvailableDataManagerTypes
                .ALLGP) as GPDataConverter;
        if (_converter == null)
            throw new Exception("Converter is null");

        _converter.Init(1920, 1080);

        _extrapolator =
            FactoryDataExtrapolator.GetInstance(FactoryDataExtrapolator.AvailableDataExtrapolatorTypes.ALLGP);

        LoadDataVisuals();
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
        _allGpData = _extrapolator.RetrieveExtrapolation() as IEnumerable<GPData>;

        //place visual in order from 0 to 1920 on the screen, each one on top of another

        for (var i = 0; i < _allGpData.Count(); i++)
        {
            var data = _allGpData.ElementAt(i) as GPData;
            GameObject visual;
            if (data.ObjectType == ElsetObjectType.MUSK)
                visual = accentVisualPool.GetOne();
            else
                visual = visualPool.GetOne();

            var posX = data.T * 1920;
            visual.transform.position = new Vector3(posX, data.Y, 0);
            visual.SetActive(true);
        }
    }

    public void Update()
    {
    }
}