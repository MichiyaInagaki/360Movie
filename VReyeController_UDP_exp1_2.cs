using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using UnityEngine.UI;

public class VReyeController_UDP_exp1_2 : MonoBehaviour
{
    //Publicオブジェクト（格納）
    public GameObject VReye;
    public Camera mainCamera;
    public GameObject UDP_Data_Receiver;
    public bool fade_on = true;
    //
    //各種パラメータ
    private float rotation_speed = 0.5f;   //旋回スピード
    private string filePath = @"C:\Users\inaga\OneDrive\デスクトップ\exe\csv\horizontal_exp1_2\test.csv";
    //
    //Privateオブジェクト
    private Vector3 newAngle = new Vector3(0, 0, 0);        //カメラ姿勢
    private Vector3 Initial_Angle = new Vector3(0, 0, 0);   //初期カメラ姿勢
    //
    private float yaw_angle = 0.0f;                 //局所回転角度
    private float pitch_angle = 0.0f;
    private float roll_angle = 0.0f;
    private float rotation_angle = 0.0f;            //大域回転角度
    //private float rotation_angle_pitch = 0.0f;
    //実験用パラメータ
    private float initial_angle_pitch = 0.0f;       //初期値
    private float ajust_angle_pitch = 0.0f;         //調整角度
    private float rotation_angle_pitch = 0.0f;      //最終的な回転角
    private float condition_pitch_angle = 90.0f;    //条件roll角度
    private int trial_num = 0;                      //試行回数
    private bool next_flag = true;                  //試行切り替え



    // Start is called before the first frame update
    void Start()
    {
        //角度の初期化
        newAngle = Initial_Angle;
        //乱数シード
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
    }

    // Update is called once per frame
    void Update()
    {
        //シリアル通信から値を取得
        yaw_angle = UDP_Data_Receiver.GetComponent<UDP_roll>().yaw_val;
        pitch_angle = UDP_Data_Receiver.GetComponent<UDP_roll>().pitch_val;
        roll_angle = UDP_Data_Receiver.GetComponent<UDP_roll>().roll_val;

        //初期値決定部
        if (next_flag == true)
        {
            ajust_angle_pitch = 0.0f;          //調整角度初期化
            //
            if (trial_num % 2 == 0)
            {
                initial_angle_pitch = UnityEngine.Random.Range(30, 50);
                next_flag = false;
            }
            else
            {
                initial_angle_pitch = UnityEngine.Random.Range(-50, -30);
                next_flag = false;
            }
        }

        //調整部
        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (ajust_angle_pitch < 90.0f)       //限界角
            {
                ajust_angle_pitch += 1.0f;
            }
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (ajust_angle_pitch > - 90.0f)
            {
                ajust_angle_pitch -= 1.0f;
            }
        }

        //pitch補正角度の決定
        rotation_angle_pitch = initial_angle_pitch + ajust_angle_pitch;    //最終的な回転角＝初期値＋調整角度
        Debug.Log("rotation_angle: " + rotation_angle_pitch + " initial_angle: " + initial_angle_pitch + " ajust_angle: " + ajust_angle_pitch);
        //姿勢の決定
        newAngle.y = -30;                       //yaw固定
        newAngle.z = 0;                         //roll固定
        newAngle.x = rotation_angle_pitch;      //pitch補正
        VReye.gameObject.transform.localEulerAngles = newAngle;     //姿勢の代入


        //条件roll角度の切り替え
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            trial_num = 0;
            next_flag = true;
            condition_pitch_angle = 0.0f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            trial_num = 0;
            next_flag = true;
            condition_pitch_angle = 10.0f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            trial_num = 0;
            next_flag = true;
            condition_pitch_angle = 20.0f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            trial_num = 0;
            next_flag = true;
            condition_pitch_angle = 30.0f;
        }
        //if (Input.GetKeyDown(KeyCode.Alpha4))
        //{
        //    trial_num = 0;
        //    next_flag = true;
        //    condition_pitch_angle = 40.0f;
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha5))
        //{
        //    trial_num = 0;
        //    next_flag = true;
        //    condition_pitch_angle = 50.0f;
        //}


        //条件角度とフェードのオンオフ対応
        if ((condition_pitch_angle + 1.0f) > pitch_angle && pitch_angle > (condition_pitch_angle - 1.0f))
        {
            fade_on = false;
        }
        else
        {
            fade_on = true;
        }



        if (Input.GetKeyDown(KeyCode.P))
        {
            trial_num++;
            // ファイル書き出し
            string[] str = { trial_num.ToString(), condition_pitch_angle.ToString(), rotation_angle_pitch.ToString() };
            string str2 = string.Join(",", str);
            File.AppendAllText(filePath, str2 + "\n");
            // 次の試行へ
            if (trial_num <= 10)
            {
                next_flag = true;
            }
        }

        //自分用
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            trial_num++;
            // ファイル書き出し
            string[] str = { trial_num.ToString(), condition_pitch_angle.ToString(), rotation_angle_pitch.ToString() };
            string str2 = string.Join(",", str);
            File.AppendAllText(filePath, str2 + "\n");
            // 次の試行へ
            if (trial_num <= 10)
            {
                next_flag = true;
            }
        }


    }

}
