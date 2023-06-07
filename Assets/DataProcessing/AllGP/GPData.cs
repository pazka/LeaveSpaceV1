using DataProcessing.Generic;
using UnityEngine;

namespace DataProcessing.AllGP
{
    public class AllGPData : Generic.TimedData
    {
        public AllGPData(string raw, float x, float y, float t) : base(raw, x, y, t)
        {
        }
    }
}