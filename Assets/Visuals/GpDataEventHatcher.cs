using DataProcessing.AllGP;
using DataProcessing.Generic;
using Tools;
using UnityEngine;

namespace Visuals
{
    public class GpDataEventHatcher : EventHatcher<DataVisual>
    {
        private Color baseColor = Color.white;
        private Color accentColor = Color.red;
        private VisualPool visualPool;
        private VisualPool accentVisualPool;
        public GpDataEventHatcher(VisualPool visualPool,VisualPool accentVisualPool) : base()
        {
            this.visualPool = visualPool;
            this.accentVisualPool = accentVisualPool;
            visualPool.PreloadNObjects(34000);
            accentVisualPool.PreloadNObjects(120000);
            
            float[] newBaseColor = Configuration.GetConfig().baseColor;
            float[] newAccentColor = Configuration.GetConfig().accentColor;
            baseColor = new Color(newBaseColor[0], newBaseColor[1], newBaseColor[2]);
            accentColor = new Color(newAccentColor[0], newAccentColor[1], newAccentColor[2]);
        }
        
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