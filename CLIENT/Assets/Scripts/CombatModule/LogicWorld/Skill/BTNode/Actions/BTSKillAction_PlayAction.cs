using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTSKillAction_PlayAction : BTAction
    {
        //配置数据
        string m_animation;
        string m_next_animation;
        bool m_loop = false;

        //运行数据

        public BTSKillAction_PlayAction()
        {
        }

        public BTSKillAction_PlayAction(BTSKillAction_PlayAction prototype)
            : base(prototype)
        {
            m_animation = prototype.m_animation;
            m_next_animation = prototype.m_next_animation;
            m_loop = prototype.m_loop;
        }

        protected override void ResetRuntimeData()
        {
        }

        public override void ClearRunningTrace()
        {
        }

        protected override void OnActionEnter()
        {
        }

        protected override void OnActionUpdate(FixPoint delta_time)
        {
#if COMBAT_CLIENT
            Skill skill = GetOwnerSkill();
            PlayAnimationRenderMessage msg = RenderMessage.Create<PlayAnimationRenderMessage>();
            msg.Construct(skill.GetOwnerEntityID(), m_animation, m_next_animation, m_loop);
            GetLogicWorld().AddRenderMessage(msg);
#endif
        }

        protected override void OnActionExit()
        {
        }
    }
}