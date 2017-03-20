using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    //ZZWTODO
    public class CombatStartInfo
    {
        public int m_level_id = -1;
        public int m_game_mode = -1;
        public int m_world_seed = -1;
        public List<CombatPlayerInfo> m_players;
    }

    public class CombatPlayerInfo
    {
        long m_pstid = -1;
        public List<CombatObjectInfo> m_objects;
    }

    public class CombatObjectInfo
    {
        int m_type_id = -1;
        int m_proto_id = -1;
        int m_level = -1;
    }
}