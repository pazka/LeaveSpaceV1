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
        public float delayAfterFullLoop;
        public float startingBaseSpeed;
        public float endingBaseSpeed;
        public float speedToTCoef;
        public float fasterMuskCoef;
        public float disappearingRate;
        public float minStartingCircleSize;
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
            float delayAfterFullLoop,
            float startingBaseSpeed,
            float speedToTCoef,
            float fasterMuskCoef,
            float disappearingRate,
            float endingBaseSpeed,
            float minStartingCircleSize,
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
            this.delayAfterFullLoop = delayAfterFullLoop;
            this.disappearingRate = disappearingRate;
            this.speedToTCoef = speedToTCoef;
            this.endingBaseSpeed = endingBaseSpeed;
            this.isDev = isDev;
            this.startingBaseSpeed = startingBaseSpeed;
            this.fasterMuskCoef = fasterMuskCoef;
            this.minStartingCircleSize = minStartingCircleSize;
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