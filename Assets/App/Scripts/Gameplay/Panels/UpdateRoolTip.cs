using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateRoolTip : MonoBehaviour
{
    private const float kShowTime = 0.8f;
    private bool mIsShow;
    private float mTimer;

    private void Update()
    {
        if (!mIsShow)
        {
            return;
        }
        mTimer += Time.deltaTime;
        if (!(mTimer > kShowTime)) return;
        mTimer = 0;
        gameObject.SetActive(false);
        mIsShow = false;
    }

    private void OnEnable()
    {
        mIsShow = true;
    }
}
