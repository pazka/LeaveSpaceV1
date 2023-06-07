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

    private static readonly Dictionary<AvailableDataReaderTypes, IDataReader<TimedData>> instances =
        new Dictionary<AvailableDataReaderTypes, IDataReader<TimedData>>();

    public static IDataReader<TimedData> GetInstance(AvailableDataReaderTypes dataReaderType)
    {
        switch (dataReaderType)
        {
            case AvailableDataReaderTypes.ALLGP:
                if (!instances.ContainsKey(dataReaderType))
                    instances.Add(dataReaderType, new AllGPDataReader());

                return instances[dataReaderType];

            default:
                throw new Exception("DataReaderType isn't implemented : " + dataReaderType);
        }
    }
}