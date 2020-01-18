using UnityEngine;
using UnityEngine.UI;
using Serial;

public class Stop : MonoBehaviour
{
    public Button BtnStop;

    private SerialConnect _serial;
    // Start is called before the first frame update
    void Start()
    {
        BtnStop.onClick.AddListener(OnClick);
        _serial = SerialConnect.GetInstance();
        _serial.StartListening();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        _serial.Close();
    }
    void OnClick()
    {
        Debug.Log("stop click");
        _serial.SendLine("sig ms");
    }
}
