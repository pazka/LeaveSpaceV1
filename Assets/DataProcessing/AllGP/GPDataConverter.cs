using System;
using System.Collections.Generic;
using DataProcessing.Generic;
using UnityEngine;

namespace DataProcessing.AllGP
{
    public class AllGPDataConverter : DataConverter
    {
        private IDataReader _reader;
        private List<TimedData> _allDataRead;
        private int _currentDataIndex = 0;

        private List<TimedData> _allDataConverted;
        private (TimedData Min, TimedData Max) _dataBounds;
        private (float X, float Y) _screenBounds;

        public override void Init(int screenBoundX, int screenBoundY)
        {
            _reader = FactoryDataReader.GetInstance(FactoryDataReader.AvailableDataReaderTypes.ALLGP);
            _reader.Init();
            BrowseAllDataBeforehand();
        }

        public override void Clean()
        {
            _allDataRead = new List<TimedData>();
            _currentDataIndex = 0;
            _reader.Clean();
            BrowseAllDataBeforehand();
        }

        private void BrowseAllDataBeforehand()
        {
            while (_reader.GetData() is { } data)
            {
                _allDataRead.Add(data);
                RegisterBounds(data);
                _reader.GoToNextData();
            }
            
            _allDataRead.Sort((a, b) => a.RawT.CompareTo(b.RawT));
        }

        public override TimedData GetNextData()
        {
            if (_currentDataIndex >= _allDataRead.Count)
                return null;

            if (_allDataRead[_currentDataIndex++] is not AllGPData data)
                throw new Exception("Data is not of type AllGPData");

            var position = GetXYFromAllGpJsonData(data.RawJson);

            //scale the position from 0 to 1
            data.SetX(position.x / _dataBounds.Max.X * _screenBounds.X);
            data.SetY(position.y / _dataBounds.Max.Y * _screenBounds.Y);
            data.SetT(data.RawT / _dataBounds.Max.T);

            return data;
        }

        public override IEnumerable<TimedData> GetAllData()
        {
            while (GetNextData() is { } data)
                yield return data;
        }

        private void RegisterBounds(TimedData data)
        {
            _dataBounds.Min ??= data;

            _dataBounds.Max ??= data;

            if (_dataBounds.Min.X > data.RawX)
            {
                _dataBounds.Min.SetX(data.RawX);
            }

            if (_dataBounds.Min.Y > data.RawY)
            {
                _dataBounds.Min.SetY(data.RawY);
            }

            if (_dataBounds.Min.T > data.RawT)
            {
                _dataBounds.Min.SetT(data.RawT);
            }

            if (_dataBounds.Max.X < data.RawX)
            {
                _dataBounds.Max.SetX(data.RawX);
            }

            if (_dataBounds.Max.Y < data.RawY)
            {
                _dataBounds.Max.SetY(data.RawY);
            }

            if (_dataBounds.Max.T < data.RawT)
            {
                _dataBounds.Max.SetT(data.RawT);
            }
        }

        private static (float x, float y) GetXYFromAllGpJsonData(AllGpJsonData data)
        {
            var meanMotion = float.Parse(data.MEAN_MOTION);
            var eccentricity = float.Parse(data.ECCENTRICITY);
            var inclination = float.Parse(data.INCLINATION);
            var raOfAscNode = float.Parse(data.RA_OF_ASC_NODE);
            var argOfPericenter = float.Parse(data.ARG_OF_PERICENTER);
            var meanAnomaly = float.Parse(data.MEAN_ANOMALY);
            var semiMajorAxis = float.Parse(data.SEMIMAJOR_AXIS);
            var periapsis = float.Parse(data.PERIAPSIS);

            var x = (int)(semiMajorAxis * Mathf.Cos(meanAnomaly));
            var y = (int)(semiMajorAxis * Mathf.Sin(meanAnomaly));

            return (x * eccentricity, y * eccentricity);
        }

        public override (TimedData Min, TimedData Max) GetDataBounds()
        {
            return _dataBounds;
        }

        public override IDataReader GetDataReader()
        {
            return _reader;
        }
    }
}