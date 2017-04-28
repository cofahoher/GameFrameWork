using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class LevelComponent : Component
    {
        //运行数据
        int m_current_level = 0;

        public int CurrentLevel
        {
            get { return m_current_level; }
            set
            {
                int delta_level = value - m_current_level;
                ChangeLevel(delta_level);
            }
        }

        void ChangeLevel(int delta_level)
        {
            if (delta_level == 0)
                return;
            m_current_level += delta_level;
            ParentObject.SendSignal(SignalType.ChangeLevel);
        }
    }
}