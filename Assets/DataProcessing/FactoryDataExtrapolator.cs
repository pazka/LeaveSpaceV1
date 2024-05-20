using System;
using System.Collections.Generic;
using DataProcessing.AllGP;
using DataProcessing.Generic;

namespace DataProcessing
{
    public class FactoryDataExtrapolator
    {
        public enum AvailableDataExtrapolatorTypes
        {
            ALLGP
        }

        private static readonly Dictionary<AvailableDataExtrapolatorTypes, IDataExtrapolator> instances =
            new Dictionary<AvailableDataExtrapolatorTypes, IDataExtrapolator>();

        public static IDataExtrapolator GetInstance(AvailableDataExtrapolatorTypes dataExtrapolatorType)
        {
            switch (dataExtrapolatorType)
            {
                case AvailableDataExtrapolatorTypes.ALLGP:
                    if (!instances.ContainsKey(dataExtrapolatorType))
                        instances.Add(dataExtrapolatorType, new GPDataExtrapolatorBias());

                    return instances[dataExtrapolatorType];

                default:
                    throw new Exception("DataExtrapolatorType isn't implemented : " + dataExtrapolatorType);
            }
        }
    }
}