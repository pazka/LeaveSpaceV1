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
        
        public TimedData(TimedData data) : base(data)
        {
            this.RawT = data.RawT;
            this.T = data.T;
        }

        public TimedData(string raw, float x, float y, float t) : base(raw, x, y)
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