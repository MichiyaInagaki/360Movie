using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MASK_Controller1_2 : MonoBehaviour
{
    public GameObject VReyeController;
    private bool fade_mode_on = true;       //フェードオンオフ（trueが黒画面）
    private float alfa;    //A値を操作するための変数
    private float red, green, blue;    //RGBを操作するための変数

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
        fade_mode_on = VReyeController.GetComponent<VReyeController_UDP_exp1_2>().fade_on;

        if (fade_mode_on == true)
        {
            alfa = 255;
        }
        else
        {
            alfa = 0;
        }
        //
        GetComponent<Image>().color = new Color(red, green, blue, alfa);
    }
}
