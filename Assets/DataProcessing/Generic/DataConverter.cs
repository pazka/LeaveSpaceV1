using System.Collections.Generic;
using System.Data;

namespace DataProcessing.Generic
{
    public  abstract class DataConverter<T> : IDataConverter<T> where T : ITimedData
    {
        public abstract void Init(int screenBoundX, int screenBoundY);
        public abstract void Clean();
        public abstract T GetNextData();
        public abstract IEnumerable<T> GetAllData();
        public abstract (T Min,T Max) GetDataBounds();
        public abstract IDataReader<T> GetDataReader();
    }
}
