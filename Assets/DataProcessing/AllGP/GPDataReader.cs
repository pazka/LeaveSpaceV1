using System;
using System.Collections.Generic;
using System.IO;
using DataProcessing.Generic;
using UnityEngine;

namespace DataProcessing.AllGP
{
    class AllGpJsonMetadata
    {
        public int Total;
        public int Limit;
        public int LimitOffset;
        public int ReturnedRows;
        public string RequestTime;
        public string DataSize;
    }

    class AllGpJsonData
    {
        public string CCSDS_OMM_VERS;
        public string COMMENT;
        public string CREATION_DATE;
        public string ORIGINATOR;
        public string OBJECT_NAME;
        public string OBJECT_ID;
        public string CENTER_NAME;
        public string REF_FRAME;
        public string TIME_SYSTEM;
        public string MEAN_ELEMENT_THEORY;
        public string EPOCH;
        public string MEAN_MOTION;
        public string ECCENTRICITY;
        public string INCLINATION;
        public string RA_OF_ASC_NODE;
        public string ARG_OF_PERICENTER;
        public string MEAN_ANOMALY;
        public string EPHEMERIS_TYPE;
        public string CLASSIFICATION_TYPE;
        public string NORAD_CAT_ID;
        public string ELEMENT_SET_NO;
        public string REV_AT_EPOCH;
        public string BSTAR;
        public string MEAN_MOTION_DOT;
        public string MEAN_MOTION_DDOT;
        public string SEMIMAJOR_AXIS;
        public string PERIOD;
        public string APOAPSIS;
        public string PERIAPSIS;
        public string OBJECT_TYPE;
        public string RCS_SIZE;
        public string COUNTRY_CODE;
        public string LAUNCH_DATE;
        public string SITE;
        public string DECAY_DATE;
        public string FILE;
        public string GP_ID;
        public string TLE_LINE0;
        public string TLE_LINE1;
        public string TLE_LINE2;
    }

    class AllGPJson
    {
        public AllGpJsonMetadata request_metadata;
        public List<AllGpJsonData> data;
    }

    public class AllGPDataReader : IDataReader<TimedData>
    {
        List<AllGpJsonData> allRawData;
        private int currentIndex;
        readonly string filePath = Application.dataPath + "/StreamingAssets/all_gp.json";

        public void Init()
        {
            allRawData = new List<AllGpJsonData>();
            currentIndex = 0;
        }

        private void ReadFile()
        {
            if (allRawData.Count > 0) return;
            var fileContent = File.ReadAllText(filePath);
            var json = JsonUtility.FromJson<AllGPJson>(fileContent);
            foreach (var raw in json.data)
            {
                allRawData.Add(raw);
            }
        }

        public void Clean()
        {
            allRawData = new List<AllGpJsonData>();
            currentIndex = 0;
        }

        public TimedData GetData()
        {
            if (allRawData.Count == 0) ReadFile();
            var currRawData = allRawData[currentIndex];
            var position = GetXYFromGeoSpatialData(
                currRawData.MEAN_MOTION,
                currRawData.ECCENTRICITY,
                currRawData.INCLINATION,
                currRawData.RA_OF_ASC_NODE,
                currRawData.ARG_OF_PERICENTER,
                currRawData.MEAN_ANOMALY,
                currRawData.SEMIMAJOR_AXIS,
                currRawData.PERIAPSIS
            );
            var newData = new TimedData(JsonUtility.ToJson(currRawData),
                    position.Item1,
                    position.Item2,
                    GetDateInSecondsFromEpochJulianDate(currRawData.EPOCH)
                );
            
            return newData;
        }

        private static float GetDateInSecondsFromEpochJulianDate(string epoch)
        {
            var epochDouble = double.Parse(epoch);
            var epochDate = new DateTime(1950, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            epochDate = epochDate.AddDays(epochDouble);
            return (float)epochDate.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        private static (float, float) GetXYFromGeoSpatialData(
            string MEAN_MOTION,
            string ECCENTRICITY,
            string INCLINATION,
            string RA_OF_ASC_NODE,
            string ARG_OF_PERICENTER,
            string MEAN_ANOMALY,
            string SEMIMAJOR_AXIS,
            string PERIAPSIS
        )
        {
            var meanMotion = float.Parse(MEAN_MOTION);
            var eccentricity = float.Parse(ECCENTRICITY);
            var inclination = float.Parse(INCLINATION);
            var raOfAscNode = float.Parse(RA_OF_ASC_NODE);
            var argOfPericenter = float.Parse(ARG_OF_PERICENTER);
            var meanAnomaly = float.Parse(MEAN_ANOMALY);
            var semiMajorAxis = float.Parse(SEMIMAJOR_AXIS);
            var periapsis = float.Parse(PERIAPSIS);

            var x = (int)(semiMajorAxis * Mathf.Cos(meanAnomaly));
            var y = (int)(semiMajorAxis * Mathf.Sin(meanAnomaly));

            return (x * eccentricity, y * eccentricity);
        }

        public void GoToNextData()
        {
            if (allRawData.Count == 0) ReadFile();
            currentIndex++;
        }
    }
}