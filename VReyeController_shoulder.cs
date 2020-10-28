using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using UnityEngine.UI;

public class VReyeController_shoulder : MonoBehaviour
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
    private float force_to_angle;                   //空気圧の値を角度に変換した値
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
        yaw_angle = Data_Receiver.GetComponent<receive_data_gyro_airpress2>().yaw_val;
        pitch_angle = Data_Receiver.GetComponent<receive_data_gyro_airpress2>().pitch_val;
        force = Data_Receiver.GetComponent<receive_data_gyro_airpress2>().f_val;

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
        }
        //END 2.///////////////////////////////////////////////////////////////////////////////////////

        //3．局所回転：空気圧枕 / 大域回転：キー操作Yaw＋Pitch ///////////////////////////////////////
        if (f3_flag == true)
        {
            //局所回転角の取得
            force_to_angle = 0.05f * force;       //y=ax : 
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
        }
        //END 3.///////////////////////////////////////////////////////////////////////////////////////

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
        f1_flag = f2_flag = f3_flag = f4_flag = false;
    }
}
