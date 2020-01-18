using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Serial
{
    /*
     * Usage: 
     * SerialConnect sc = SerialConnect.getInstance();
     * sc.addListener(...)
     * sc.open(...)
     * sc.startListening()
     * sc.stopListening()
     * sc.close()
     */
    public class SerialConnect
    {
        // singleton for easy access in Unity Objects
        private static SerialConnect instance;
        private static readonly object initLock = new object();
        public static SerialConnect GetInstance()
        {
            lock (initLock)
            {
                if (instance == null)
                {
                    instance = new SerialConnect();
                    // 暂时把初始值写在这里
                    instance.Open("COM4", 115200, Parity.None, 8, StopBits.One);
                }
            }
            return instance;
        }

        // static methods
        public static string[] ScanPorts()
        {
            return SerialPort.GetPortNames();
        }


        // listener interface for incoming data
        public interface IListener
        {
            // return true to stop iterating other listeners
            bool OnData(string data);
        }

        // class definition
        public SerialPort Port { get; private set; }
        private readonly List<IListener> listeners = new List<IListener>();
        private Thread listenThread; // it's an indicator whether is listening incoming data.

        private SerialConnect() { }
        public void Open(string device, int baud, Parity parity, int databits, StopBits stopbits)
        {
            if (IsOpen())
            {
                Close();
            }
            // Microsoft document says the best practice is wait some time between close and open
            Port = new SerialPort(device, baud, parity, databits, stopbits)
            {
                Handshake = Handshake.None,
                ReadTimeout = 100,
                WriteTimeout = 100
            };
            Port.Open();
        }
        public bool IsOpen()
        {
            return Port != null && Port.IsOpen;
        }
        public void Close()
        {
            if (IsListening())
                StopListening();
            if (IsOpen())
                Port.Close();
        }
        public bool AddListener(IListener listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
                return true;
            }
            return false;
        }
        public bool RemoveListener(IListener listener)
        {
            return listeners.Remove(listener);
        }

        private bool _listening;
        public bool IsListening()
        {
            return listenThread != null;
        }
        public bool StartListening()
        {
            if (!IsOpen())
                return false;
            if (!IsListening())
            {
                _listening = true;
                listenThread = new Thread(new ThreadStart(ReadPort));
                listenThread.Start();
            }
            else
            {
                // Debug.Log("Is Already Listening");
            }
            return true;
        }
        private void ReadPort()
        {
            while (_listening)
            {
                try
                {
                    string line = Port.ReadLine();
                    foreach (IListener listener in listeners)
                    {
                        if (listener.OnData(line))
                            break;
                    }
                }
                catch (TimeoutException) { }
            }
        }
        public void StopListening()
        {
            if (IsListening())
            {
                _listening = false;
                listenThread.Join();
                listenThread = null;
            }
        }
        public void SendLine(string data)
        {
            if (IsOpen())
            {
                Port.WriteLine(data);
                Port.BaseStream.Flush();
                Debug.Log("Serial Sent: " + data);
            }
            else
            {
                Debug.Log("Port is not open.");
            }
        }
    }
}
