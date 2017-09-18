using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTSKillAction_PlaySound : BTAction
    {
        //配置数据
        int m_sound = 0;

        //运行数据

        public BTSKillAction_PlaySound()
        {
        }

        public BTSKillAction_PlaySound(BTSKillAction_PlaySound prototype)
            : base(prototype)
        {
            m_sound = prototype.m_sound;
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
            PlaySoundMessage msg = RenderMessage.Create<PlaySoundMessage>();
            msg.Construct(skill.GetOwnerEntityID(), m_sound, FixPoint.MinusOne);
            GetLogicWorld().AddRenderMessage(msg);
#endif
        }

        protected override void OnActionExit()
        {
        }
    }
}