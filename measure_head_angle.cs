using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;

public class measure_head_angle : MonoBehaviour
{
    //角度取得用
    public GameObject Data_Receiver;
    private float yaw_angle = 0.0f;     //ジャイロ角
    //書き出し用
    private float target_angle = 0.0f;  //ターゲット位置（ジャイロ座標系）
    private string filePath = @"C:\Users\inaga\OneDrive\デスクトップ\exe\csv\ambient\gaze\target_gyro.csv";

    // 目的地に着いたかどうか
    private bool isReachTargetPosition;
    // 目的地
    private Vector3 targetPosition;

    // 移動スピード
    private const float SPEED = 0.075f;     //default 0.075

    //phiの範囲, theta固定
    private float INIT_THETA = 90.0f * (Mathf.PI / 180.0f);     //初期位置
    private float INIT_PHI = 90.0f * (Mathf.PI / 180.0f);       //初期位置
    private int num = 0;    //試行回数
    // r
    private const float r = 10.0f;

    // 角度格納用
    private float THETA;
    private float PHI;

    // 極座標変換後の x, y, z の値
    private float X_VALUE;
    private float Y_VALUE;
    private float Z_VALUE;



    void Start()
    {
        this.isReachTargetPosition = false;
        //decideTargetPotision();
        targetPosition = new Vector3(0, 0, 10);
    }

    void Update()
    {
        //シリアル通信から値を取得
        yaw_angle = Data_Receiver.GetComponent<receive_data_gyro_airpress>().yaw_val;

        // Pキーで角度確定→次の位置へ
        if (Input.GetKeyDown(KeyCode.P))
        {
            // 角度記録（ジャイロ角，見ていた角度）
            string[] str = { yaw_angle.ToString(), target_angle.ToString() };
            string str2 = string.Join(",", str);
            File.AppendAllText(filePath, str2 + "\n");
            //次の目的地決定
            decideTargetPotision();
        }

        //移動
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, SPEED);
        // 目的地についたらisReachTargetPositionをtrueにする
        if (transform.position == targetPosition)
        {
            isReachTargetPosition = true;
        }

    }

    // 目的地を設定する
    private void decideTargetPotision()
    {
        // まだ目的地についてなかったら（移動中なら）目的地を変えない
        if (!isReachTargetPosition)
        {
            return;
        }

        //試行回数のインクリメント
        num++;
        //角度の決定
        THETA = INIT_THETA;     //Θは固定
        //ジャイロ座標での計算
        if (num < 13)
        {
            target_angle = num * 10.0f;
        }
        else if (num == 13)
        {
            target_angle = 0.0f;
        }
        else if (num > 13 && num < 26)
        {
            target_angle = -(num - 13) * 10.0f;
        }
        else
        {
            target_angle = 0.0f;
        }

        PHI = (90.0f-target_angle) * (Mathf.PI / 180.0f);   //target_angle（ジャイロ座標）→φ座標→radへ

        //極座標変換
        X_VALUE = r * Mathf.Sin(THETA) * Mathf.Cos(PHI);
        Y_VALUE = r * Mathf.Sin(THETA) * Mathf.Sin(PHI);
        Z_VALUE = r * Mathf.Cos(THETA);

        // 目的地に着いていたら目的地を再設定する
        targetPosition = new Vector3(X_VALUE, Z_VALUE, Y_VALUE);    //座標系を合わせるために(x,z,y)になっている
        isReachTargetPosition = false;
    }

}