using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class LevelComponent : Component
    {
        //配置数据
        int m_be_killed_experience = 0;
        int m_max_level = int.MaxValue;
        string m_experience_level_table;

        //运行数据
        int m_current_level = 1;
        LevelTableData m_table;
        int m_current_experience = 0;

#region GETTER
        public int CurrentLevel
        {
            get { return m_current_level; }
            set
            {
                if (value <= m_current_level || value > m_max_level)
                    return;
                ChangeLevel(value);
            }
        }

        public int CurrentExperience
        {
            get { return m_current_experience; }
        }
#endregion

#region 初始化/消耗
        public override void InitializeComponent()
        {
            ObjectProtoData proto_data = ParentObject.GetCreationContext().m_proto_data;
            if (proto_data == null)
                return;
            var dic = proto_data.m_component_variables;
            if (dic == null)
                return;
            string value;
            if (dic.TryGetValue("be_killed_experience", out value))
                m_be_killed_experience = int.Parse(value);

            if (m_experience_level_table != null)
            {
                m_table = GetLogicWorld().GetConfigProvider().GetLevelTableData(m_experience_level_table);
                if (m_max_level > m_table.m_max_level)
                    m_max_level = m_table.m_max_level;
            }
        }

        protected override void OnDestruct()
        {
            m_table = null;
        }
#endregion

        public void AddExperience(int xp_point)
        {
            if (m_table == null)
                return;
            m_current_experience += xp_point;
            int new_level = m_current_level;
            while (new_level < m_max_level && m_current_experience >= (int)m_table[new_level + 1])
                ++new_level;
            if (new_level != m_current_level)
                ChangeLevel(new_level);
            GetLogicWorld().AddSimpleRenderMessage(RenderMessageType.AddExperience, ParentObject.ID);
        }

        public void GetCurrentLevelInfo(out int total_xp, out int current_xp)
        {
            if (m_table == null || m_current_level == m_max_level)
            {
                total_xp = 0;
                current_xp = 0;
                return;
            }
            int xp1 = (int)m_table[m_current_level];
            int xp2 = (int)m_table[m_current_level + 1];
            total_xp = xp2 - xp1;
            current_xp = m_current_experience - xp1;
        }

        void ChangeLevel(int new_level)
        {
            if (new_level > m_max_level)
                return;
            int old_level = m_current_level;
            m_current_level = new_level;
            ParentObject.SendSignal(SignalType.ChangeLevel);
            GetLogicWorld().OnEntityChangeLevel(ParentObject, old_level, new_level);
            GetLogicWorld().AddSimpleRenderMessage(RenderMessageType.ChangeLevel, ParentObject.ID);
        }
    }
}