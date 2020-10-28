using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;

public class shoulder_move : MonoBehaviour
{
    // センサ値取得用
    public GameObject Data_Receiver;
    // 値格納用
    private float yaw_angle = 0.0f;                 //局所回転角度
    private float pitch_angle = 0.0f;               //局所回転角度
    private float force;                            //空気圧の値（左右）

    //書き出し用
    private string filePath = @"C:\Users\inaga\OneDrive\デスクトップ\exe\csv\shoulder\limit_angle.csv";
    private string filePath2 = @"C:\Users\inaga\OneDrive\デスクトップ\exe\csv\shoulder\test.csv";

    // 移動用変数
    private bool isReachTargetPosition;     // 目的地に着いたかどうか
    private Vector3 targetPosition;         // 目的地
    private const float SPEED = 0.05f;     // 移動スピード: default 0.075

    // r
    private const float r = 10.0f;

    // 補間座標格納用
    private Vector3[] interpolation_vector = new Vector3[10];
    private float interpolation_theta;
    private float interpolation_phi;
    private float pre_theta = 90.0f * (Mathf.PI / 180.0f);     //移動前theta
    private float pre_phi = 90.0f * (Mathf.PI / 180.0f);       //移動前phi
    private int step = 0;

    //キャリブレーション用
    private bool cal_flag = false;
    private int cal_step = 0;
    private Vector3[] cal_vector = new Vector3[11];
    private float MIN_CAL_PHI = 30.0f * (Mathf.PI / 180.0f);    //最小のphi
    private float MAX_CAL_PHI = 150.0f * (Mathf.PI / 180.0f);   //最大のphi
    private float INIT_THETA = 90.0f * (Mathf.PI / 180.0f);     //初期位置
    private float INIT_PHI = 30.0f * (Mathf.PI / 180.0f);       //初期位置：右端30度

    //
    private bool start_flag = false;
    private bool init_flag = false;

    //記録用
    private Vector3 limit_position;       //限界点
    private float limit_phi = 0.0f;       //限界角度
    private Vector3 Now_position;         //蝶々の位置
    private float Now_phi = 0.0f;         //蝶々の角度


    void Start()
    {
        this.isReachTargetPosition = false;
        //初期化
        for (int i = 0; i < 10; i++)
        {
            interpolation_vector[i] = new Vector3(0, 0, 10);
        }
        targetPosition = new Vector3(0, 0, 10);
        //キャリブレーション用の初期化
        init_Cal();
    }

    void Update()
    {
        //シリアル通信から値を取得
        yaw_angle = Data_Receiver.GetComponent<receive_data_gyro_airpress2>().yaw_val;
        pitch_angle = Data_Receiver.GetComponent<receive_data_gyro_airpress2>().pitch_val;
        force = Data_Receiver.GetComponent<receive_data_gyro_airpress2>().f_val;

        // 操作選択
        if (Input.GetKeyDown(KeyCode.C))
        {
            cal_step = 0;
            init_flag = false;
            start_flag = false;
            cal_flag = true;
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            cal_step = 0;
            init_flag = false;
            cal_flag = false;
            start_flag = true;
        }
        //F12キーで計測終了→初期位置に戻る
        if (Input.GetKeyDown(KeyCode.F12))
        {
            start_flag = false;
            cal_flag = false;
            init_flag = true;
        }

        //蝶々の表示非表示
        if (Input.GetKeyDown(KeyCode.D))
        {
            gameObject.SetActive(false);
        }


        if (init_flag == true)
        {
            targetPosition = new Vector3(r * Mathf.Sin(INIT_THETA) * Mathf.Cos(INIT_PHI), r * Mathf.Cos(INIT_THETA), r * Mathf.Sin(INIT_THETA) * Mathf.Sin(INIT_PHI));  //初期位置
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, SPEED);
        }

        // 0. キャリブレーション
        if (cal_flag == true)
        {
            //初期位置→左へ
            transform.position = Vector3.MoveTowards(transform.position, cal_vector[cal_step], SPEED);
            if (transform.position == cal_vector[cal_step])
            {
                if (cal_step < 10)
                {
                    cal_step++;
                }
            }
            //限界点の記録
            if (Input.GetKeyDown(KeyCode.Space))
            {
                limit_position = transform.position;
                limit_phi = Mathf.Acos((float)limit_position[0] / (float)(Mathf.Sqrt(limit_position[0] * limit_position[0] + limit_position[2] * limit_position[2]))) * (180.0f / Mathf.PI);    //限界角（degree）
                // ファイル書き出し
                string[] str = { limit_phi.ToString(), yaw_angle.ToString() };
                string str2 = string.Join(",", str);
                //書き込み
                File.AppendAllText(filePath, str2 + "\n");
                //初期地点に戻る                                                                                                                                                                          
                start_flag = false;
                cal_flag = false;
                init_flag = true;
            }
        }

        // 1. 計測用
        if (start_flag == true)
        {
            //初期位置→左へ
            transform.position = Vector3.MoveTowards(transform.position, cal_vector[cal_step], SPEED);
            if (transform.position == cal_vector[cal_step])
            {
                if (cal_step < 10)
                {
                    cal_step++;
                }
                else
                {
                    //移動完了＝計測終了→初期地点に戻る                                                                                                                                                                          //Debug.Log("limit phi: " + limit_phi);
                    start_flag = false;
                    cal_flag = false;
                    init_flag = true;
                }
            }

            Now_position = transform.position;  //蝶々の位置
            Now_phi = Mathf.Acos((float)Now_position[0] / (float)(Mathf.Sqrt(Now_position[0] * Now_position[0] + Now_position[2] * Now_position[2]))) * (180.0f / Mathf.PI);    //限界角（degree）

            // ファイル書き出し
            string[] str = { yaw_angle.ToString(), pitch_angle.ToString(), force.ToString(), (Now_phi - 30).ToString() };
            string str2 = string.Join(",", str);
            //書き込み
            File.AppendAllText(filePath2, str2 + "\n");
        }
    }


    // キャリブレーション用初期化
    private void init_Cal()
    {
        //初期位置から左へ
        for (int i = 0; i < 10; i++)
        {
            interpolation_phi = INIT_PHI + ((MAX_CAL_PHI - INIT_PHI) / 10.0f) * i;
            //極座標変換した座標を格納（x,z,y）
            cal_vector[i] = new Vector3(r * Mathf.Sin(INIT_THETA) * Mathf.Cos(interpolation_phi), r * Mathf.Cos(INIT_THETA), r * Mathf.Sin(INIT_THETA) * Mathf.Sin(interpolation_phi));
            //Debug.Log("interpolation_phi: " + interpolation_phi * (180.0f / Mathf.PI));
        }
        cal_vector[10] = new Vector3(r * Mathf.Sin(INIT_THETA) * Mathf.Cos(MAX_CAL_PHI), r * Mathf.Cos(INIT_THETA), r * Mathf.Sin(INIT_THETA) * Mathf.Sin(MAX_CAL_PHI));    //最終目的地点
    }

}