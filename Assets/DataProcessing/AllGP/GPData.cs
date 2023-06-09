using DataProcessing.Generic;
using UnityEngine;

namespace DataProcessing.AllGP
{
    public class AllGPData : Generic.TimedData
    {
        public AllGpJsonData RawJson { get; private set; }

        public AllGPData(AllGpJsonData rawJson, float x, float y, float t) : base(JsonUtility.ToJson(rawJson), x, y, t)
        {
            this.RawJson = rawJson;
        }
    }
}