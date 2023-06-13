using DataProcessing.Generic;
using Tools;
using UnityEngine;

namespace Visuals
{
    public class AllGpDataEventHatcher : EventHatcher<DataVisual>
    {
        private Color baseColor = Color.white;
        private Color accentColor = Color.red;
        private VisualPool visualPool;
        private VisualPool accentVisualPool;
        public AllGpDataEventHatcher(VisualPool visualPool,VisualPool accentVisualPool) : base()
        {
            this.visualPool = visualPool;
            this.accentVisualPool = accentVisualPool;
            visualPool.PreloadNObjects(35000);
            accentVisualPool.PreloadNObjects(35000);
            
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
            
            if (dataToTrigger.Data.RawJson.OBJECT_NAME.Contains("STARLINK"))
            {
                dataToTrigger.Visual = accentVisualPool.GetOne();
                dataToTrigger.Visual.GetComponent<Renderer>().material.SetColor("_Color",accentColor);
            }
            else
            {
                dataToTrigger.Visual = visualPool.GetOne();
                dataToTrigger.Visual.GetComponent<Renderer>().material.SetColor("_Color",baseColor);
            }
            
            dataToTrigger.Visual.transform.position = new Vector3(dataToTrigger.Data.X, dataToTrigger.Data.Y, 0);
            dataToTrigger.Visual.SetActive(true);
            
            return dataToTrigger;
        }
    }
}