using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Serial;

public class Backward : MonoBehaviour
{
    public Button btnBack;
    public SerialConnect serial;
    void Start()
    {
        btnBack.onClick.AddListener(OnClick);
        serial = SerialConnect.GetInstance();
        serial.AddListener(new BackwardListener());
        if(!serial.IsListening())
            serial.StartListening();
    }
    void Update()
    {

    }
    void OnDestroy()
    {
        serial.Close();
    }
    void OnClick() {
        Debug.Log("back click");
        serial.SendLine("sig mr 1 10");
    }
}

class BackwardListener : SerialConnect.IListener
{
    public bool OnData(string data)
    {
        Debug.Log("Serial Received: " + data);
        return false;
    }
}