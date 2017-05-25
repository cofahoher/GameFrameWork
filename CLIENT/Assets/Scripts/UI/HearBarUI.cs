using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HearBarUI : MonoBehaviour
{
    public GameObject m_HpGauge;
    public GameObject m_HpGaugeBack;
    Transform mTrans;
    Transform mHPTrans;

    void Awake()
    {
        mTrans = transform;
        mHPTrans = m_HpGauge.transform;
    }

    void Start()
    {
        mTrans.localPosition = new Vector3(-0.01f, 2.26f, -0.05f);
    }

    public void ChangeHealth(FixPoint curHealth, FixPoint maxHealth)
    {
        float scale = (float)curHealth / (float)maxHealth;
        mHPTrans.localScale = new Vector3(scale, 1, 1);
        float x = 0.34f * (scale - 1);
        mHPTrans.localPosition = new Vector3(x, 0, 0);
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            mTrans.rotation = Camera.main.transform.rotation;
        }
    }
}
