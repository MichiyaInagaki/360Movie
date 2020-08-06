﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using UnityEngine.UI;

public class VReyeController_ambient : MonoBehaviour
{
    //Publicオブジェクト（格納）
    public GameObject VReye;
    public Camera mainCamera;
    public GameObject Data_Receiver;
    //
    //各種パラメータ
    public int stride = 50;               //離散移動の歩幅
    public float rotation_speed = 0.5f;   //旋回スピード
    //
    //Publicオブジェクト（送信）
    public bool fade_in = false;
    public bool fade_out = false;
    //
    //Privateオブジェクト
    private Vector3 newAngle = new Vector3(0, 0, 0);        //カメラ姿勢
    private Vector3 Initial_Angle = new Vector3(0, 0, 0);   //初期カメラ姿勢
    //
    private float yaw_angle = 0.0f;                 //局所回転角度
    private float pitch_angle = 0.0f;               //局所回転角度
    private float rotation_angle = 0.0f;            //大域回転角度
    private float rotation_angle_pitch = 0.0f;      //大域回転角度
    private float force;                            //空気圧の値（左右）
    private float force_ud;                         //空気圧の値（上下）
    private float force_to_angle;                   //空気圧の値を角度に変換した値
    private float t_force = 100.0f;                 //空気圧回転閾値
    //
    private bool headlock_flag = false;             //ヘッドロック用
    private float pre_angle = 0.0f;                 //ヘッドロック用
    private float temp_yaw_angle = 0.0f;            //トラッキング無効用
    private bool notrack_flag = false;              //トラッキング無効フラグ
    private float span_notrack = 1.0f;              //トラッキング無効時間
    private float currentTime_notrack = 0f;         //トラッキング無効時間カウント
    private float restart_yaw = 0.0f;               //トラッキング再開用
    private float headlock_gain = 0.8f;             //ヘッドロックゲイン
    private float mod_angle = 0.0f;                 //補正用角度
    private float gyro_yaw_angle = 0.0f;            //ジャイロ角計算用
    //
    private bool gazing_flag = false;               //注視中フラグ
    private bool gaze_on_flag = false;              //注視トリガー
    private float gaze_span = 3.0f;                 //注視の時間間隔
    private float gaze_time = 0f;                   //注視時間カウント
    private float gaze_angle = 0.0f;                //注視角度
    private float gaze_limit_angle = 10.0f;         //注視角度の許容ブレ
    private float no_gaze_angle = 20.0f;            //注視領域でない角度±
    //                                              
    private float move_angle = 40.0f;               //頭部制御閾値
    private bool head_move_flag = false;            //頭部制御中フラグ
    //
    //ローパスフィルタ
    private float filter_gain = 0.75f;              //default: 0.75
    private float pre_yaw = 0.0f;
    private float pre_pitch = 0.0f;
    private float pre_force = 0.0f;
    private float mod_force = 0.0f;
    //平均値フィルタ
    private const int AVE_NUM = 10;               //平均するデータの個数
    private float ave_force = 0;
    private float[] list_force = new float[AVE_NUM];
    //
    private bool first_step_flag = true;            //短押し用フラグ
    private float span = 1.0f;                      //長押しの時間間隔
    private float currentTime = 0f;                 //長押し時間カウント
    //
    //固定フレームレートで回転させるためのフラグ
    private bool fixed_L_flag = false;
    private bool fixed_R_flag = false;
    private bool fixed_U_flag = false;
    private bool fixed_D_flag = false;
    //
    //動作切り替え用                                                        
    private bool f1_flag = false;
    private bool f2_flag = false;
    private bool f3_flag = false;
    private bool f4_flag = false;
    private bool f5_flag = false;
    private bool f6_flag = false;
    private bool f7_flag = false;
    private bool f8_flag = false;
    private bool f9_flag = false;
    private bool f10_flag = false;


    // Start is called before the first frame update
    void Start()
    {
        //角度の初期化
        newAngle = Initial_Angle;
    }

    // Update is called once per frame
    void Update()
    {
        //シリアル通信から値を取得
        yaw_angle = Data_Receiver.GetComponent<receive_data_gyro_airpress>().yaw_val;
        pitch_angle = Data_Receiver.GetComponent<receive_data_gyro_airpress>().pitch_val;
        force = Data_Receiver.GetComponent<receive_data_gyro_airpress>().f_val;
        force_ud = Data_Receiver.GetComponent<receive_data_gyro_airpress>().f_val_ud;


        //動作切り替え
        if (Input.GetKeyDown(KeyCode.F1))
        {
            FlagDown();
            f1_flag = true;
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            FlagDown();
            f2_flag = true;
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            FlagDown();
            f3_flag = true;
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            FlagDown();
            f4_flag = true;
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            FlagDown();
            f5_flag = true;
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            FlagDown();
            f6_flag = true;
        }
        if (Input.GetKeyDown(KeyCode.F7))
        {
            FlagDown();
            f7_flag = true;
        }
        if (Input.GetKeyDown(KeyCode.F8))
        {
            FlagDown();
            f8_flag = true;
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            FlagDown();
            f9_flag = true;
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            FlagDown();
            f10_flag = true;
        }


        //各種処理
        //1．局所回転：なし / 大域回転：キー操作Yaw＋Pitch ///////////////////////////////////////
        if (f1_flag == true)
        {
            //大域回転部（一定速度＋フェード）
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                fade_in = true;
                fade_out = false;
            }
            //FixedUpdeteでの回転処理開始
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                fixed_L_flag = true;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                fixed_R_flag = true;
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                fixed_U_flag = true;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                fixed_D_flag = true;
            }
            //FixedUpdeteでの回転処理終了
            if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow))
            {
                fixed_L_flag = fixed_R_flag = fixed_U_flag = fixed_D_flag = false;
                fade_in = false;
                fade_out = true;
            }
            //
            //姿勢の決定
            newAngle.y = rotation_angle;            //局所回転角度＋大域回転角度
            newAngle.z = 0;                                     //首回転角roll初期化
            newAngle.x = rotation_angle_pitch;    //局所回転角度＋大域回転角度
            VReye.gameObject.transform.localEulerAngles = newAngle;     //姿勢の代入
            //
            //歩行動作（長押し・短押し対応版）
            if (Input.GetKey(KeyCode.Space))
            {
                currentTime += Time.deltaTime;  //長押しの時間カウント
                if (first_step_flag == true)    //最初に押した瞬間は一歩進む（短押し用）
                {
                    moveForward_D();
                    first_step_flag = false;
                }
                else
                {
                    if (currentTime > span)     //長押しで一定時間ごとに前進
                    {
                        moveForward_D();
                        currentTime = 0f;
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.Space))  //ボタン離したらフラグ戻す（短押し用）
            {
                first_step_flag = true;
                currentTime = 0f;
            }
        }
        //END 1.///////////////////////////////////////////////////////////////////////////////////////


        //2．局所回転：ジャイロ / 大域回転：キー操作Yaw＋Pitch ///////////////////////////////////////
        if (f2_flag == true)
        {
            //局所回転角度＋ローパスフィルタ
            yaw_angle = pre_yaw * filter_gain + yaw_angle * (1 - filter_gain);
            pre_yaw = yaw_angle;
            pitch_angle = pre_pitch * filter_gain + pitch_angle * (1 - filter_gain);
            pre_pitch = pitch_angle;
            //
            //大域回転部（一定速度＋フェード）
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                fade_in = true;
                fade_out = false;
            }
            //FixedUpdeteでの回転処理開始
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                fixed_L_flag = true;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                fixed_R_flag = true;
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                fixed_U_flag = true;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                fixed_D_flag = true;
            }
            //FixedUpdeteでの回転処理終了
            if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow))
            {
                fixed_L_flag = fixed_R_flag = fixed_U_flag = fixed_D_flag = false;
                fade_in = false;
                fade_out = true;
            }
            //
            //姿勢の決定
            newAngle.y = yaw_angle + rotation_angle;            //局所回転角度＋大域回転角度
            newAngle.z = 0;                                     //首回転角roll初期化
            newAngle.x = pitch_angle + rotation_angle_pitch;    //局所回転角度＋大域回転角度
            VReye.gameObject.transform.localEulerAngles = newAngle;     //姿勢の代入
            //
            //歩行動作（長押し・短押し対応版）
            if (Input.GetKey(KeyCode.Space))
            {
                currentTime += Time.deltaTime;  //長押しの時間カウント
                if (first_step_flag == true)    //最初に押した瞬間は一歩進む（短押し用）
                {
                    moveForward_D();
                    first_step_flag = false;
                }
                else
                {
                    if (currentTime > span)     //長押しで一定時間ごとに前進
                    {
                        moveForward_D();
                        currentTime = 0f;
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.Space))  //ボタン離したらフラグ戻す（短押し用）
            {
                first_step_flag = true;
                currentTime = 0f;
            }
        }
        //END 2.///////////////////////////////////////////////////////////////////////////////////////


        //3．局所回転：空気圧枕 / 大域回転：キー操作Yaw＋Pitch ///////////////////////////////////////
        if (f3_flag == true)
        {
            //局所回転角の取得
            force_to_angle = 0.4f * force;       //y=ax : force100 / yaw40
            //平均値フィルタ
            for (int i = AVE_NUM - 1; i > 0; i--)
            {
                list_force[i] = list_force[i - 1];
            }
            list_force[0] = force_to_angle;
            for (int i = 0; i < AVE_NUM; i++)
            {
                ave_force += list_force[i];
            }
            ave_force = (float)(ave_force / AVE_NUM);
            //ローパスフィルタ
            mod_force = pre_force * filter_gain + ave_force * (1 - filter_gain);
            pre_force = mod_force;
            //ここで角度代入
            yaw_angle = -mod_force;
            //Debug.Log(yaw_angle);

            //大域回転部（一定速度＋フェード）
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                fade_in = true;
                fade_out = false;
            }
            //FixedUpdeteでの回転処理開始
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                fixed_L_flag = true;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                fixed_R_flag = true;
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                fixed_U_flag = true;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                fixed_D_flag = true;
            }
            //FixedUpdeteでの回転処理終了
            if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow))
            {
                fixed_L_flag = fixed_R_flag = fixed_U_flag = fixed_D_flag = false;
                fade_in = false;
                fade_out = true;
            }

            //姿勢の決定
            newAngle.y = yaw_angle + rotation_angle;     //首回転角＋旋回角度
            newAngle.z = 0;                                              //首回転角roll初期化
            newAngle.x = 0;                                              //首回転角pitch
            VReye.gameObject.transform.localEulerAngles = newAngle;

            //歩行動作（長押し・短押し対応版）
            if (Input.GetKey(KeyCode.Space))
            {
                currentTime += Time.deltaTime;  //長押しの時間カウント
                if (first_step_flag == true)    //最初に押した瞬間は一歩進む（短押し用）
                {
                    moveForward_D();
                    first_step_flag = false;
                }
                else
                {
                    if (currentTime > span)     //長押しで一定時間ごとに前進
                    {
                        moveForward_D();
                        currentTime = 0f;
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.Space))  //ボタン離したらフラグ戻す（短押し用）
            {
                first_step_flag = true;
                currentTime = 0f;
            }
        }
        //END 3.///////////////////////////////////////////////////////////////////////////////////////


        //4．局所回転：ジャイロ / 大域回転：なし ///////////////////////////////////////
        if (f4_flag == true)
        {
            //局所回転角度＋ローパスフィルタ
            yaw_angle = pre_yaw * filter_gain + yaw_angle * (1 - filter_gain);
            pre_yaw = yaw_angle;
            pitch_angle = pre_pitch * filter_gain + pitch_angle * (1 - filter_gain);
            pre_pitch = pitch_angle;

            //姿勢の決定
            newAngle.y = yaw_angle + rotation_angle;            //局所回転角度＋大域回転角度
            newAngle.z = 0;                                     //首回転角roll初期化
            newAngle.x = pitch_angle + rotation_angle_pitch;    //局所回転角度＋大域回転角度
            VReye.gameObject.transform.localEulerAngles = newAngle;     //姿勢の代入
            //
            //歩行動作（長押し・短押し対応版）
            if (Input.GetKey(KeyCode.Space))
            {
                currentTime += Time.deltaTime;  //長押しの時間カウント
                if (first_step_flag == true)    //最初に押した瞬間は一歩進む（短押し用）
                {
                    moveForward_D();
                    first_step_flag = false;
                }
                else
                {
                    if (currentTime > span)     //長押しで一定時間ごとに前進
                    {
                        moveForward_D();
                        currentTime = 0f;
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.Space))  //ボタン離したらフラグ戻す（短押し用）
            {
                first_step_flag = true;
                currentTime = 0f;
            }
        }
        //END 4.///////////////////////////////////////////////////////////////////////////////////////


        //5．局所回転：ジャイロ / 大域回転：空気圧閾値（Yawのみ） ///////////////////////////////////////
        if (f5_flag == true)
        {
            //局所回転角度＋ローパスフィルタ
            yaw_angle = pre_yaw * filter_gain + yaw_angle * (1 - filter_gain);
            pre_yaw = yaw_angle;
            pitch_angle = pre_pitch * filter_gain + pitch_angle * (1 - filter_gain);
            pre_pitch = pitch_angle;
            //
            //大域回転部（空気圧閾値）
            if (Math.Abs(force) > t_force)
            {
                //閾値を超えたらフェードアニメーション
                fade_in = true;
                fade_out = false;
                //回転開始
                if (force > 0)
                {
                    fixed_L_flag = true;
                }
                else
                {
                    fixed_R_flag = true;
                }
            }
            else
            {
                fixed_L_flag = false;
                fixed_R_flag = false;
                fade_in = false;
                fade_out = true;
            }
            //
            //姿勢の決定
            newAngle.y = yaw_angle + rotation_angle;            //局所回転角度＋大域回転角度
            newAngle.z = 0;                                     //首回転角roll初期化
            newAngle.x = pitch_angle + rotation_angle_pitch;    //局所回転角度＋大域回転角度
            VReye.gameObject.transform.localEulerAngles = newAngle;     //姿勢の代入
            //
            //歩行動作（長押し・短押し対応版）
            if (Input.GetKey(KeyCode.Space))
            {
                currentTime += Time.deltaTime;  //長押しの時間カウント
                if (first_step_flag == true)    //最初に押した瞬間は一歩進む（短押し用）
                {
                    moveForward_D();
                    first_step_flag = false;
                }
                else
                {
                    if (currentTime > span)     //長押しで一定時間ごとに前進
                    {
                        moveForward_D();
                        currentTime = 0f;
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.Space))  //ボタン離したらフラグ戻す（短押し用）
            {
                first_step_flag = true;
                currentTime = 0f;
            }
        }
        //END 5.///////////////////////////////////////////////////////////////////////////////////////


        //6．局所回転：空気圧枕 / 大域回転：空気圧閾値（Yawのみ） ///////////////////////////////////////
        if (f6_flag == true)
        {
            //局所回転角の取得
            force_to_angle = 0.4f * force;       //y=ax : force100 / yaw40
            //平均値フィルタ
            for (int i = AVE_NUM - 1; i > 0; i--)
            {
                list_force[i] = list_force[i - 1];
            }
            list_force[0] = force_to_angle;
            for (int i = 0; i < AVE_NUM; i++)
            {
                ave_force += list_force[i];
            }
            ave_force = (float)(ave_force / AVE_NUM);
            //ローパスフィルタ
            mod_force = pre_force * filter_gain + ave_force * (1 - filter_gain);
            pre_force = mod_force;
            //ここで角度代入
            yaw_angle = -mod_force;
            //Debug.Log(yaw_angle);

            //大域回転部（空気圧閾値）
            if (Math.Abs(force) > t_force)
            {
                //閾値を超えたらフェードアニメーション
                fade_in = true;
                fade_out = false;
                //回転開始
                if (force > 0)
                {
                    fixed_L_flag = true;
                }
                else
                {
                    fixed_R_flag = true;
                }
            }
            else
            {
                fixed_L_flag = false;
                fixed_R_flag = false;
                fade_in = false;
                fade_out = true;
            }
            //

            //姿勢の決定
            newAngle.y = yaw_angle + rotation_angle;     //首回転角＋旋回角度
            newAngle.z = 0;                                              //首回転角roll初期化
            newAngle.x = 0;                                              //首回転角pitch
            VReye.gameObject.transform.localEulerAngles = newAngle;

            //歩行動作（長押し・短押し対応版）
            if (Input.GetKey(KeyCode.Space))
            {
                currentTime += Time.deltaTime;  //長押しの時間カウント
                if (first_step_flag == true)    //最初に押した瞬間は一歩進む（短押し用）
                {
                    moveForward_D();
                    first_step_flag = false;
                }
                else
                {
                    if (currentTime > span)     //長押しで一定時間ごとに前進
                    {
                        moveForward_D();
                        currentTime = 0f;
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.Space))  //ボタン離したらフラグ戻す（短押し用）
            {
                first_step_flag = true;
                currentTime = 0f;
            }
        }
        //END 6.///////////////////////////////////////////////////////////////////////////////////////


        //7．局所回転：ジャイロ / 大域回転：ヘッドロック ///////////////////////////////////////
        if (f7_flag == true)
        {
            //局所回転角度＋ローパスフィルタ
            yaw_angle = pre_yaw * filter_gain + yaw_angle * (1 - filter_gain);
            pre_yaw = yaw_angle;
            pitch_angle = pre_pitch * filter_gain + pitch_angle * (1 - filter_gain);
            pre_pitch = pitch_angle;
            //
            //連続ヘッドロック//////////////////////////////
            //if (Input.GetKeyDown(KeyCode.Z))
            //{
            //    rotation_angle += yaw_angle * 2;
            //}
            //if (Input.GetKey(KeyCode.Z))
            //{
            //    yaw_angle = -yaw_angle;
            //}
            //if (Input.GetKeyUp(KeyCode.Z))
            //{
            //    rotation_angle -= yaw_angle * 2;
            //}
            if (Input.GetKeyDown(KeyCode.Z))    //ヘッドロックのONトリガー
            {
                rotation_angle += yaw_angle * 2;
                headlock_flag = true;
                fade_in = true;                 //ONトリガーでフェードイン
                fade_out = false;
            }
            if (headlock_flag == true)
            {
                yaw_angle = -yaw_angle;
                if (yaw_angle * pre_angle < 0) //OFFトリガー（正面を向く）
                {
                    rotation_angle -= yaw_angle * 2;
                    temp_yaw_angle = -yaw_angle;
                    headlock_flag = false;
                    notrack_flag = true;
                    fade_in = false;            //OFFトリガーでフェードアウト
                    fade_out = true;
                }
                pre_angle = yaw_angle;
            }
            else
            {
                pre_angle = -yaw_angle;
            }
            //Debug.Log("yaw_angle" + yaw_angle + " rotation_angle" + rotation_angle + " headlock_flag" + headlock_flag + " notrack_flag" + notrack_flag);
            //
            if (notrack_flag == false)
            {
                newAngle.y = yaw_angle + rotation_angle;        //首回転角＋初期調整角度＋旋回角度
                newAngle.z = 0;                                 //首回転角roll初期化
                newAngle.x = pitch_angle;                       //首回転角pitch
                VReye.gameObject.transform.localEulerAngles = newAngle;
            }
            else
            {
                //トラッキング無効時（正面に戻ったとき一定時間停止）
                newAngle.y = temp_yaw_angle + rotation_angle;     //首回転角＋初期調整角度＋旋回角度
                newAngle.z = 0;                                   //首回転角roll初期化
                newAngle.x = pitch_angle;                         //首回転角pitch
                VReye.gameObject.transform.localEulerAngles = newAngle;
                currentTime_notrack += Time.deltaTime;  //時間カウント
                if (currentTime_notrack > span_notrack)
                {
                    rotation_angle -= yaw_angle;    //トラッキング無効時の角度変化を補正
                    currentTime_notrack = 0f;
                    notrack_flag = false;
                }
            }

            //
            //歩行動作（長押し・短押し対応版）
            if (Input.GetKey(KeyCode.Space))
            {
                currentTime += Time.deltaTime;  //長押しの時間カウント
                if (first_step_flag == true)    //最初に押した瞬間は一歩進む（短押し用）
                {
                    moveForward_D();
                    first_step_flag = false;
                }
                else
                {
                    if (currentTime > span)     //長押しで一定時間ごとに前進
                    {
                        moveForward_D();
                        currentTime = 0f;
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.Space))  //ボタン離したらフラグ戻す（短押し用）
            {
                first_step_flag = true;
                currentTime = 0f;
            }
        }
        //END 7.///////////////////////////////////////////////////////////////////////////////////////


        //8．局所回転：ジャイロ / 大域回転：ヘッドロック（注視物体推定，注視角度補正なし） ///////////////////////////////////////
        if (f8_flag == true)
        {
            //局所回転角度＋ローパスフィルタ
            yaw_angle = pre_yaw * filter_gain + yaw_angle * (1 - filter_gain);
            pre_yaw = yaw_angle;
            pitch_angle = pre_pitch * filter_gain + pitch_angle * (1 - filter_gain);
            pre_pitch = pitch_angle;
            //
            //注視トリガー
            if (gazing_flag == true)            //gazing_flag：注視中
            {
                //注視が外れるとリセット
                if (Math.Abs(gaze_angle - yaw_angle) > gaze_limit_angle)
                {
                    gazing_flag = false;
                }
                //注視時間カウント，時間がgaze_spanを超えるとgaze_on_flagでトリガー
                gaze_time += Time.deltaTime;
                if (gaze_time > gaze_span)
                {
                    gaze_on_flag = true;
                    gazing_flag = false;
                    gaze_time = 0f;
                }
            }
            else if (gazing_flag == false && headlock_flag == false && Math.Abs(yaw_angle) > no_gaze_angle)     //注視中でなく，ヘッドロック中でなく，注視領域に入っているとき
            {
                gaze_angle = yaw_angle;     //注視角度
                gaze_time = 0f;
                gazing_flag = true;
            }
            //
            if (gaze_on_flag == true)    //ヘッドロックのONトリガー
            {
                rotation_angle += yaw_angle * 2;
                headlock_flag = true;           //headlock_flag ヘッドロック中のフラグ
                gaze_on_flag = false;
                fade_in = true;
                fade_out = false;
            }
            if (headlock_flag == true)
            {
                yaw_angle = -yaw_angle;
                if (yaw_angle * pre_angle < 0) //OFFトリガー（正面を向く）
                {
                    //rotation_angle -= yaw_angle * 2;
                    //temp_yaw_angle = -yaw_angle;
                    headlock_flag = false;      //ヘッドロック終了
                    notrack_flag = true;        //トラッキング無効にする
                    fade_in = false;            //OFFトリガーでフェードアウト
                    fade_out = true;
                }
                pre_angle = yaw_angle;
            }
            else
            {
                pre_angle = -yaw_angle;
            }
            //
            if (notrack_flag == false)
            {
                //通常時
                newAngle.y = yaw_angle + rotation_angle;        //首回転角＋初期調整角度＋旋回角度
                newAngle.z = 0;                                 //首回転角roll初期化
                newAngle.x = pitch_angle;                       //首回転角pitch
                VReye.gameObject.transform.localEulerAngles = newAngle;
            }
            else
            {
                //トラッキング無効時（正面に戻ったとき一定時間停止）
                newAngle.y = temp_yaw_angle + rotation_angle;     //首回転角＋初期調整角度＋旋回角度
                newAngle.z = 0;                                   //首回転角roll初期化
                newAngle.x = pitch_angle;                         //首回転角pitch
                VReye.gameObject.transform.localEulerAngles = newAngle;
                currentTime_notrack += Time.deltaTime;  //時間カウント
                if (currentTime_notrack > span_notrack)
                {
                    rotation_angle -= yaw_angle;    //トラッキング無効時の角度変化を補正
                    currentTime_notrack = 0f;
                    notrack_flag = false;
                }
            }
            //
            //歩行動作（長押し・短押し対応版）
            if (Input.GetKey(KeyCode.Space))
            {
                currentTime += Time.deltaTime;  //長押しの時間カウント
                if (first_step_flag == true)    //最初に押した瞬間は一歩進む（短押し用）
                {
                    moveForward_D();
                    first_step_flag = false;
                }
                else
                {
                    if (currentTime > span)     //長押しで一定時間ごとに前進
                    {
                        moveForward_D();
                        currentTime = 0f;
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.Space))  //ボタン離したらフラグ戻す（短押し用）
            {
                first_step_flag = true;
                currentTime = 0f;
            }
        }
        //END 8.///////////////////////////////////////////////////////////////////////////////////////

        //9．局所回転：ジャイロ / 大域回転：ヘッドロック（注視物体推定，注視角度ゲイン補正あり） ///////////////////////////////////////
        if (f9_flag == true)
        {
            //局所回転角度＋ローパスフィルタ
            yaw_angle = pre_yaw * filter_gain + yaw_angle * (1 - filter_gain);
            pre_yaw = yaw_angle;
            pitch_angle = pre_pitch * filter_gain + pitch_angle * (1 - filter_gain);
            pre_pitch = pitch_angle;
            //
            //注視トリガー
            if (gazing_flag == true)            //gazing_flag：注視中
            {
                //注視が外れるとリセット
                if (Math.Abs(gaze_angle - yaw_angle) > gaze_limit_angle)
                {
                    gazing_flag = false;
                }
                //注視時間カウント，時間がgaze_spanを超えるとgaze_on_flagでトリガー
                gaze_time += Time.deltaTime;
                if (gaze_time > gaze_span)
                {
                    gaze_on_flag = true;
                    gazing_flag = false;
                    gaze_time = 0f;
                }
            }
            else if (gazing_flag == false && headlock_flag == false && Math.Abs(yaw_angle) > no_gaze_angle)     //注視中でなく，ヘッドロック中でなく，注視領域に入っているとき
            {
                gaze_angle = yaw_angle;     //注視角度
                gaze_time = 0f;
                gazing_flag = true;
            }
            //Debug.Log("gazing_flag " + gazing_flag + " gaze_on_flag " + gaze_on_flag);
            //
            if (gaze_on_flag == true)    //ヘッドロックのONトリガー
            {
                rotation_angle += yaw_angle * 2;
                mod_angle = yaw_angle;          //補正用角度
                headlock_flag = true;           //headlock_flag ヘッドロック中のフラグ
                gaze_on_flag = false;
                fade_in = true;
                fade_out = false;
            }
            if (headlock_flag == true)
            {
                yaw_angle = -((yaw_angle - mod_angle) * headlock_gain + mod_angle);     //戻すときゲインかける（mod_angleは今向いてる方向を0度にしてゲインをかけるため）
                gyro_yaw_angle = (-yaw_angle - mod_angle) / headlock_gain + mod_angle;   //ゲイン補正かける前の値＝実際のジャイロの角度（正面計算用）
                //Debug.Log(" yaw_angle " + yaw_angle + " gyro_angle " + gyro_yaw_angle);
                if (gyro_yaw_angle * pre_angle < 0)     //OFFトリガー（ジャイロ座標で正面を向く）
                {
                    //rotation_angle -= yaw_angle * 2;  
                    rotation_angle += yaw_angle;    //ゲイン分の補正
                    //Debug.Log(" yaw_angle " + yaw_angle + " mod_angle " + mod_angle);
                    //temp_yaw_angle = -yaw_angle;
                    headlock_flag = false;      //ヘッドロック終了
                    notrack_flag = true;        //トラッキング無効にする
                    fade_in = false;            //OFFトリガーでフェードアウト
                    fade_out = true;
                }
                pre_angle = (-yaw_angle - mod_angle) / headlock_gain + mod_angle;
            }
            else
            {
                pre_angle = yaw_angle;
            }
            //
            //Debug.Log("yaw_angle " + yaw_angle + " headlock_flag " + headlock_flag);
            //
            if (notrack_flag == false)
            {
                //通常時
                newAngle.y = yaw_angle + rotation_angle;        //首回転角＋旋回角度
                newAngle.z = 0;                                 //首回転角roll初期化
                newAngle.x = pitch_angle;                       //首回転角pitch
                VReye.gameObject.transform.localEulerAngles = newAngle;
            }
            else
            {
                //トラッキング無効時（正面に戻ったとき一定時間停止）
                //Debug.Log(" temp_yaw_angle " + temp_yaw_angle + " rotation_angle " + rotation_angle);
                newAngle.y = temp_yaw_angle + rotation_angle;     //首回転角＋旋回角度
                newAngle.z = 0;                                   //首回転角roll初期化
                newAngle.x = pitch_angle;                         //首回転角pitch
                VReye.gameObject.transform.localEulerAngles = newAngle;
                currentTime_notrack += Time.deltaTime;  //時間カウント
                if (currentTime_notrack > span_notrack)
                {
                    rotation_angle -= yaw_angle;    //トラッキング無効時の角度変化を補正
                    currentTime_notrack = 0f;
                    notrack_flag = false;
                }
            }
            //
            //歩行動作（長押し・短押し対応版）
            if (Input.GetKey(KeyCode.Space))
            {
                currentTime += Time.deltaTime;  //長押しの時間カウント
                if (first_step_flag == true)    //最初に押した瞬間は一歩進む（短押し用）
                {
                    moveForward_D();
                    first_step_flag = false;
                }
                else
                {
                    if (currentTime > span)     //長押しで一定時間ごとに前進
                    {
                        moveForward_D();
                        currentTime = 0f;
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.Space))  //ボタン離したらフラグ戻す（短押し用）
            {
                first_step_flag = true;
                currentTime = 0f;
            }
        }
        //END 9.///////////////////////////////////////////////////////////////////////////////////////

        //10．局所回転：ジャイロ / 大域回転：ジャイロ閾値（首戻しあり） ///////////////////////////////////////
        if (f10_flag == true)
        {
            //局所回転角度＋ローパスフィルタ
            yaw_angle = pre_yaw * filter_gain + yaw_angle * (1 - filter_gain);
            pre_yaw = yaw_angle;
            pitch_angle = pre_pitch * filter_gain + pitch_angle * (1 - filter_gain);
            pre_pitch = pitch_angle;
            //頭部制御部
            if (Math.Abs(yaw_angle) > Math.Abs(move_angle))    //頭部閾値でのONトリガー
            {
                head_move_flag = true;  //頭部制御中フラグ
                //閾値を超えたらフェードアニメーション
                fade_in = true;
                fade_out = false;
                //回転開始
                if (yaw_angle < 0)
                {
                    temp_yaw_angle = -move_angle;    //閾値で角度固定
                    fixed_L_flag = true;
                }
                else
                {
                    temp_yaw_angle = move_angle;     //閾値で角度固定
                    fixed_R_flag = true;
                }
                notrack_flag = true;    //トラッキング無効
            }

            if (head_move_flag == true)
            {
                if(Math.Abs(yaw_angle) < Math.Abs(move_angle))
                {
                    fixed_L_flag = false;
                    fixed_R_flag = false;
                    if (yaw_angle * pre_angle < 0) //トラッキング無効解除トリガー（正面を向く）
                    {
                        head_move_flag = false; //頭部制御中フラグOFF
                        rotation_angle += temp_yaw_angle;   //トラッキング無効時に回転した分を補正
                        notrack_flag = false;   //トラッキング有効
                        fade_in = false;
                        fade_out = true;
                    }
                }
            }
            pre_angle = yaw_angle;

            if (notrack_flag == false)
            {
                //通常時
                newAngle.y = yaw_angle + rotation_angle;        //首回転角＋初期調整角度＋旋回角度
                newAngle.z = 0;                                 //首回転角roll初期化
                newAngle.x = pitch_angle;                       //首回転角pitch
                VReye.gameObject.transform.localEulerAngles = newAngle;
            }
            else
            {
                //トラッキング無効時（正面に戻ったとき一定時間停止）
                newAngle.y = temp_yaw_angle + rotation_angle;     //首回転角＋初期調整角度＋旋回角度
                newAngle.z = 0;                                   //首回転角roll初期化
                newAngle.x = pitch_angle;                         //首回転角pitch
                VReye.gameObject.transform.localEulerAngles = newAngle;
            }
            //
            //歩行動作（長押し・短押し対応版）
            if (Input.GetKey(KeyCode.Space))
            {
                currentTime += Time.deltaTime;  //長押しの時間カウント
                if (first_step_flag == true)    //最初に押した瞬間は一歩進む（短押し用）
                {
                    moveForward_D();
                    first_step_flag = false;
                }
                else
                {
                    if (currentTime > span)     //長押しで一定時間ごとに前進
                    {
                        moveForward_D();
                        currentTime = 0f;
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.Space))  //ボタン離したらフラグ戻す（短押し用）
            {
                first_step_flag = true;
                currentTime = 0f;
            }
        }
        //END 10.///////////////////////////////////////////////////////////////////////////////////////
    }

    //離散移動の関数
    private void moveForward_D()
    {
        VReye.transform.position = VReye.transform.position + new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z) * Time.deltaTime * stride;
    }

    private void moveBackward_D()
    {
        VReye.transform.position = VReye.transform.position + new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z) * Time.deltaTime * stride * (-1);
    }

    //フレームレート依存の動作をここで行う
    void FixedUpdate()
    {
        if (fixed_L_flag == true)
        {
            rotation_angle -= rotation_speed;
        }

        if (fixed_R_flag == true)
        {
            rotation_angle += rotation_speed;
        }

        if (fixed_U_flag == true)
        {
            rotation_angle_pitch -= rotation_speed;
        }

        if (fixed_D_flag == true)
        {
            rotation_angle_pitch += rotation_speed;
        }
    }

    //動作切り替え用関数
    void FlagDown()
    {
        f1_flag = f2_flag = f3_flag = f4_flag = f5_flag = f6_flag = f7_flag = f8_flag = f9_flag = f10_flag = false;
    }
}
