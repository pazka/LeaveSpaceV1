using System;
using System.Collections.Generic;
using System.IO;
using DataProcessing.Generic;
using JetBrains.Annotations;
using Tools;
using UnityEngine;

namespace DataProcessing.AllGP
{
    [Serializable]
    public class AllGpJsonData
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
        [CanBeNull] public AllGpJsonDataVisu DataVisu;
    }

    [Serializable]
    public class AllGpJsonDataVisu
    {
        public float projected2DX;
        public float projected2DY;
        public float projected2DRadius;
    }


    public class AllGPDataReader : DataProcessing.Generic.IDataReader
    {
        [Serializable]
        public class AllGpJsonMetadata
        {
            public int Total;
            public int Limit;
            public int LimitOffset;
            public int ReturnedRows;
            public string RequestTime;
            public string DataSize;
        }

        [Serializable]
        public class AllGPJson
        {
            public List<AllGpJsonData> data;
        }

        List<AllGpJsonData> allRawData;
        private int currentIndex;
        readonly string filePath = Application.dataPath + "/StreamingAssets/all_gp_data_visu.json";

        public void Init()
        {
            Clean();
            ReadFile();
        }

        private void ReadFile()
        {
            if (allRawData.Count > 0) return;

            var fileContent = File.ReadAllText(filePath);
            var json = JsonUtility.FromJson<AllGPJson>("{\"data\":"+fileContent+"}");
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
            return IsDecayed(data.DECAY_DATE) && data.DataVisu != null;
        }

        public TimedData GetData()
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

            float potentialDate = GetDateInSecondsFromLaunchDate(currRawData.LAUNCH_DATE);
            if (potentialDate < 0)
                potentialDate = GetDateInSecondsFromEpoch(currRawData.EPOCH);

            var position = SpaceTools.GetXYFromAllGpJsonDataVisu(currRawData);

            var newData = new AllGPData(currRawData,
                position.x,
                position.y,
                potentialDate
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

        private static float GetDateInSecondsFromEpoch(string epoch)
        {
            //format of epoch is ISO 8601 
            //https://en.wikipedia.org/wiki/ISO_8601
            //YYYY-MM-DDThh:mm:ss.ssssss
            var epochSplit = epoch.Split('T');
            var date = epochSplit[0];
            var time = epochSplit[1];
            var dateSplit = date.Split('-');
            var timeSplit = time.Split(':');
            var year = int.Parse(dateSplit[0]);
            var month = int.Parse(dateSplit[1]);
            var day = int.Parse(dateSplit[2]);
            var hour = int.Parse(timeSplit[0]);
            var minute = int.Parse(timeSplit[1]);
            var second = int.Parse(timeSplit[2].Split('.')[0]);
            var millisecond = int.Parse(timeSplit[2].Split('.')[1]);
            var epochDateTime = new DateTime(year, month, day, hour, minute, second, millisecond % 999);
            var today = DateTime.Now;
            var timeSpan = today - epochDateTime;
            return (float)timeSpan.TotalSeconds;
        }

        private static float GetDateInSecondsFromLaunchDate(string launchDate)
        {
            if (string.IsNullOrEmpty(launchDate))
            {
                return -1;
            }

            //format of launchYear is "YYYY-MM-DD"
            var launchDateSplit = launchDate.Split('-');
            var launchYear = int.Parse(launchDateSplit[0]);
            var launchMonth = int.Parse(launchDateSplit[1]);
            var launchDay = int.Parse(launchDateSplit[2]);
            var launchDateDateTime = new DateTime(launchYear, launchMonth, launchDay);
            var today = DateTime.Now;
            var timeSpan = today - launchDateDateTime;
            return (float)timeSpan.TotalSeconds;
        }


        public void GoToNextData()
        {
            if (allRawData.Count == 0) ReadFile();
            currentIndex++;
        }
    }
}