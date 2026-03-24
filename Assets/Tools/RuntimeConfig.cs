using UnityEngine;

namespace Tools
{
    public static class RuntimeConfig
    {
        private static JsonConfiguration _cache;

        public static JsonConfiguration Get()
        {
            if (_cache != null)
            {
                return _cache;
            }

            var isDev = Config.Get("isDev", false);
            if (Debug.isDebugBuild)
            {
                isDev = true;
            }

            _cache = new JsonConfiguration(
                Config.Get("outPort", 10300),
                Config.Get("outIp", "127.0.0.1"),
                Config.Get("inPort", 10301),
                Config.Get("offsetX", 0),
                Config.Get("offsetY", 0),
                Config.Get("scaleX", 1f),
                Config.Get("scaleY", 1f),
                Config.Get("loopDuration", 720f),
                Config.Get("contemplationDelay", 1f),
                Config.Get("endDuration", 20f),
                Config.Get("midBaseSpeed", 0.175f),
                Config.Get("startBaseSpeed", 0.005f),
                Config.Get("fasterMuskCoef", 3f),
                Config.Get("disappearingRate", 0.008f),
                Config.Get("endingBaseSpeed", 10f),
                Config.Get("minCircleDiam", 70f),
                Config.Get("maxCircleDiam", 2160f),
                Config.Get("startStarSize", 12f),
                Config.Get("muskStarSizeCoef", 0.9f),
                Config.Get("midStarSize", 3f),
                isDev,
                Config.Get("baseColor", new[] { 1f, 1f, 1f }),
                Config.Get("accentColor", new[] { 1f, 1f, 1f })
            );

            return _cache;
        }

        public static void Reload()
        {
            _cache = null;
            Config.Reload();
        }
    }
}
