using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class SkillManagerComponent : EntityComponent, ISignalListener
    {
        public const int DEFAULT_SKILL_INDEX = 0;

        //运行数据
        FixPoint m_attack_speed_rate = FixPoint.One;
        FixPoint m_cooldown_reduce_rate = FixPoint.Zero;
        LocomotorComponent m_locomotor_cmp;
        SignalListenerContext m_listener_context;
        SortedDictionary<int, int> m_index2id = new SortedDictionary<int, int>();
        int m_default_skill_id = 0;
        int m_move_block_count = 0;
        int m_active_block_count = 0;
        List<int> m_active_skill_ids = new List<int>();
        List<Skill> m_skill_to_interrupt = new List<Skill>();//ZZWTODO C#无法一边遍历一边删除的容器

        #region GETTER/SETTER
        public FixPoint AttackSpeedRate
        {
            get { return m_attack_speed_rate; }
            set
            {
                if (m_attack_speed_rate == value)
                    return;
                m_attack_speed_rate = value;
                SkillManager skill_manager = GetLogicWorld().GetSkillManager();
                for (int i = 0; i < m_active_skill_ids.Count; ++i)
                {
                    Skill skill = skill_manager.GetObject(m_active_skill_ids[i]);
                    if (skill == null)
                        continue;
                    SkillDefinitionComponent def_cmp = skill.GetDefinitionComponent();
                    if (!def_cmp.NormalAttack)
                        continue;
                    FixPoint current_time = GetLogicWorld().GetCurrentTime();
                    SkillTimer timer = def_cmp.GetTimer(SkillTimer.CooldownTimer);
                    timer.ChangeUpdateRate(current_time, m_attack_speed_rate);
                    timer = def_cmp.GetTimer(SkillTimer.CastingTimer);
                    timer.ChangeUpdateRate(current_time, m_attack_speed_rate);
                    timer = def_cmp.GetTimer(SkillTimer.InflictingTimer);
                    timer.ChangeUpdateRate(current_time, m_attack_speed_rate);
                    timer = def_cmp.GetTimer(SkillTimer.ExpirationTimer);
                    timer.ChangeUpdateRate(current_time, m_attack_speed_rate);
                }
                GetLogicWorld().AddSimpleRenderMessage(RenderMessageType.ChangeAttackSpeed, GetOwnerEntityID());
            }
        }

        public FixPoint CooldownReduceRate
        {
            get { return m_cooldown_reduce_rate; }
            set
            {
                if (m_cooldown_reduce_rate == value)
                    return;
                m_cooldown_reduce_rate = value;

                SkillManager skill_manager = GetLogicWorld().GetSkillManager();
                for (int i = 0; i < m_active_skill_ids.Count; ++i)
                {
                    Skill skill = skill_manager.GetObject(m_active_skill_ids[i]);
                    if (skill == null)
                        continue;
                    SkillDefinitionComponent def_cmp = skill.GetDefinitionComponent();
                    if (def_cmp.NormalAttack)
                        continue;
                    SkillTimer timer = def_cmp.GetTimer(SkillTimer.CooldownTimer);
                    timer.ChangeUpdateRate(GetCurrentTime(), CoolDownSpeedUpRate);
                }
                GetLogicWorld().AddSimpleRenderMessage(RenderMessageType.ChangeCooldDownReduce, GetOwnerEntityID());
            }
        }

        public FixPoint CoolDownSpeedUpRate
        {
            get
            {
                return FixPoint.One / (FixPoint.One - m_cooldown_reduce_rate);
            }
        }

        public LocomotorComponent GetLocomotorComponent()
        {
            return m_locomotor_cmp;
        }
        #endregion

        #region 初始化/销毁
        public void AddSkill(int index, int skill_cfgid)
        {
            if (index == DEFAULT_SKILL_INDEX)
                m_default_skill_id = skill_cfgid;
            m_index2id[index] = skill_cfgid;
        }

        protected override void PostInitializeComponent()
        {
            LogicWorld logic_world = GetLogicWorld();
            var enumerator = m_index2id.GetEnumerator();
            while (enumerator.MoveNext())
            {
                int skill_index = enumerator.Current.Key;
                int skill_cfgid = enumerator.Current.Value;
                CreateSkill(skill_index, skill_cfgid);
            }
            m_locomotor_cmp = ParentObject.GetComponent(LocomotorComponent.ID) as LocomotorComponent;
            m_listener_context = SignalListenerContext.CreateForEntityComponent(logic_world.GenerateSignalListenerID(), ParentObject.ID, m_component_type_id);
            ParentObject.AddListener(SignalType.StartMoving, m_listener_context);
        }

        Skill CreateSkill(int skill_index, int skill_cfgid)
        {
            LogicWorld logic_world = GetLogicWorld();
            SkillManager skill_manager = logic_world.GetSkillManager();
            IConfigProvider config = logic_world.GetConfigProvider();
            ObjectTypeData skill_data = config.GetSkillData(skill_cfgid);
            if (skill_data == null)
            {
                m_index2id[skill_index] = -1;
                return null;
            }
            ObjectCreationContext object_context = new ObjectCreationContext();
            object_context.m_object_type_id = skill_cfgid;
            object_context.m_type_data = skill_data;
            object_context.m_logic_world = logic_world;
            object_context.m_owner_id = ParentObject.ID;
            Skill skill = skill_manager.CreateObject(object_context);
            m_index2id[skill_index] = skill.ID;
            if (skill_index == DEFAULT_SKILL_INDEX)
                m_default_skill_id = skill.ID;
            if (skill.GetDefinitionComponent().StartsActive)
                skill.Activate(GetCurrentTime());
            return skill;
        }

        protected override void OnDestruct()
        {
            m_locomotor_cmp = null;
            ParentObject.RemoveListener(SignalType.StartMoving, m_listener_context.ID);
            SignalListenerContext.Recycle(m_listener_context);
            m_listener_context = null;

            SkillManager skill_manager = GetLogicWorld().GetSkillManager();
            var enumerator = m_index2id.GetEnumerator();
            while (enumerator.MoveNext())
            {
                int skill_id = enumerator.Current.Value;
                skill_manager.DestroyObject(skill_id);
            }
        }

        public override void OnDeletePending()
        {
            //StateSystem.DEAD_STATE

            SkillManager skill_manager = GetLogicWorld().GetSkillManager();
            var enumerator = m_index2id.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Skill skill = skill_manager.GetObject(enumerator.Current.Value);
                if (skill == null)
                    continue;
                if (skill.GetDefinitionComponent().StartsActive)
                    skill.Interrupt();
            }
        }

        public override void OnResurrect()
        {
            SkillManager skill_manager = GetLogicWorld().GetSkillManager();
            var enumerator = m_index2id.GetEnumerator();
            while (enumerator.MoveNext())
            {
                int skill_id = enumerator.Current.Value;
                Skill skill = skill_manager.GetObject(skill_id);
                if (skill == null)
                    continue;
                if (skill.GetDefinitionComponent().StartsActive)
                    skill.Activate(GetCurrentTime());
            }
        }
        #endregion

        #region ISignalListener
        public void ReceiveSignal(ISignalGenerator generator, int signal_type, System.Object signal = null)
        {
            switch (signal_type)
            {
            case SignalType.StartMoving:
                OnMovementStart();
                break;
            default:
                break;
            }
        }

        void OnMovementStart()
        {
            SkillManager skill_manager = GetLogicWorld().GetSkillManager();
            for (int i = 0; i < m_active_skill_ids.Count; ++i)
            {
                Skill skill = skill_manager.GetObject(m_active_skill_ids[i]);
                if (skill != null)
                {
                    SkillDefinitionComponent def_cmp = skill.GetDefinitionComponent();
                    if (def_cmp.DeactivateWhenMoving)
                        m_skill_to_interrupt.Add(skill);
                }
            }
            for (int i = 0; i < m_skill_to_interrupt.Count; ++i)
                m_skill_to_interrupt[i].Interrupt();
            m_skill_to_interrupt.Clear();
        }

        public void OnGeneratorDestroyed(ISignalGenerator generator)
        {
        }
        #endregion

        #region 技能替换
        public void ReplaceSkill(int skill_index, int skill_cfgid)
        {
            SkillManager skill_manager = GetLogicWorld().GetSkillManager();
            int old_skill_id;
            if (m_index2id.TryGetValue(skill_index, out old_skill_id))
                skill_manager.DestroyObject(old_skill_id);
            m_index2id[skill_index] = -1;
            CreateSkill(skill_index, skill_cfgid);
        }
        #endregion

        public Skill GetDefaultSkill()
        {
            return GetLogicWorld().GetSkillManager().GetObject(m_default_skill_id);
        }

        public Skill GetSkill(int index)
        {
            int skill_id;
            if (m_index2id.TryGetValue(index, out skill_id))
                return GetLogicWorld().GetSkillManager().GetObject(skill_id);
            else
                return null;
        }

        public bool CanActivateSkill()
        {
            if (!IsEnable())
                return false;
            return m_active_block_count == 0;
        }

        public void OnSkillActivated(Skill skill)
        {
            SkillDefinitionComponent def_cmp = skill.GetDefinitionComponent();
            if (def_cmp.StartsActive)
                return;
            if (def_cmp.BlocksMovementWhenActive)
            {
                ++m_move_block_count;
                if (m_move_block_count == 1)
                {
                    if (m_locomotor_cmp != null)
                        m_locomotor_cmp.Disable();
                }
            }
            else
            {
                if (def_cmp.BlocksRotatingWhenActive)
                {
                    PositionComponent position_component = skill.GetOwnerEntity().GetComponent(PositionComponent.ID) as PositionComponent;
                    if (position_component != null)
                        position_component.DisableRotating();
                }
                if (def_cmp.DeactivateWhenMoving)
                {
                    if (m_locomotor_cmp != null)
                        m_locomotor_cmp.StopMoving();
                }
                else
                {
                    if (def_cmp.m_main_animation != null && m_locomotor_cmp != null)
                    {
                        m_locomotor_cmp.BlockAnimation();
                    }
                }
            }
            if (def_cmp.BlocksOtherSkillsWhenActive)
                ++m_active_block_count;
            m_active_skill_ids.Add(skill.ID);
        }

        public void OnSkillDeactivated(Skill skill)
        {
            SkillDefinitionComponent def_cmp = skill.GetDefinitionComponent();
            if (def_cmp.StartsActive)
                return;
            bool play_idle_animation = true;
            if (def_cmp.BlocksMovementWhenActive)
            {
                --m_move_block_count;
                if (m_move_block_count == 0)
                {
                    if (m_locomotor_cmp != null)
                        m_locomotor_cmp.Enable();
                }
            }
            else
            {
                if (def_cmp.BlocksRotatingWhenActive)
                {
                    PositionComponent position_component = skill.GetOwnerEntity().GetComponent(PositionComponent.ID) as PositionComponent;
                    if (position_component != null)
                        position_component.EnableRotating();
                    if (m_locomotor_cmp != null && m_locomotor_cmp.IsMoving)
                        position_component.SetFacing(m_locomotor_cmp.GetMovementProvider().GetCurrentDirection());
                }
                if (def_cmp.DeactivateWhenMoving)
                {
                }
                else
                {
                    if (def_cmp.m_main_animation != null && m_locomotor_cmp != null)
                    {
                        m_locomotor_cmp.UnblockAnimation();
                        play_idle_animation = false;
                    }
                }
            }
            if (def_cmp.BlocksOtherSkillsWhenActive)
                --m_active_block_count;
            m_active_skill_ids.Remove(skill.ID);

#if COMBAT_CLIENT
            if (play_idle_animation && def_cmp.m_main_animation != null)
            {
                PlayAnimationRenderMessage msg = RenderMessage.Create<PlayAnimationRenderMessage>();
                msg.Construct(GetOwnerEntityID(), AnimationName.IDLE, null, true, FixPoint.One);
                GetLogicWorld().AddRenderMessage(msg);
            }
#endif
        }

        bool IsSkillActivated(Skill skill)
        {
            for (int i = 0; i < m_active_skill_ids.Count; ++i)
                if (m_active_skill_ids[i] == skill.ID)
                    return true;
            return false;
        }

        public bool IsMoveAllowed()
        {
            return m_move_block_count == 0;
        }

        protected override void OnDisable()
        {
            SkillManager skill_manager = GetLogicWorld().GetSkillManager();
            for (int i = 0; i < m_active_skill_ids.Count; ++i)
            {
                Skill skill = skill_manager.GetObject(m_active_skill_ids[i]);
                if (skill != null && !skill.GetDefinitionComponent().StartsActive)
                    m_skill_to_interrupt.Add(skill);
            }
            for (int i = 0; i < m_skill_to_interrupt.Count; ++i)
                m_skill_to_interrupt[i].Interrupt();
            m_skill_to_interrupt.Clear();
        }
    }
}
