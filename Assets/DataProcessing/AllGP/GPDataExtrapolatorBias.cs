using System;
using System.Collections.Generic;
using System.Linq;
using DataProcessing.Generic;

namespace DataProcessing.AllGP
{
    public class GPDataExtrapolatorBias : DataExtrapolator
    {
        private IList<GPData> _gpDataToExtrapolate = new List<GPData>();
        private IList<GPData> _extrapolatedData = new List<GPData>();

        protected override IEnumerable<IData> GetConcreteExtrapolation()
        {
            if (_extrapolatedData != null)
                return _extrapolatedData;
            else
                throw new System.Exception("Extrapolation has not been executed yet");
        }

        protected override void SetConcreteDataToExtrapolate(IEnumerable<IData> dataToExtrapolate)
        {
            if (dataToExtrapolate is IEnumerable<GPData> gpDataToExtrapolate)
            {
                foreach (var gpData in gpDataToExtrapolate)
                {
                    _gpDataToExtrapolate.Add(gpData);
                }
            }
            else
                throw new System.Exception("Data to extrapolate is not of type GPData");
        }

        protected override void ExecuteExtrapolation(object parameters)
        {
            //T 0 = year 1957
            //T 100 = year 2023

            //slice the time between the first Musk launch and the last one into 100 segments and extrapolate the data to 2100
            var yearToTarget = 2100;
            var coefSlicingAmount = 100;
            var muskApparitionCoefs = new float[coefSlicingAmount];
            var xAvgForEachCoef = new float[coefSlicingAmount];
            var yAvgForEachCoef = new float[coefSlicingAmount];
            var aggravationAmount = 1.2f;
            var nbOfDebrisForEachFutureMusk = 5;

            //fill the coefs with amount of launch between the first lauch of a DATA.ElstObjectType.MUSK and the last one
            var firstTofMuskLaunchT = _gpDataToExtrapolate.First(gpData => gpData.ObjectType == ElsetObjectType.MUSK).T;
            var lastTofMuskLaunchT = _gpDataToExtrapolate.Last(gpData => gpData.ObjectType == ElsetObjectType.MUSK).T;
            var tRangeOfMuskApparition = lastTofMuskLaunchT - firstTofMuskLaunchT;

            _extrapolatedData = new List<GPData>();

            for (int i = 0; i < coefSlicingAmount; i++)
            {
                var data = _gpDataToExtrapolate.Where(gpData =>
                    gpData.T >= firstTofMuskLaunchT + i * tRangeOfMuskApparition / coefSlicingAmount &&
                    gpData.T < firstTofMuskLaunchT + (i + 1) * tRangeOfMuskApparition / coefSlicingAmount);

                var gpDatas = data as GPData[] ?? data.ToArray();
                muskApparitionCoefs[i] = gpDatas.Count();
                if (!gpDatas.Any())
                    continue;
                xAvgForEachCoef[i] = gpDatas.Average(gpData => gpData.X);
                yAvgForEachCoef[i] = gpDatas.Average(gpData => gpData.Y);
            }

            // Now find the T that corresponds to the year  yearToTarget
            var timeToTarget = (yearToTarget - 1957f) / (2023f - 1957f);
            var timeSlice = (timeToTarget - 1) / coefSlicingAmount;

            foreach (var gpData in _gpDataToExtrapolate)
            {
                _extrapolatedData.Add(gpData);
            }

            // add fake future data with the same pattern as the real apparitions
            for (int i = 0; i < coefSlicingAmount; i++)
            {
                var amountToCreate = (int)(muskApparitionCoefs[i]);
                var timeSliceForEachAmount = timeSlice / amountToCreate;

                //add musk in a linear way between for each time slice
                for (int j = 0; j < amountToCreate; j++)
                {
                    var newX = xAvgForEachCoef[i] + j;
                    var newY = yAvgForEachCoef[i] + j;
                    var newT = 1 + i * timeSlice + j * timeSliceForEachAmount * j;
                    var newGPData = new GPData(null, newX, newY, newT, ElsetObjectType.MUSK, true);
                    _extrapolatedData.Add(newGPData);

                    //add debris for each musks
                    for (int k = 0; k < nbOfDebrisForEachFutureMusk; k++)
                    {
                        var random = new System.Random();
                        var amountDeviationX = 10 * (float)(random.NextDouble() - 0.5f);
                        var amountDeviationY = 10 * (float)(random.NextDouble() - 0.5f);
                        
                        var newDebrisT = newT + k * timeSliceForEachAmount;
                        var newDebris = new GPData(null, newX - amountDeviationX, newY - amountDeviationY, newDebrisT, ElsetObjectType.DEBRIS_OTHER,
                            true);
                        _extrapolatedData.Add(newDebris);
                    }
                }
            }

            //normalize and set
            NormalizeDataT();
        }

        private void NormalizeDataT()
        {
            //Data T will have T from 0 to 1, we want them to scale to that they go from 0 to 1
            float minT = float.MaxValue;
            float maxT = float.MinValue;
            foreach (var gpData in _extrapolatedData)
            {
                if (gpData.T < minT)
                    minT = gpData.T;
                if (gpData.T > maxT)
                    maxT = gpData.T;
            }

            float tRange = maxT - minT;
            foreach (var gpData in _extrapolatedData)
            {
                gpData.SetT((gpData.T - minT) / tRange);
            }
        }
    }
}