using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;

public class Visible_object : MonoBehaviour
{
    // センサ値取得用
    public GameObject Data_Receiver;
    // 値格納用
    private float yaw_angle = 0.0f;                 //局所回転角度
    private float pitch_angle = 0.0f;               //局所回転角度
    private float force;                            //空気圧の値（左右）
    private float force_ud;                         //空気圧の値（上下）
    //書き出し用
    private string filePath = @"C:\Users\inaga\OneDrive\デスクトップ\exe\csv\ambient\test_random.csv";
    private string filePath2 = @"C:\Users\inaga\OneDrive\デスクトップ\exe\csv\ambient\test_reguler.csv";
    // カメラ内にいるかどうか
    private bool isInsideCamera;
    private int incam_flag = 0;     // 0：incam，1：outcam
    //
    private bool start_flag = false;
    private bool start_flag2 = false;


    // Update is called once per frame
    void Update()
    {
        //シリアル通信から値を取得
        yaw_angle = Data_Receiver.GetComponent<receive_data_gyro_airpress>().yaw_val;
        pitch_angle = Data_Receiver.GetComponent<receive_data_gyro_airpress>().pitch_val;
        force = Data_Receiver.GetComponent<receive_data_gyro_airpress>().f_val;
        force_ud = Data_Receiver.GetComponent<receive_data_gyro_airpress>().f_val_ud;

        if (isInsideCamera)
        {
            //Debug.Log("in cam");
            incam_flag = 0;
        }
        else
        {
            //Debug.Log("out cam");
            incam_flag = 1;
        }

        //蝶々が動き出したら計測開始
        if (Input.GetKeyDown(KeyCode.M))
        {
            start_flag2 = false;
            start_flag = true;
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            start_flag = false;
            start_flag2 = true;
        }
        //F12キーで計測停止
        if (Input.GetKeyDown(KeyCode.F12))
        {
            start_flag = false;
            start_flag2 = false;
        }

        if (start_flag == true)
        {
            // ファイル書き出し
            string[] str = { incam_flag.ToString(), yaw_angle.ToString(), pitch_angle.ToString(), force.ToString() };
            string str2 = string.Join(",", str);
            //書き込み
            File.AppendAllText(filePath, str2 + "\n");
        }

        if (start_flag2 == true)
        {
            // ファイル書き出し
            string[] str = { incam_flag.ToString(), yaw_angle.ToString(), pitch_angle.ToString(), force.ToString() };
            string str2 = string.Join(",", str);
            //書き込み
            File.AppendAllText(filePath2, str2 + "\n");
        }

    }
    // カメラから外れた
    private void OnBecameInvisible()
    {
        isInsideCamera = false;
    }
    // カメラ内に入った
    private void OnBecameVisible()
    {
        isInsideCamera = true;
    }
}
