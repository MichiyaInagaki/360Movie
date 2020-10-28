using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Delete_Image : MonoBehaviour
{
    private bool fade_mode_on = true;    
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
        //表示のオンオフ   
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
