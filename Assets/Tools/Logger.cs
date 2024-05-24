using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

namespace Tools
{
    public class Logger : MonoBehaviour
    {
        private static string LogPath;
        public int LineLimit = 30;
        public TextMeshPro DebugText;
        public int lastIndex;
        public FileStream fileDump;
        private int lineCounter;

        public Queue<string> logLines = new Queue<string>();
        public Queue<string> logLinesToDump = new Queue<string>();
        private int position = 0;

        public void Reset()
        {
            logLines.Clear();
            lineCounter = 0;
        }

        public void Start()
        {
            var tstr = GetTimeString();
            lineCounter = 0;
            Directory.CreateDirectory(Application.persistentDataPath + "/DataVisuLogs");
            LogPath = Application.persistentDataPath + "/DataVisuLogs/" + tstr + ".txt";
            File.WriteAllText(LogPath, "");
            Log("LogFile : " + Application.persistentDataPath + "/DataVisuLogs/" + tstr + ".txt");
        }

        public void Update()
        {
            if (lastIndex != lineCounter)
            {
                File.AppendAllText(LogPath, string.Join("\n", logLinesToDump)+"\n");
                logLinesToDump.Clear();

                var str = "";
                foreach (var logLine in logLines) str = logLine + "\n" + str;

                DebugText.SetText(str);

                lastIndex = lineCounter;
            }
        }

        public FileStream GetFile()
        {
            if (fileDump == null) LogPath = Application.persistentDataPath + "/../Logs." + GetTimeString() + ".txt";

            return fileDump;
        }

        public string GetTimeString()
        {
            var t = DateTime.Now;
            return t.Month + "_" + t.Day + "." + t.Hour + "." + t.Minute + "." + t.Second + "_" + t.Millisecond;
        }

        public void Log(string str)
        {
            Debug.Log(str);
            logLinesToDump.Enqueue(GetTimeString() + " : " + str);

            if (logLines.Count > LineLimit) logLines.Dequeue();

            Console.WriteLine(str);
            logLines.Enqueue(str);
            lineCounter++;
        }

        public void Error(string str)
        {
            Log("ERROR : " + str);
        }
    }
}