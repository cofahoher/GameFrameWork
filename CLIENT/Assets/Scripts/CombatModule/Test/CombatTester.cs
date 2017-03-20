using System;
using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class CombatTester
    {
        public static long TEST_LOCAL_PLAYER_PSTID = 123456789L;
        DateTime m_dt_original = DateTime.Now;
        CombatClient m_combat_client;

        bool m_init = false;
        int m_magic_count = 0;

        public CombatTester()
        {
        }

        public CombatClient GetCombatClient()
        {
            return m_combat_client;
        }

        public void Init()
        {
            CombatStartInfo csi = new CombatStartInfo();
            csi.m_level_id = 1;
            csi.m_world_seed = 1;
            m_combat_client = new CombatClient();
            m_combat_client.Initializa(TEST_LOCAL_PLAYER_PSTID, csi);
            m_combat_client.AddPlayer(TEST_LOCAL_PLAYER_PSTID);
            m_init = true;
        }

        public void Update()
        {
            if (!m_init)
                return;
            m_combat_client.OnUpdate(m_combat_client.GetCurrentTime());
        }
    }
}