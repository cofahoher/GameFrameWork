using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class LevelComponent : Component
    {
        //配置数据
        int m_level = 0;
        //运行数据
        int m_current_level = 0;

        public int CurrentLevel
        {
            get { return m_current_level; }
        }

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("level", out value))
            {
                m_level = int.Parse(value);
                m_current_level = m_level;
            }
        }
    }
}