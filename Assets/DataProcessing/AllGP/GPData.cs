using DataProcessing.Generic;
using UnityEngine;

namespace DataProcessing.AllGP
{
    public class GPData : Generic.TimedData
    {
        public GpJsonData RawJson { get; private set; }
        public ElsetObjectType ObjectType;
        public bool IsFake;

        public GPData(GpJsonData rawJson, float x, float y, float t, ElsetObjectType elsetObjectType, bool isFake) : base(JsonUtility.ToJson(rawJson), x, y, t)
        {
            this.RawJson = rawJson;
            this.ObjectType = elsetObjectType;
            this.IsFake = isFake;
        }
    }
}