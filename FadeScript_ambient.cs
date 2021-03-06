﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeScript_ambient : MonoBehaviour
{
    public GameObject VReyeController;
    public float speed = 0.04f;  //透明化の速さ
    public bool fade_mode_on = true;    //フェードかけるかどうか
    public GameObject Arrow_L;
    public GameObject Arrow_R;
    public GameObject Data_Receiver;
    //
    private float alfa;    //A値を操作するための変数
    private float red, green, blue;    //RGBを操作するための変数
    private float max_fade = 0.9f;      //最大のフェード
    private bool fadein_flag = false;   //フェードイン
    private bool fadeout_flag = false;  //フェードアウト
    private float yaw_angle;


    // Start is called before the first frame update
    void Start()
    {
        //Panelの色を取得
        red = GetComponent<Image>().color.r;
        green = GetComponent<Image>().color.g;
        blue = GetComponent<Image>().color.b;
    }

    // Update is called once per frame
    void Update()
    {
        //
        fadein_flag = VReyeController.GetComponent<VReyeController_ambient>().fade_in;
        fadeout_flag = VReyeController.GetComponent<VReyeController_ambient>().fade_out;
        yaw_angle = Data_Receiver.GetComponent<receive_data_gyro_airpress>().yaw_val;
        //フェード機能のオンオフ
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (fade_mode_on == true)
            {
                fade_mode_on = false;
            }
            else if (fade_mode_on == false)
            {
                fade_mode_on = true;
            }
        }
        //
        GetComponent<Image>().color = new Color(red, green, blue, alfa);

    }

    //フレームレート依存の動作をここで行う
    void FixedUpdate()
    {
        if (fade_mode_on == true)
        {
            if (fadein_flag == true)
            {
                if (alfa < max_fade)
                {
                    alfa += speed;
                }
                else
                {
                    alfa = max_fade;
                    if (yaw_angle > 30)
                    {
                        Arrow_R.SetActive(true);
                    }
                    else if (yaw_angle < -30)
                    {
                        Arrow_L.SetActive(true);
                    }
                }
            }

            if (fadeout_flag == true)
            {
                Arrow_L.SetActive(false);
                Arrow_R.SetActive(false);
                if (alfa > 0)
                {
                    alfa -= speed;
                }
                else
                {
                    alfa = 0;
                }
            }
        }
        else if (fade_mode_on == false)
        {
            alfa = 0;   //フェード機能オフ
        }

    }
}
