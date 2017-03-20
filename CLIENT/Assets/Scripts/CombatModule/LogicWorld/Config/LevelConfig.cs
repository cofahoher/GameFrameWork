using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class LevelData
    {
        public string m_scene_name;
        public string m_enemy_wave_count;
        public Vector3I[] m_birth_position = new Vector3I[CommonDefinition.BirthPositionCountPerScene];
        //public 
    }

    public class LevelConfig
    {
        public Dictionary<int, LevelData> m_level_data = new Dictionary<int,LevelData>();

        public LevelConfig()
        {
            InitDummyConfigData();
        }

        public LevelData GetLevelData(int level_id)
        {
            LevelData level_data = null;
            if (!m_level_data.TryGetValue(level_id, out level_data))
                return null;
            return level_data;
        }

        public void InitDummyConfigData()
        {
            //假装有配置
            LevelData level_data = new LevelData();
            level_data.m_scene_name = "Scenes/zzw_test";
            m_level_data[1] = level_data;
        }
    }
}