using System;
using System.Collections.Generic;
using System.IO;
using DataProcessing.Generic;
using UnityEngine;

namespace DataProcessing.AllGP
{
    abstract class AllGpJsonMetadata
    {
        public int Total;
        public int Limit;
        public int LimitOffset;
        public int ReturnedRows;
        public string RequestTime;
        public string DataSize;
    }

    public abstract class AllGpJsonData
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

    public class AllGPDataReader : IDataReader<AllGPData>
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

        private bool DataIsToKeep(AllGpJsonData data)
        {
            return IsDecayed(data.DECAY_DATE);
        }

        public AllGPData GetData()
        {
            if (allRawData.Count == 0) ReadFile();

            if (currentIndex >= allRawData.Count) return null;

            var currRawData = allRawData[currentIndex];

            while (!DataIsToKeep(currRawData))
            {
                currentIndex++;
                if (currentIndex >= allRawData.Count) return null;
                currRawData = allRawData[currentIndex];
            }

            var newData = new AllGPData(currRawData,
                -1,
                -1,
                GetDateInSecondsFromEpochJulianDate(currRawData.EPOCH)
            );

            return newData;
        }

        private bool IsDecayed(string decayDate)
        {
            //decayDate is in the format "YYY-MM-DD"
            if (decayDate == null) return false;
            if (string.IsNullOrEmpty(decayDate)) return false;

            var decayDateSplit = decayDate.Split('-');
            var decayYear = int.Parse(decayDateSplit[0]);
            var decayMonth = int.Parse(decayDateSplit[1]);
            var decayDay = int.Parse(decayDateSplit[2]);
            var decayDateDateTime = new DateTime(decayYear, decayMonth, decayDay);
            var today = DateTime.Now;

            return decayDateDateTime < today;
        }

        private static float GetDateInSecondsFromEpochJulianDate(string epoch)
        {
            var epochDouble = double.Parse(epoch);
            var epochDate = new DateTime(1950, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            epochDate = epochDate.AddDays(epochDouble);
            return (float)epochDate.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }


        public void GoToNextData()
        {
            if (allRawData.Count == 0) ReadFile();
            currentIndex++;
        }
    }
}