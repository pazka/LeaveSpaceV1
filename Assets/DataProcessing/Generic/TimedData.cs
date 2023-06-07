using UnityEngine;

namespace DataProcessing.Generic
{
    public interface ITimedData : IData
    {   
        void SetT(float t);
        float GetT();
    }
    
    public class TimedData : Data, ITimedData
    {
        public float RawT { get; private set;}
        public float T { get; protected set;}

        public TimedData(string raw, float x, float y) : base(raw, x, y)
        {
        }

        public TimedData(string raw, float x, float y, float t) : this(raw, x, y)
        {
            this.RawT = t;
            this.T = t;
        }

        public void SetT(float t)
        {
            this.T = t;
        }

        public float GetT()
        {
            return this.T;
        }
    }
}