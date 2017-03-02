using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class GlobalConfigManager : Singleton<GlobalConfigManager>
    {
        LevelConfig m_level_config = new LevelConfig();
        ObjectConfig m_object_config = new ObjectConfig();
        AttrubuteConfig m_attribute_config = new AttrubuteConfig();

        private GlobalConfigManager()
        {
        }

        public LevelConfig GetLevelConfig()
        {
            return m_level_config;
        }

        public ObjectConfig GetObjectConfig()
        {
            return m_object_config;
        }
        public AttrubuteConfig GetAttrubuteConfig()
        {
            return m_attribute_config;
        }

        public override void Destruct()
        {
        }
    }
}