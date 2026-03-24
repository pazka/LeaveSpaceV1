using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace Tools
{
    public static class Config
    {
        private static Dictionary<string, JToken> _data;
        private static Dictionary<(string, Type), object> _typedCache;
        private static bool _loaded;

        private static void EnsureLoaded()
        {
            if (_loaded) return;

            _loaded = true;
            _typedCache = new Dictionary<(string, Type), object>();
            _data = new Dictionary<string, JToken>();

            var path = Path.Combine(Application.streamingAssetsPath, "config.json");


                var json = File.ReadAllText(path);
                var obj = JObject.Parse(json);

                foreach (var property in obj.Properties())
                {
                    _data[property.Name] = property.Value;
                }
        }

        public static T Get<T>(string key, T defaultValue = default)
        {
            EnsureLoaded();

            var cacheKey = (key, typeof(T));
            if (_typedCache.TryGetValue(cacheKey, out var cached))
                return (T)cached;

            if (!_data.TryGetValue(key, out var token))
                return defaultValue;

            try
            {
                var result = token.ToObject<T>();
                _typedCache[cacheKey] = result;
                return result;
            }
            catch (Exception e)
            {
                return defaultValue;
            }
        }

        public static bool HasKey(string key)
        {
            EnsureLoaded();
            return _data.ContainsKey(key);
        }

        public static string[] GetAllKeys()
        {
            EnsureLoaded();
            var keys = new string[_data.Count];
            _data.Keys.CopyTo(keys, 0);
            return keys;
        }

        public static void Reload()
        {
            _loaded = false;
            _typedCache?.Clear();
            EnsureLoaded();
        }
    }
}
