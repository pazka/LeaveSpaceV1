using System.Collections.Generic;
using System.Data;

namespace DataProcessing.Generic
{
    public abstract class DataConverter : IDataConverter
    {
        public abstract void Init(int screenBoundX, int screenBoundY);
        public abstract void Clean();
        public abstract TimedData GetNextData();
        public abstract IEnumerable<TimedData> GetAllData();
        public abstract (TimedData Min, TimedData Max) GetDataBounds();
        public abstract IDataReader GetDataReader();
    }
}