using System;
using System.Collections.Generic;
using System.Data;
using DataProcessing.AllGP;
using DataProcessing.Generic;

public static class FactoryDataReader
{
    public enum AvailableDataReaderTypes
    {
        ALLGP
    }

    private static readonly Dictionary<AvailableDataReaderTypes, IDataReader<ITimedData>> instances =
        new Dictionary<AvailableDataReaderTypes, IDataReader<ITimedData>>();

    public static IDataReader<ITimedData> GetInstance(AvailableDataReaderTypes dataReaderType)
    {
        switch (dataReaderType)
        {
            case AvailableDataReaderTypes.ALLGP:
                if (!instances.ContainsKey(dataReaderType))
                    instances.Add(dataReaderType, new AllGPDataReader() as IDataReader<ITimedData>);

                return instances[dataReaderType];

            default:
                throw new Exception("DataReaderType isn't implemented : " + dataReaderType);
        }
    }
}