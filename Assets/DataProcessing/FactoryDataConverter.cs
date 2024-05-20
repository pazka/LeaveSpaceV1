using System;
using System.Collections.Generic;
using DataProcessing.AllGP;
using DataProcessing.Generic;

public static class FactoryDataConverter
{
    public enum AvailableDataManagerTypes
    {
        ALLGP
    }

    private static readonly Dictionary<AvailableDataManagerTypes, IDataConverter> instances =
        new Dictionary<AvailableDataManagerTypes, IDataConverter>();

    public static IDataConverter GetInstance(AvailableDataManagerTypes dataManagerType)
    {
        switch (dataManagerType)
        {
            case AvailableDataManagerTypes.ALLGP:
                if (!instances.ContainsKey(dataManagerType))
                    instances.Add(dataManagerType, new GPDataConverter());

                return instances[dataManagerType];

            default:
                throw new Exception("DataManagerType isn't implemented : " + dataManagerType);
        }
    }
}