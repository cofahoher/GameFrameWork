using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class FactionComponent : PlayerComponent
    {
        //配置
        int m_faction = 0;
        //运行数据
        int m_faction_index = 0;

        #region GETTER
        public int Faction
        {
            get { return m_faction; }
            set
            {
                ChangeFaction(value);
            }
        }

        public int FactionIndex
        {
            get { return m_faction_index; }
        }
        #endregion

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            ObjectProtoData proto_data = ParentObject.GetCreationContext().m_proto_data;
            if (proto_data == null)
                return;
            var dic = proto_data.m_component_variables;
            if (dic == null)
                return;
            string value;
            if (dic.TryGetValue("faction", out value))
                m_faction = (int)CRC.Calculate(value);
        }
        #endregion

        void ChangeFaction(int new_faction)
        {
            int new_faction_index = GetLogicWorld().GetFactionManager().Faction2Index(new_faction);
            ChangeFactionIndex(new_faction_index);
        }

        void ChangeFactionIndex(int new_faction_index)
        {
            if (m_faction_index == new_faction_index)
                return;
            m_faction_index = new_faction_index;
            ParentObject.SendSignal(SignalType.ChangeFaction);
            GetLogicWorld().AddSimpleRenderMessage(RenderMessageType.PlayerChangeFaction, ParentObject.ID);
        }
    }
}