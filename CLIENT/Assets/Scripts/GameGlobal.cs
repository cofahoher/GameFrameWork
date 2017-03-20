using UnityEngine;
using System.Collections;

public class GameGlobal : MonoBehaviour
{
    static GameGlobal ms_instance;
    public static GameGlobal Instance
    {
        get { return ms_instance; }
    }

    void Awake()
    {
        ms_instance = this;
        UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        InitSyncModelTest();
    }

    void Update()
    {
        UpdateSyncModelTest();
    }

    #region 测试同步模型
    Combat.SyncTester m_sync_tester = null;
    Combat.CombatTester m_combat_tester = null;

    void InitSyncModelTest()
    {
        if (m_sync_tester != null)
            return;
        m_sync_tester = new Combat.SyncTester(this);
        m_sync_tester.Init();
        m_combat_tester = new Combat.CombatTester();
        m_combat_tester.Init();
    }

    void UpdateSyncModelTest()
    {
        if (m_sync_tester != null)
            m_sync_tester.Update();
        if (m_combat_tester != null)
            m_combat_tester.Update();
    }

    public delegate void LoadSceneCallback(int scene_build_index);
    public LoadSceneCallback m_loadscene_callback = null;
    void OnLevelWasLoaded(int scene_build_index)
    {
        if (m_loadscene_callback != null)
            m_loadscene_callback(scene_build_index);
    }
    #endregion
}
