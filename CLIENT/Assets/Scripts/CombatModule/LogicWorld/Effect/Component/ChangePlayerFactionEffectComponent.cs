using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class ChangePlayerFactionEffectComponent : EffectComponent
    {
        //配置
        int m_faction = 0;
        bool m_revert_when_unapply = true;
        //运行数据
        int m_old_faction = 0;

        public override void Apply()
        {
            Player owner_player = GetOwnerPlayer();
            FactionComponent faction_component = owner_player.GetComponent(FactionComponent.ID) as FactionComponent;
            if (faction_component == null)
                return;
            m_old_faction = faction_component.Faction;
            faction_component.Faction = m_faction;
        }

        public override void Unapply()
        {
            if (!m_revert_when_unapply)
                return;
            Player owner_player = GetOwnerPlayer();
            FactionComponent faction_component = owner_player.GetComponent(FactionComponent.ID) as FactionComponent;
            if (faction_component == null)
                return;
            faction_component.Faction = m_old_faction;
        }
    }
}