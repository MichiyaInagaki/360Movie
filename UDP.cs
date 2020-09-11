using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDP : MonoBehaviour
{
    public float yaw_val;
    public float pitch_val;
    private float pre_yaw = 0.0f;
    private float pre_pitch = 0.0f;
    private float filter_gain = 0.5f;
    private float initial_yaw = 0.0f;
    private float initial_pitch = 0.0f;
    //
    static UdpClient udp;
    IPEndPoint remoteEP = null;
    int i = 0;
    // Use this for initialization
    void Start()
    {
        int LOCA_LPORT = 50007;

        udp = new UdpClient(LOCA_LPORT);
        udp.Client.ReceiveTimeout = 2000;
    }

    // Update is called once per frame
    void Update()
    {
        IPEndPoint remoteEP = null;
        byte[] data = udp.Receive(ref remoteEP);        //送られてきたデータをbyte型で取得
        string text = Encoding.UTF8.GetString(data);    //string型に変換
        string[] dest = text.Split(',');                //データごとに分割
        yaw_val = -float.Parse(dest[0]);
        pitch_val = float.Parse(dest[1]);
        // ローパスフィルタ
        //yaw_val = pre_yaw * filter_gain + yaw_val * (1 - filter_gain);
        //pre_yaw = yaw_val;
        //pitch_val = pre_pitch * filter_gain + pitch_val * (1 - filter_gain);
        //pre_pitch = pitch_val;
        if (Input.GetKeyDown(KeyCode.R))
        {
            initial_yaw = yaw_val;
            initial_pitch = pitch_val;
        }
        yaw_val = yaw_val - initial_yaw;
        pitch_val = pitch_val - initial_pitch;
        Debug.Log("yaw: " + yaw_val +" pitch: " + pitch_val);					//送られてきたデータの表示
    }
}
