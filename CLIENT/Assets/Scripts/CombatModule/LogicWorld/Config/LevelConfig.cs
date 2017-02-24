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
        public Dictionary<string, LevelData> m_level_data;

        public LevelConfig()
        {
            InitDummyConfigData();
        }

        public LevelData GetLevelData(string level_name)
        {
            LevelData level_data = null;
            if (!m_level_data.TryGetValue(level_name, out level_data))
                return null;
            return level_data;
        }

        public void InitDummyConfigData()
        {
            //假装有配置
        }
    }
}