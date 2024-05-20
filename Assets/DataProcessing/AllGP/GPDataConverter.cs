using System;
using System.Collections.Generic;
using DataProcessing.Generic;
using Tools;
using UnityEngine;

namespace DataProcessing.AllGP
{
    public class GPDataConverter : DataConverter
    {
        private IDataReader _reader;
        private List<TimedData> _allDataRead;
        private int _currentDataIndex = 0;

        private List<TimedData> _allDataConverted;
        private (TimedData Min, TimedData Max, TimedData Mean) _dataBounds;
        public (float X, float Y) ScreenBounds;

        public override void Init(int screenBoundX, int screenBoundY)
        {
            ScreenBounds = (screenBoundX, screenBoundY);
            _reader = FactoryDataReader.GetInstance(FactoryDataReader.AvailableDataReaderTypes.ALLGP);
            Clean();
        }

        public override void Clean()
        {
            _allDataConverted = new List<TimedData>();
            _allDataRead = new List<TimedData>();
            _currentDataIndex = 0;
            _dataBounds.Min = new TimedData("minbounds", float.MaxValue, float.MaxValue, float.MaxValue);
            _dataBounds.Max = new TimedData("maxbounds", float.MinValue, float.MinValue, float.MinValue);
            _dataBounds.Mean = new TimedData("meantotalbounds", 0, 0, 0);
            _reader.Clean();
            _reader.Init();
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

            _dataBounds.Mean = new TimedData(
                "meanbounds",
                _dataBounds.Mean.X / _allDataRead.Count,
                _dataBounds.Mean.Y / _allDataRead.Count,
                _dataBounds.Mean.T / _allDataRead.Count
            );

            _allDataRead.Sort((a, b) => -a.RawT.CompareTo(b.RawT));
        }

        public override TimedData GetNextData()
        {
            if (_currentDataIndex >= _allDataRead.Count)
                return null;

            if (_allDataRead[_currentDataIndex++] is not GPData data)
                throw new Exception("Data is not of type GPData");

            var position = SpaceTools.GetXYFromAllGpJsonDataVisu(data.RawJson);

            //scale the position from 0 to 1
            float tmpX = ScaleTo1(position.x, _dataBounds.Min.X, _dataBounds.Max.X);
            float tmpY = ScaleTo1(position.y, _dataBounds.Min.Y, _dataBounds.Max.Y);
            float scaledMeanX = ScaleTo1(_dataBounds.Mean.X, _dataBounds.Min.X, _dataBounds.Max.X);
            float scaledMeanY = ScaleTo1(_dataBounds.Mean.Y, _dataBounds.Min.Y, _dataBounds.Max.Y);
            float tmpDist = Mathf.Sqrt(tmpX * tmpX + tmpY * tmpY) / Mathf.Sqrt(2);
            float avgDist = Mathf.Sqrt(scaledMeanX * scaledMeanX + scaledMeanY * scaledMeanY) / Mathf.Sqrt(2);

            float range = 0.005f;
            if (tmpDist < avgDist + range && tmpDist > avgDist - range)
            {
                float diff = (avgDist - tmpDist) / range;
                tmpX += (diff * 0.4f);
                tmpY += (diff * 0.4f);
            }

            data.SetX(tmpX * ScreenBounds.X);
            data.SetY(tmpY * ScreenBounds.Y);
            data.SetT(1-ScaleTo1(data.RawT, _dataBounds.Min.T, _dataBounds.Max.T));


            return data;
        }

        private static float ScaleTo1(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }

        public override IEnumerable<TimedData> GetAllData()
        {
            while (GetNextData() is { } data)
                yield return data;
        }

        private void RegisterBounds(TimedData data)
        {
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

            _dataBounds.Mean.SetX((_dataBounds.Mean.X + data.RawX));
            _dataBounds.Mean.SetY((_dataBounds.Mean.Y + data.RawY));
            _dataBounds.Mean.SetT((_dataBounds.Mean.T + data.RawT));
        }


        public override (TimedData Min, TimedData Max) GetDataBounds()
        {
            return (_dataBounds.Min, _dataBounds.Max);
        }

        public override IDataReader GetDataReader()
        {
            return _reader;
        }
    }
}