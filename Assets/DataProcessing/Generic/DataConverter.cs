using System.Collections.Generic;
using System.Data;

namespace DataProcessing.Generic
{
    public  abstract class DataConverter : IDataConverter
    {
        public abstract void Init(int screenBoundX, int screenBoundY);
        public abstract void Clean();
        public abstract IData GetNextData();
        public abstract IEnumerable<IData> GetAllData();
        public abstract IData[] GetDataBounds();
        public abstract IDataReader<ITimedData> GetDataReader();
    }
}
