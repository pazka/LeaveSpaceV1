using DataProcessing.AllGP;
using DataProcessing.Generic;

namespace Visuals
{
    public class AllGpDataEventHatcher : EventHatcher<DataVisual>
    {
        protected override bool DecideIfReady(DataVisual data, dynamic timePassed)
        {
            return data.Data.T.CompareTo((float)timePassed) <= 0;
        }

        protected override DataVisual ExecuteData(DataVisual dataToTrigger)
        {   
            dataToTrigger.Visual.SetActive(true);
            return dataToTrigger;
        }
    }
}