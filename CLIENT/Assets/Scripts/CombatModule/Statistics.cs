using UnityEngine;
using System.Collections;

public class Statistics : MonoBehaviour
{
    public static Statistics Instance = null;

    bool m_enabled = false;
    bool m_started = false;
    int m_frame_cnt = 0;
    float m_start_time = 0;
    float m_last_frame_realtime = 0;

    int m_current_fps = 0;
    int m_statistics_fps1 = 0;
    int m_statistics_fps2 = 0;

    const int STATISTICS_CNT_1 = 10;
    const int STATISTICS_CNT_2 = 100;
    float[] m_time = new float[STATISTICS_CNT_2];

    public bool Enabled
    {
        get { return m_enabled; }
        set
        {
            m_enabled = value;
            m_started = false;
            m_frame_cnt = 0;
        }
    }

    void Start()
    {
        Instance = this;
    }

    void Update()
    {
        if (!m_enabled)
            return;
        ++m_frame_cnt;
        if (m_started)
        {
            float cur_realtime = Time.realtimeSinceStartup;
            float delta_time = cur_realtime - m_last_frame_realtime;  // Time.deltaTime
            m_current_fps = Mathf.FloorToInt(1.0f / delta_time);
            int index1 = (m_frame_cnt - 1) % STATISTICS_CNT_2;
            m_time[index1] = cur_realtime;
            if (m_frame_cnt > STATISTICS_CNT_2)
            {
                int index2 = (m_frame_cnt - STATISTICS_CNT_2) % STATISTICS_CNT_2;
                m_statistics_fps2 = Mathf.FloorToInt((STATISTICS_CNT_2 - 1) / (m_time[index1] - m_time[index2]));

                index1 = (m_frame_cnt - 1) % STATISTICS_CNT_2;
                index2 = (m_frame_cnt - STATISTICS_CNT_1) % STATISTICS_CNT_2;
                m_statistics_fps1 = Mathf.FloorToInt((STATISTICS_CNT_1 - 1) / (m_time[index1] - m_time[index2]));
            }
            m_last_frame_realtime = cur_realtime;
        }
        else if (m_frame_cnt == 200)
        {
            m_started = true;
            m_frame_cnt = 0;
            m_start_time = Time.realtimeSinceStartup;
            m_last_frame_realtime = m_start_time;
        }
    }

    void OnGUI()
    {
        if (m_started)
            GUI.Label(new Rect(20, 10, 960, 50), "FPS_1 = " + m_current_fps + ", FPS_10 = " + m_statistics_fps1 + ", FPS_100 = " + m_statistics_fps2 + "\r\nLATENCY = ??");
    }
}
