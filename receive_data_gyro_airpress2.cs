using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class receive_data_gyro_airpress2 : MonoBehaviour
{
    public SerialHandler_airpress serialHandler;
    public float yaw_val;
    public float pitch_val;
    public float f_val;     //左右方向のフォース
    //
    private int P0, P1;             //圧力生データ
    private int P0_initial = 0;     //圧力初期値
    private int P1_initial = 0;
    private float D0, D1;            //圧力変化量


    void Start()
    {
        //信号を受信したときに、そのメッセージの処理を行う
        serialHandler.OnDataReceived += OnDataReceived;
    }

    void Update()
    {
        // Rキーで姿勢を初期化
        if (Input.GetKey(KeyCode.R))
        {
            //ジャイロセンサ初期化
            serialHandler.Write("0");   //文字列を送信
            //圧力枕初期化
            P0_initial = P0;    //頭を中心部に置いた時の圧力
            P1_initial = P1;
            f_val = 0.0f;
        }

        //圧力の変化量
        D0 = (float)(P0 - P0_initial);
        D1 = (float)(P1 - P1_initial);
        //左右ストローク
        f_val = D1 - D0;
        Debug.Log("D0: " + D0 + " D1: " + D1 + " f_val: " + f_val);
    }

    //受信処理
    void OnDataReceived(string message)
    {
        try
        {
            //Debug.Log(message);
            //"\t"分割でpitchとyaw情報取得
            string[] data = message.Split(new string[] { "\t" }, StringSplitOptions.None);
            yaw_val = -float.Parse(data[1]);
            pitch_val = float.Parse(data[0]);
            P0 = int.Parse(data[2]);      //圧力生データ
            P1 = int.Parse(data[3]);
            //Debug.Log("yaw: " + yaw_val + " pitch: " + pitch_val + " P0: " + P0 + " P1: " + P1);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }
}