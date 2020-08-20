using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

public class Stereophonic_Sound : MonoBehaviour
{
    public AudioClip sound;
    private Vector3 sound_source = new Vector3(-2, 0, -2);        //音源位置
    private float r = 2.0f;         //音源までの距離
    private float angle = 0.0f;     //音源の方向（極座標系）
    private float rand = 0.0f;      //テスト音源用
    // センサ値取得用
    public GameObject Data_Receiver;
    // 値格納用
    private float yaw_angle = 0.0f;                 //局所回転角度
    private float pitch_angle = 0.0f;               //局所回転角度
    private float force;                            //空気圧の値（左右）
    private float yaw_velocity = 0.0f;              //角速度
    private float yaw_accel = 0.0f;                 //角加速度
    //書き出し用
    private string Path = @"C:\Users\inaga\OneDrive\デスクトップ\exe\csv\accel\";
    private string filePath;
    private bool in_writing_flag = false;   //書き出し中
    private float time = 0.0f;  //時刻
    //
    private float span = 10.0f;                      //音を鳴らすまでの時間
    private float currentTime = 0f;                 //時間カウント用
    private bool sound_flag = false;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //シリアル通信から値を取得
        yaw_angle = Data_Receiver.GetComponent<receive_data_accel>().yaw_val;
        pitch_angle = Data_Receiver.GetComponent<receive_data_accel>().pitch_val;
        force = Data_Receiver.GetComponent<receive_data_accel>().f_val;
        yaw_velocity = Data_Receiver.GetComponent<receive_data_accel>().yaw_velocity;
        yaw_accel = Data_Receiver.GetComponent<receive_data_accel>().yaw_accel;


        //動作切り替え
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            angle = 0.0f;
            init_data();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            angle = 180.0f;
            init_data();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            angle = 225.0f;
            init_data();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            angle = 270.0f;
            init_data();
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            angle = 315.0f;
            init_data();
        }
        //テスト音源
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            playsound(0.0f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            playsound(180.0f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            playsound(225.0f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            playsound(270.0f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            playsound(315.0f);
        }
        //計測終了
        if (Input.GetKeyDown(KeyCode.F12))
        {
            in_writing_flag = false;
        }


        if (in_writing_flag == true)
        {
            time += Time.deltaTime;
            // ファイル書き込み
            string[] str = { time.ToString(), yaw_angle.ToString(), pitch_angle.ToString(), force.ToString(), yaw_velocity.ToString(), yaw_accel.ToString() };
            string str2 = string.Join(",", str);
            filePath = Path + ((int)angle).ToString() + ".csv";
            File.AppendAllText(filePath, str2 + "\n");
            // 時間カウント
            if (sound_flag == false)
            {
                currentTime += Time.deltaTime;
                if (currentTime > span)
                {
                    playsound(angle);   //音鳴らす
                    sound_flag = true;
                }
            }
        }


    }

    //指定した角度（極座標系）から音を鳴らす
    void playsound(float source_angle)
    {
        //ラジアンに変換
        source_angle = source_angle * (Mathf.PI / 180.0f);
        //場所を極座標→直交座標に変換
        sound_source = new Vector3(r * Mathf.Cos(source_angle), 0, r * Mathf.Sin(source_angle));
        //音声再生
        AudioSource.PlayClipAtPoint(sound, sound_source);
    }

    void init_data()
    {
        in_writing_flag = true;
        sound_flag = false;
        currentTime = 0.0f;
        time = 0.0f;
    }
}
