using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class autoMove : MonoBehaviour
{

    // 目的地に着いたかどうか
    private bool isReachTargetPosition;
    // 目的地
    private Vector3 targetPosition;

    // 移動スピード
    private const float SPEED = 0.075f;     //default 0.075

    // Θ下限上限 rad = deg*π/180
    private const float T_MIN_MOVE_RANGE = 70.0f * (Mathf.PI / 180.0f);
    private const float T_MAX_MOVE_RANGE = 110.0f * (Mathf.PI / 180.0f);
    //private const float T_MIN_MOVE_RANGE = 90.0f * (Mathf.PI / 180.0f);
    //private const float T_MAX_MOVE_RANGE = 95.0f * (Mathf.PI / 180.0f);
    // φ下限上限 deg
    private const float P_MIN_MOVE_RANGE = -60.0f * (Mathf.PI / 180.0f);
    private const float P_MAX_MOVE_RANGE = 240.0f * (Mathf.PI / 180.0f);
    //private const float P_MIN_MOVE_RANGE = 180.0f * (Mathf.PI / 180.0f);
    //private const float P_MAX_MOVE_RANGE = 225.0f * (Mathf.PI / 180.0f);
    // r
    private const float r = 10.0f;

    // ランダム角度格納用
    private float THETA;
    private float PHI;

    // 極座標変換後の x, y, z の値
    private float X_VALUE;
    private float Y_VALUE;
    private float Z_VALUE;

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
    private Vector3[] cal_vector = new Vector3[31];
    private float MIN_CAL_PHI = 0.0f * (Mathf.PI / 180.0f);     //最小のphi
    private float MAX_CAL_PHI = 180.0f * (Mathf.PI / 180.0f);   //最大のphi
    private float INIT_THETA = 90.0f * (Mathf.PI / 180.0f);     //初期位置
    private float INIT_PHI = 90.0f * (Mathf.PI / 180.0f);       //初期位置

    //定常動作用
    private Vector3[] reg_vector = new Vector3[31];
    private float MIN_REG_PHI = -60.0f * (Mathf.PI / 180.0f);    //最小のphi
    private float MAX_REG_PHI = 240.0f * (Mathf.PI / 180.0f);    //最大のphi
    //private float MIN_REG_PHI = 0.0f * (Mathf.PI / 180.0f);    //最小のphi
    //private float MAX_REG_PHI = 180.0f * (Mathf.PI / 180.0f);    //最大のphi
    //
    private bool start_flag = false;
    private bool start_flag2 = false;
    private bool init_flag = false;


    void Start()
    {
        this.isReachTargetPosition = false;
        decideTargetPotision();
        //初期化
        for (int i = 0; i < 10; i++)
        {
            interpolation_vector[i] = new Vector3(0, 0, 10);
        }
        targetPosition = new Vector3(0, 0, 10);
        //キャリブレーション用の初期化
        init_Cal();
        //定常動作用の初期化
        Reguler_Move();
    }

    void Update()
    {
        decideTargetPotision();

        // 操作選択
        if (Input.GetKeyDown(KeyCode.M))
        {
            init_flag = false;
            cal_flag = false;
            start_flag2 = false;
            start_flag = true;
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            cal_step = 0;
            init_flag = false;
            cal_flag = false;
            start_flag = false;
            start_flag2 = true;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            cal_step = 0;
            init_flag = false;
            start_flag = false;
            start_flag2 = false;
            cal_flag = true;
        }
        //F12キーで計測終了→初期位置に戻る
        if (Input.GetKeyDown(KeyCode.F12))
        {
            start_flag = false;
            start_flag2 = false;
            cal_flag = false;
            init_flag = true;
        }

        if (init_flag == true)
        {
            targetPosition = new Vector3(0, 0, 10);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, SPEED);
        }

        // 0. キャリブレーション
        if (cal_flag == true)
        {
            //初期位置→左→初期位置→右→初期位置
            transform.position = Vector3.MoveTowards(transform.position, cal_vector[cal_step], SPEED);
            if (transform.position == cal_vector[cal_step])
            {
                if (cal_step < 30)
                {
                    cal_step++;
                }
            }
        }

        // 1. 定常動作（左右のみ移動）
        if (start_flag2 == true)
        {
            //初期位置→左→初期位置→右→初期位置
            transform.position = Vector3.MoveTowards(transform.position, reg_vector[cal_step], SPEED);
            if (transform.position == reg_vector[cal_step])
            {
                if (cal_step < 30)
                {
                    cal_step++;
                }
            }
        }

        // 2. ランダムに蝶々を動かす
        if (start_flag == true)
        {
            //補間部分
            if (step < 10)
            {
                transform.position = Vector3.MoveTowards(transform.position, interpolation_vector[step], SPEED);
                if (transform.position == interpolation_vector[step])
                {
                    step++;
                }
            }
            else
            {
                // 最終地点へ
                // 現在地から目的地までSPEEDの速度で移動する
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, SPEED);
                // 目的地についたらisReachTargetPositionをtrueにする
                if (transform.position == targetPosition)
                {
                    isReachTargetPosition = true;
                    step = 0;
                }
            }
        }
    }


    // キャリブレーション用初期化
    private void init_Cal()
    {
        //初期位置から左のとき
        for (int i = 0; i < 10; i++)
        {
            interpolation_phi = INIT_PHI + ((MAX_CAL_PHI - INIT_PHI) / 10.0f) * i;
            //極座標変換した座標を格納（x,z,y）
            cal_vector[i] = new Vector3(r * Mathf.Sin(INIT_THETA) * Mathf.Cos(interpolation_phi), r * Mathf.Cos(INIT_THETA), r * Mathf.Sin(INIT_THETA) * Mathf.Sin(interpolation_phi));
        }
        //左から右のとき
        for (int i = 10; i < 20; i++)
        {
            interpolation_phi = MAX_CAL_PHI + ((MIN_CAL_PHI - MAX_CAL_PHI) / 10.0f) * (i - 10);
            //極座標変換した座標を格納（x,z,y）
            cal_vector[i] = new Vector3(r * Mathf.Sin(INIT_THETA) * Mathf.Cos(interpolation_phi), r * Mathf.Cos(INIT_THETA), r * Mathf.Sin(INIT_THETA) * Mathf.Sin(interpolation_phi));
        }
        //右から初期位置のとき
        for (int i = 20; i < 30; i++)
        {
            interpolation_phi = MIN_CAL_PHI + ((INIT_PHI - MIN_CAL_PHI) / 10.0f) * (i - 20);
            //極座標変換した座標を格納（x,z,y）
            cal_vector[i] = new Vector3(r * Mathf.Sin(INIT_THETA) * Mathf.Cos(interpolation_phi), r * Mathf.Cos(INIT_THETA), r * Mathf.Sin(INIT_THETA) * Mathf.Sin(interpolation_phi));
        }
        cal_vector[30] = new Vector3(r * Mathf.Sin(INIT_THETA) * Mathf.Cos(INIT_PHI), r * Mathf.Cos(INIT_THETA), r * Mathf.Sin(INIT_THETA) * Mathf.Sin(INIT_PHI));
    }


    // 定常動作用初期化
    private void Reguler_Move()
    {
        //初期位置から左のとき
        for (int i = 0; i < 10; i++)
        {
            interpolation_phi = INIT_PHI + ((MAX_REG_PHI - INIT_PHI) / 10.0f) * i;
            //極座標変換した座標を格納（x,z,y）
            reg_vector[i] = new Vector3(r * Mathf.Sin(INIT_THETA) * Mathf.Cos(interpolation_phi), r * Mathf.Cos(INIT_THETA), r * Mathf.Sin(INIT_THETA) * Mathf.Sin(interpolation_phi));
        }
        //左から右のとき
        for (int i = 10; i < 20; i++)
        {
            interpolation_phi = MAX_REG_PHI + ((MIN_REG_PHI - MAX_REG_PHI) / 10.0f) * (i - 10);
            //極座標変換した座標を格納（x,z,y）
            reg_vector[i] = new Vector3(r * Mathf.Sin(INIT_THETA) * Mathf.Cos(interpolation_phi), r * Mathf.Cos(INIT_THETA), r * Mathf.Sin(INIT_THETA) * Mathf.Sin(interpolation_phi));
        }
        //右から初期位置のとき
        for (int i = 20; i < 30; i++)
        {
            interpolation_phi = MIN_REG_PHI + ((INIT_PHI - MIN_REG_PHI) / 10.0f) * (i - 20);
            //極座標変換した座標を格納（x,z,y）
            reg_vector[i] = new Vector3(r * Mathf.Sin(INIT_THETA) * Mathf.Cos(interpolation_phi), r * Mathf.Cos(INIT_THETA), r * Mathf.Sin(INIT_THETA) * Mathf.Sin(interpolation_phi));
        }
        reg_vector[30] = new Vector3(r * Mathf.Sin(INIT_THETA) * Mathf.Cos(INIT_PHI), r * Mathf.Cos(INIT_THETA), r * Mathf.Sin(INIT_THETA) * Mathf.Sin(INIT_PHI));
    }


    // 目的地を設定する
    private void decideTargetPotision()
    {
        // まだ目的地についてなかったら（移動中なら）目的地を変えない
        if (!isReachTargetPosition)
        {
            return;
        }

        //ランダム角度生成
        THETA = Random.Range(T_MIN_MOVE_RANGE, T_MAX_MOVE_RANGE);
        PHI = Random.Range(P_MIN_MOVE_RANGE, P_MAX_MOVE_RANGE);
        //do
        //{
        //    THETA = Random.Range(T_MIN_MOVE_RANGE, T_MAX_MOVE_RANGE);
        //    PHI = Random.Range(P_MIN_MOVE_RANGE, P_MAX_MOVE_RANGE);
        //} while ((PHI * 180.0f / Mathf.PI) < 225 && (PHI * 180.0f / Mathf.PI) > -45);     //φが両端に来るものだけ採用

        //Debug.Log("THETA" + (THETA * 180.0f / Mathf.PI) + "PHI" + (PHI * 180.0f / Mathf.PI));
        //極座標変換
        X_VALUE = r * Mathf.Sin(THETA) * Mathf.Cos(PHI);
        Y_VALUE = r * Mathf.Sin(THETA) * Mathf.Sin(PHI);
        Z_VALUE = r * Mathf.Cos(THETA);

        //補間
        for (int i = 0; i < 10; i++)
        {
            interpolation_theta = pre_theta + ((THETA - pre_theta) / 10.0f) * i;     //現在の角度＋10分割*i
            interpolation_phi = pre_phi + ((PHI - pre_phi) / 10.0f) * i;
            //極座標変換した座標を格納（x,z,y）
            interpolation_vector[i] = new Vector3(r * Mathf.Sin(interpolation_theta) * Mathf.Cos(interpolation_phi), r * Mathf.Cos(interpolation_theta), r * Mathf.Sin(interpolation_theta) * Mathf.Sin(interpolation_phi));
        }
        pre_theta = THETA;
        pre_phi = PHI;

        // 目的地に着いていたら目的地を再設定する
        targetPosition = new Vector3(X_VALUE, Z_VALUE, Y_VALUE);    //座標系を合わせるために(x,z,y)になっている
        isReachTargetPosition = false;
    }

}