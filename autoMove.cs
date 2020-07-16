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
    private const float SPEED = 0.05f;

    // Θ下限上限 rad = deg*π/180
    private const float T_MIN_MOVE_RANGE = 75.0f * (Mathf.PI / 180.0f);
    private const float T_MAX_MOVE_RANGE = 105.0f * (Mathf.PI / 180.0f);
    // φ下限上限 deg
    private const float P_MIN_MOVE_RANGE = -45.0f * (Mathf.PI / 180.0f);    
    private const float P_MAX_MOVE_RANGE = 225.0f * (Mathf.PI / 180.0f);
    // r
    private const float r = 10.0f;

    // ランダム角度格納用
    private float THETA;
    private float PHI;

    // 極座標変換後の x, y, z の値
    private float X_VALUE;
    private float Y_VALUE;
    private float Z_VALUE;


    void Start()
    {
        this.isReachTargetPosition = false;
        decideTargetPotision();
    }

    void Update()
    {
        decideTargetPotision();

        // 現在地から目的地までSPEEDの速度で移動する
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

        //ランダム角度生成
        THETA = Random.Range(T_MIN_MOVE_RANGE, T_MAX_MOVE_RANGE);
        PHI = Random.Range(P_MIN_MOVE_RANGE, P_MAX_MOVE_RANGE);
        Debug.Log("THETA" + (THETA * 180.0f / Mathf.PI) + "PHI" + (PHI * 180.0f / Mathf.PI));
        //極座標変換
        X_VALUE = r * Mathf.Sin(THETA) * Mathf.Cos(PHI);
        Y_VALUE = r * Mathf.Sin(THETA) * Mathf.Sin(PHI);
        Z_VALUE = r * Mathf.Cos(THETA);

        // 目的地に着いていたら目的地を再設定する
        targetPosition = new Vector3(X_VALUE, Z_VALUE, Y_VALUE);
        //Debug.Log(targetPosition);
        isReachTargetPosition = false;
    }
}