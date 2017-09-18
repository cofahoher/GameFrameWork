using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTSKillAction_PlayRenderEffect : BTAction
    {
        //配置数据
        int m_render_effect_cfgid = 0;

        //运行数据

        public BTSKillAction_PlayRenderEffect()
        {
        }

        public BTSKillAction_PlayRenderEffect(BTSKillAction_PlayRenderEffect prototype)
            : base(prototype)
        {
            m_render_effect_cfgid = prototype.m_render_effect_cfgid;
        }

        protected override void ResetRuntimeData()
        {
        }

        public override void ClearRunningTrace()
        {
#if COMBAT_CLIENT
            Skill skill = GetOwnerSkill();
            PlayRenderEffectMessage stop_msg = RenderMessage.Create<PlayRenderEffectMessage>();
            stop_msg.ConstructAsStop(skill.GetOwnerEntityID(), m_render_effect_cfgid);
            GetLogicWorld().AddRenderMessage(stop_msg);
#endif
        }

        protected override void OnActionEnter()
        {
        }

        protected override void OnActionUpdate(FixPoint delta_time)
        {
#if COMBAT_CLIENT
            Skill skill = GetOwnerSkill();
            PlayRenderEffectMessage start_msg = RenderMessage.Create<PlayRenderEffectMessage>();
            start_msg.ConstructAsPlay(skill.GetOwnerEntityID(), m_render_effect_cfgid, FixPoint.MinusOne);
            GetLogicWorld().AddRenderMessage(start_msg);
#endif
        }

        protected override void OnActionExit()
        {
        }
    }
}