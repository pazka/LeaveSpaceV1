using System.IO;
using UnityEngine;

namespace Tools
{
    public class JsonConfiguration
    {
        public int outPort;
        public string outIp;
        public int inPort;
        public int offsetX;
        public int offsetY;
        public float scaleX;
        public float scaleY;
        public float loopDuration;
        public float endDuration;
        public float contemplationDelay;
        public float startBaseSpeed;
        public float midBaseSpeed;
        public float endingBaseSpeed;
        public float fasterMuskCoef;
        public float disappearingRate;
        public float minCircleDiam;
        public float muskStarSizeCoef;
        public float startStarSize;
        public float midStarSize;
        public float maxCircleDiam;
        public bool isDev;
        public float[] baseColor;
        public float[] accentColor;

        public JsonConfiguration(
            int outPort,
            string outIp,
            int inPort,
            int offsetX,
            int offsetY,
            float scaleX,
            float scaleY,
            float loopDuration,
            float contemplationDelay,
            float endDuration,
            float midBaseSpeed,
            float startBaseSpeed,
            float fasterMuskCoef,
            float disappearingRate,
            float endingBaseSpeed,
            float minCircleDiam,
            float maxCircleDiam,
            float startStarSize,
            float muskStarSizeCoef,
            float midStarSize,
            bool isDev,
            float[] baseColor,
            float[] accentColor
        )
        {
            this.outPort = outPort;
            this.outIp = outIp;
            this.inPort = inPort;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.scaleX = scaleX;
            this.scaleY = scaleY;
            this.loopDuration = loopDuration;
            this.contemplationDelay = contemplationDelay;
            this.endDuration = endDuration;
            this.disappearingRate = disappearingRate;
            this.endingBaseSpeed = endingBaseSpeed;
            this.startBaseSpeed = startBaseSpeed;
            this.isDev = isDev;
            this.midBaseSpeed = midBaseSpeed;
            this.fasterMuskCoef = fasterMuskCoef;
            this.minCircleDiam = minCircleDiam;
            this.maxCircleDiam = maxCircleDiam;
            this.startStarSize = startStarSize;
            this.muskStarSizeCoef = muskStarSizeCoef;
            this.midStarSize = midStarSize;
            this.baseColor = baseColor;
            this.accentColor = accentColor;
        }
    }

    public static class Configuration
    {
        private static readonly string ConfigPath = Application.dataPath + "/StreamingAssets/config.json";
        public static string ConfigContent;
        private  static JsonConfiguration _config;

        public static JsonConfiguration GetConfig()
        {
            if(_config != null) return _config;
            
            //create config file if not exist
            if (ConfigContent == null) ConfigContent = File.ReadAllText(ConfigPath);
            _config = JsonUtility.FromJson<JsonConfiguration>(ConfigContent);

            if (Debug.isDebugBuild)
            {
                _config.isDev = true;
            }

            return _config;
        }
    }
}