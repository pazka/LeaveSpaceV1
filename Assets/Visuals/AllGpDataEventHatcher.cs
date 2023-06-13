using System;
using DataProcessing.AllGP;
using DataProcessing.Generic;
using UnityEngine;
using Random = System.Random;

namespace Visuals
{
    public class AllGpDataEventHatcher : EventHatcher<DataVisual>
    {
        Random rnd = new Random();
        protected override bool DecideIfReady(DataVisual data, dynamic timePassed)
        {
            return data.Data.T.CompareTo((float)timePassed) <= 0;
        }

        protected override DataVisual ExecuteData(DataVisual dataToTrigger)
        {   
            dataToTrigger.Visual.SetActive(true);
            int num = rnd.Next();
            if(num % 10 == 0)
            {
                dataToTrigger.Visual.GetComponent<Renderer>().material.SetColor("_Color",Color.red);
            }
            
            return dataToTrigger;
        }
    }
}