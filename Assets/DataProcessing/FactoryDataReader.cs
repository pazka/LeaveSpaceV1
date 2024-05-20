using System;
using System.Collections.Generic;
using DataProcessing.AllGP;
using DataProcessing.Generic;

public static class FactoryDataReader
{
    public enum AvailableDataReaderTypes
    {
        ALLGP
    }

    private static readonly Dictionary<AvailableDataReaderTypes, IDataReader> instances =
        new Dictionary<AvailableDataReaderTypes, IDataReader>();

    public static IDataReader GetInstance(AvailableDataReaderTypes dataReaderType)
    {
        switch (dataReaderType)
        {
            case AvailableDataReaderTypes.ALLGP:
                if (!instances.ContainsKey(dataReaderType))
                    instances.Add(dataReaderType, new GPDataReader());

                return instances[dataReaderType];

            default:
                throw new Exception("DataReaderType isn't implemented : " + dataReaderType);
        }
    }
}