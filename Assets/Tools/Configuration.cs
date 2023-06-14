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
        public float speedCoef;
        public float baseSpeed;
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
            float speedCoef,
            float baseSpeed,
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
            this.isDev = isDev;
            this.speedCoef = speedCoef;
            this.baseSpeed = baseSpeed;
            this.baseColor = baseColor;
            this.accentColor = accentColor;
        }
    }

    public static class Configuration
    {
        private static readonly string ConfigPath = Application.dataPath + "/StreamingAssets/config.json";
        public static string ConfigContent;

        public static JsonConfiguration GetConfig()
        {
            //create config file if not exist

            if (ConfigContent == null) ConfigContent = File.ReadAllText(ConfigPath);
            var config = JsonUtility.FromJson<JsonConfiguration>(ConfigContent);

            if (Debug.isDebugBuild)
            {
                config.isDev = true;
            }

            return config;
        }
    }
}