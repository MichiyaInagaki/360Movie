using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using UnityEngine.UI;

public class VReyeController_UDP_roll : MonoBehaviour
{
    //Publicオブジェクト（格納）
    public GameObject VReye;
    public Camera mainCamera;
    public GameObject UDP_Data_Receiver;
    //
    //各種パラメータ
    public int stride = 50;               //離散移動の歩幅
    public float rotation_speed = 0.5f;   //旋回スピード
    //
    //Privateオブジェクト
    private Vector3 newAngle = new Vector3(0, 0, 0);        //カメラ姿勢
    private Vector3 Initial_Angle = new Vector3(0, 0, 0);   //初期カメラ姿勢
    //
    private float yaw_angle = 0.0f;                 //局所回転角度
    private float pitch_angle = 0.0f;               
    private float roll_angle = 0.0f;                
    private float rotation_angle = 0.0f;            //大域回転角度
    private float rotation_angle_pitch = 0.0f;      
    //
    //ローパスフィルタ
    private float filter_gain = 0.75f;              //default: 0.75
    private float pre_yaw = 0.0f;
    private float pre_pitch = 0.0f;
    private float pre_roll = 0.0f;
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
        yaw_angle = UDP_Data_Receiver.GetComponent<UDP_roll>().yaw_val;
        pitch_angle = UDP_Data_Receiver.GetComponent<UDP_roll>().pitch_val;
        roll_angle = UDP_Data_Receiver.GetComponent<UDP_roll>().roll_val;

        //ローパスフィルタ
        //yaw_angle = pre_yaw * filter_gain + yaw_angle * (1 - filter_gain);
        //pre_yaw = yaw_angle;
        //pitch_angle = pre_pitch * filter_gain + pitch_angle * (1 - filter_gain);
        //pre_pitch = pitch_angle;
        //roll_angle = pre_roll * filter_gain + roll_angle * (1 - filter_gain);
        //pre_roll = roll_angle;

        //大域回転部（一定速度＋フェード）
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
        }


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

        //各種処理
        //1．局所回転：なし / 大域回転：キー操作Yaw＋Pitch ///////////////////////////////////////
        if (f1_flag == true)
        {
            //姿勢の決定
            newAngle.y = rotation_angle;          //局所回転角度＋大域回転角度
            newAngle.z = 0;                       //首回転角roll初期化
            newAngle.x = rotation_angle_pitch;    //局所回転角度＋大域回転角度
            VReye.gameObject.transform.localEulerAngles = newAngle;     //姿勢の代入
        }

        //2．補正なし：	仮想世界の水平線＝物理世界の水平線，Pitch補正なし ///////////////////////////////////////
        if (f2_flag == true)
        {
            //姿勢の決定
            newAngle.y = yaw_angle + rotation_angle;            //局所回転角度＋大域回転角度
            newAngle.z = roll_angle;                            //roll補正
            newAngle.x = pitch_angle + rotation_angle_pitch;    //局所回転角度＋大域回転角度
            VReye.gameObject.transform.localEulerAngles = newAngle;     //姿勢の代入
        }

        //3．Roll補正のみ：	仮想世界の水平線＝目の水平線，Pitch補正なし ///////////////////////////////////////
        if (f3_flag == true)
        {
            //姿勢の決定
            newAngle.y = yaw_angle + rotation_angle;            //局所回転角度＋大域回転角度
            newAngle.z = 0;                                     //首回転角roll初期化
            newAngle.x = pitch_angle + rotation_angle_pitch;    //局所回転角度＋大域回転角度
            VReye.gameObject.transform.localEulerAngles = newAngle;     //姿勢の代入
        }

        //4．Pitch補正のみ：	仮想世界の水平線＝物理世界の水平線，Pitch補正あり /////////////////////////////////
        if (f4_flag == true)
        {
            //姿勢の決定
            newAngle.y = yaw_angle + rotation_angle;            //局所回転角度＋大域回転角度
            newAngle.z = roll_angle;                            //roll補正
            newAngle.x = rotation_angle_pitch;    //局所回転角度＋大域回転角度
            VReye.gameObject.transform.localEulerAngles = newAngle;     //姿勢の代入
        }

        //5．Roll＋Pitch補正：	仮想世界の水平線＝目の水平線，Pitch補正あり ///////////////////////////////////////
        if (f5_flag == true)
        {
            //姿勢の決定
            newAngle.y = yaw_angle + rotation_angle;            //局所回転角度＋大域回転角度
            newAngle.z = 0;                                     //首回転角roll初期化
            newAngle.x = rotation_angle_pitch;    //局所回転角度＋大域回転角度
            VReye.gameObject.transform.localEulerAngles = newAngle;     //姿勢の代入
        }
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
        f1_flag = f2_flag = f3_flag = f4_flag = f5_flag = false;
    }
}
