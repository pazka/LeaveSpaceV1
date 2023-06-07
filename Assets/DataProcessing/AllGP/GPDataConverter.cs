using System;
using System.Collections.Generic;
using DataProcessing.Generic;

namespace DataProcessing.AllGP
{
    public class AllGPDataConverter : DataConverter
    {
        AllGPDataReader reader;
        
        public override void Init(int screenBoundX, int screenBoundY)
        {
            reader = FactoryDataReader.GetInstance(FactoryDataReader.AvailableDataReaderTypes.ALLGP) as AllGPDataReader;
        }

        public override void Clean()
        {
            throw new NotImplementedException();
        }

        public override IData GetNextData()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IData> GetAllData()
        {
            
        }

        public override IData[] GetDataBounds()
        {
            throw new NotImplementedException();
        }

        public override IDataReader<ITimedData> GetDataReader()
        {
            throw new NotImplementedException();
        }
    }
}
