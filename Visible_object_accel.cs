using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;

public class Visible_object_accel : MonoBehaviour
{
    // センサ値取得用
    public GameObject Data_Receiver;
    // 値格納用
    private float yaw_angle = 0.0f;                 //局所回転角度
    private float pitch_angle = 0.0f;               //局所回転角度
    private float force;                            //空気圧の値（左右）
    private float yaw_velocity = 0.0f;              //角速度
    private float yaw_accel = 0.0f;                 //角加速度
    //書き出し用
    private string filePath = @"C:\Users\inaga\OneDrive\デスクトップ\exe\csv\accel\random.csv";
    private string filePath2 = @"C:\Users\inaga\OneDrive\デスクトップ\exe\csv\accel\reguler.csv";
    private float time = 0.0f;  //時刻
    // カメラ内にいるかどうか
    private bool isInsideCamera;
    private int incam_flag = 0;     // 0：incam，1：outcam
    //
    private bool start_flag = false;
    private bool start_flag2 = false;
    //
    private int cameraNum;


    // Update is called once per frame
    void Update()
    {
        //シリアル通信から値を取得
        yaw_angle = Data_Receiver.GetComponent<receive_data_accel>().yaw_val;
        pitch_angle = Data_Receiver.GetComponent<receive_data_accel>().pitch_val;
        force = Data_Receiver.GetComponent<receive_data_accel>().f_val;
        yaw_velocity = Data_Receiver.GetComponent<receive_data_accel>().yaw_velocity;
        yaw_accel = Data_Receiver.GetComponent<receive_data_accel>().yaw_accel;

        //メインカメラから外れたかどうか
        //if (isInsideCamera)
        //{
        //    //Debug.Log("in cam");
        //    incam_flag = 0;
        //}
        //else
        //{
        //    //Debug.Log("out cam");
        //    incam_flag = 1;
        //}

        //インカメラ画角とメインカメラ画角
        ShowText(cameraNum);
        cameraNum = 0;
        //Debug.Log(incam_flag);

        //蝶々が動き出したら計測開始
        if (Input.GetKeyDown(KeyCode.M))
        {
            start_flag2 = false;
            start_flag = true;
            time = 0.0f;
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            start_flag = false;
            start_flag2 = true;
            time = 0.0f;
        }
        //F12キーで計測停止
        if (Input.GetKeyDown(KeyCode.F12))
        {
            start_flag = false;
            start_flag2 = false;
            time = 0.0f;
        }

        if (start_flag == true)
        {
            time += Time.deltaTime;
            // ファイル書き出し
            string[] str = { time.ToString(), incam_flag.ToString(), yaw_angle.ToString(), pitch_angle.ToString(), force.ToString(), yaw_velocity.ToString(), yaw_accel.ToString() };
            string str2 = string.Join(",", str);
            //書き込み
            File.AppendAllText(filePath, str2 + "\n");
        }

        if (start_flag2 == true)
        {
            time += Time.deltaTime;
            // ファイル書き出し
            string[] str = { time.ToString(), incam_flag.ToString(), yaw_angle.ToString(), pitch_angle.ToString(), force.ToString(), yaw_velocity.ToString(), yaw_accel.ToString() };
            string str2 = string.Join(",", str);
            //書き込み
            File.AppendAllText(filePath2, str2 + "\n");
        }

    }


    //// メインカメラから外れた
    //private void OnBecameInvisible()
    //{
    //    isInsideCamera = false;
    //}
    //// メインカメラ内に入った
    //private void OnBecameVisible()
    //{
    //    isInsideCamera = true;
    //}

    // インカメラ画角とメインカメラ画角
    void OnWillRenderObject()
    {
        if (Camera.current.name == "Inside Camera")
        {
            //Debug.Log("inside");
            cameraNum++;
        }
        if (Camera.current.name == "Main Camera")
        {
            //Debug.Log("outside");
            cameraNum += 2;
        }
    }

    void ShowText(int num)
    {
        switch (num)
        {
            case 0:
                //Debug.Log("映ってないよ");
                incam_flag = 1;
                break;
            case 1:
                //Debug.Log("inside");
                break;
            case 2:
                //Debug.Log("outside");
                incam_flag = 1;
                break;
            case 3:
                //Debug.Log("both");
                incam_flag = 0;
                break;
        }
    }
}
