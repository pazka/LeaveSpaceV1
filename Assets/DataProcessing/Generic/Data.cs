using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataProcessing.Generic
{
    public interface IData
    {
        void SetX(float x);
        void SetY(float y);
        float[] GetPosition();
        IData Clone();
    }

    public abstract class Data : IData
    {
        public string Raw;
        /// <summary>
        /// X given by the data at the reading of the raw data
        /// </summary>
        public float RawX { get; private set; }
        /// <summary>
        /// Y given by the data at the reading of the raw data
        /// </summary>
        public float RawY { get; private set; }
        /// <summary>
        /// Normalized X
        /// </summary>
        public float X { get; protected set; }
        /// <summary>
        /// Normalized Y
        /// </summary>
        public float Y { get; protected set; }

        public Data(Data data)
        {
            this.Raw = data.Raw;
            this.RawX = data.RawX;
            this.RawY = data.RawY;
            this.X = data.X;
            this.Y = data.Y;
        }
        
        public Data(float x, float y)
        {
            this.RawX = x;
            this.RawY = y;
            this.X = x;
            this.Y = y;
        }
        public Data(string raw,float x, float y) : this(x,y)
        {
            this.Raw = raw;
        }

        public virtual void SetX(float x)
        {
            this.X = x;
        }

        public virtual void SetY(float y)
        {
            this.Y = y;
        }

        public virtual float[] GetPosition()
        {
            return new float[]{RawX, RawY};
        }
        
        public IData Clone()
        {
            return (Data)this.MemberwiseClone();
        }
        
    }
}