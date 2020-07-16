


//！！！Updateで毎回アクセスすると，リクエスト回数が無限に増えるので使わない！！！



using UnityEngine;
using System.Collections;

public class StreetView_SceneMove : MonoBehaviour
{

    public double heading = 0.0;
    public double pitch = 0.0;
    public string APIkey;
    public float rotation_speed = 0.5f;   //旋回スピード
    //
    private int width = 640;
    private int height = 480;
    //
    private double longitude = 139.667431;
    private double latitude = 35.697408;
    //
    //固定フレームレートで回転させるためのフラグ
    private bool fixed_L_flag = false;
    private bool fixed_R_flag = false;
    private bool fixed_U_flag = false;
    private bool fixed_D_flag = false;
    //
    //Publicオブジェクト（送信）
    public bool fade_in = false;
    public bool fade_out = false;
    //

    // Use this for initialization
    void Start()
    {
        StartCoroutine(GetStreetViewImage(latitude, longitude, heading, pitch));
    }

    void Update()
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
        //ストリートビューの呼び出し
        //StartCoroutine(GetStreetViewImage(latitude, longitude, heading, pitch));
    }

        private IEnumerator GetStreetViewImage(double latitude, double longitude, double heading, double pitch)
    {
        string url = "http://maps.googleapis.com/maps/api/streetview?" + "size=" + width + "x" + height + "&location=" + latitude + "," + longitude + "&heading=" + heading + "&pitch=" + pitch + "&fov=90&sensor=false&key=" + APIkey;
        WWW www = new WWW(url);
        yield return www;

        GetComponent<Renderer>().material.mainTexture = www.texture;
    }

    //フレームレート依存の動作をここで行う
    void FixedUpdate()
    {
        if (fixed_L_flag == true)
        {
            heading -= rotation_speed;
        }

        if (fixed_R_flag == true)
        {
            heading += rotation_speed;
        }

        if (fixed_U_flag == true)
        {
            pitch += rotation_speed;
        }

        if (fixed_D_flag == true)
        {
            pitch -= rotation_speed;
        }
    }
}