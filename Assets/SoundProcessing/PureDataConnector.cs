using System;
using Tools;
using UnityEngine;
using Logger = Tools.Logger;

namespace SoundProcessing
{
    public class PureDataConnector : MonoBehaviour
    {
        public Logger logger;
        private JsonConfiguration config;
        private OSC osc;

        private void Start()
        {
            OpenConnection();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyBindings.TogglePureData))
            {
                if (!IsOpen())
                {
                    logger.Log($"Trying to connect to Pure Data : {config.outIp}:{config.outPort} ");
                    OpenConnection();
                }
                else
                {
                    logger.Log("Closing Pure Data connection ");
                    osc.Close();
                }
            }

            if (!IsOpen())
                return;

            osc.Update();
        }

        public void OnDestroy()
        {
            if (!IsOpen())
                return;

            osc.OnDestroy();
        }

        private void OpenConnection()
        {
            config = Configuration.GetConfig();
            try
            {
                osc = new OSC(config.inPort, config.outIp, config.outPort);
                logger.Log("Connected to Pure Data client !");

                var oscMess = new OscMessage();
                oscMess.address = "/Test";
                oscMess.values.Add("Hello");
                oscMess.values.Add(DateTime.Now.Millisecond);
                Send(oscMess);
            }
            catch (Exception e)
            {
                osc = null;
                logger.Log(e.Message);
                throw;
            }
        }

        public bool IsOpen()
        {
            return osc != null && osc.IsOpen();
        }

        public void Send(OscMessage message)
        {
            if (!IsOpen())
                logger.Log("Error when trying to send a message, the connection is not open")
                    ;
            osc.Send(message);
        }

        public void SendOscMessage(string address, int value)
        {
            Send(new OscMessage(address, value));
        }

        public void SendOscMessage(string address, float value)
        {
            Send(new OscMessage(address, value));
        }

        public void SendOscMessage(string address, string value)
        {
            Send(new OscMessage(address, value));
        }
    }
}