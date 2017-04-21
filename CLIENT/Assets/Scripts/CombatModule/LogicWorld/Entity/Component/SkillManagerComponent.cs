using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    //yqqtodo 技能配置


    public partial class SkillManagerComponent : EntityComponent, ISignalListener
    {
        SignalListenerContext m_listener_context;

        //武器普通攻击
        int m_default_skill_cfgid = -1;
        //技能
        SortedDictionary<int, int> m_skill_cfgid2id = new SortedDictionary<int, int>();
        int m_move_block_count = 0;
        int m_active_block_count = 0;
        List<int> m_active_skill_ids = new List<int>();

        //缓存变量
        LocomotorComponent m_locomotor_cmp;

        public void SetExternalConfigDefaultSkill(int cfgid)
        {
            m_default_skill_cfgid = cfgid;
            m_skill_cfgid2id[cfgid] = -1;
        }

        public void SetExternalConfigSkill(int cfgid)
        {
            m_skill_cfgid2id[cfgid] = -1;
        }

        #region 初始化/销毁
        protected override void PostInitializeComponent()
        {
            //根据配置创建skill object
            var enumerator = m_skill_cfgid2id.GetEnumerator();
            while(enumerator.MoveNext())
            {
                int skill_cfgid = enumerator.Current.Key;
                ObjectTypeData type_data = null;
                //type_data = config.GetObjectTypeData(context.m_object_type_id); yqqtodo
                ObjectCreationContext object_context = new ObjectCreationContext();
                object_context.m_logic_world = GetLogicWorld();
                object_context.m_object_type_id = skill_cfgid;
                object_context.m_type_data = type_data;
                object_context.m_owner_id = ParentObject.ID;

                GetLogicWorld().GetSkillManager().CreateObject(object_context);
                m_skill_cfgid2id[skill_cfgid] = object_context.m_object_id;
            }

            //缓存变量
            m_locomotor_cmp = ParentObject.GetComponent<LocomotorComponent>();

            //添加监听
            m_listener_context = SignalListenerContext.CreateForEntityComponent(GetLogicWorld().GenerateSignalListenerID(), ParentObject.ID, m_component_type_id);
            ParentObject.AddListener(SignalType.StartMoving, m_listener_context);
            ParentObject.AddListener(SignalType.StopMoving, m_listener_context);
        }

        protected override void OnDestruct()
        {
            ParentObject.RemoveListener(SignalType.StartMoving, m_listener_context.ID);
            ParentObject.RemoveListener(SignalType.StopMoving, m_listener_context.ID);

            m_skill_cfgid2id.Clear();
            m_active_skill_ids.Clear();
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
            case SignalType.StopMoving:
                OnMovementStop();
                break;
            default:
                break;
            }
        }

        private void OnMovementStart()
        {
            SkillManager skill_manager = GetLogicWorld().GetSkillManager();
            var enumerator = m_active_skill_ids.GetEnumerator();
            while(enumerator.MoveNext())
            {
                Skill skill = skill_manager.GetObject(enumerator.Current);
                if(skill != null)
                {
                    SkillDefinitionComponent def_cmp = skill.GetSkillDefinitionComponent();
                    if (def_cmp.DeactivateWhenMoving)
                        skill.Interrupt();
                }
            }
        }
        private void OnMovementStop()
        {
        }

        public void OnGeneratorDestroyed(ISignalGenerator generator)
        {
        }
        #endregion

        public Skill GetDefaultSkill()
        {
            int default_skill_id = -1;
            m_skill_cfgid2id.TryGetValue(m_default_skill_cfgid, out default_skill_id);
            return GetLogicWorld().GetSkillManager().GetObject(default_skill_id);
        }

        public Skill GetSkill(int skill_cfgid)
        {
            int skill_id = -1;
            m_skill_cfgid2id.TryGetValue(skill_cfgid, out skill_id);
            return GetLogicWorld().GetSkillManager().GetObject(skill_id);
        }

        public bool CanActivateSkill()
        {
            if (!IsEnable())
                return false;
            return m_active_block_count == 0;
        }

        public void OnSkillActivated(Skill skill)
        {
            SkillDefinitionComponent def_cmp = skill.GetSkillDefinitionComponent();
            if(def_cmp.BlocksMovementWhenActive)
            {
                ++m_move_block_count;
                if(m_move_block_count == 1)
                {
                    if (m_locomotor_cmp != null)
                        m_locomotor_cmp.Disable();
                }
            }
            if (def_cmp.BlocksOtherSkillsWhenActive)
                ++m_active_block_count;
            m_active_skill_ids.Add(skill.ID);
        }

        public void OnSkillDeactivated(Skill skill)
        {
            SkillDefinitionComponent def_cmp = skill.GetSkillDefinitionComponent();
            if (def_cmp.BlocksMovementWhenActive)
            {
                --m_move_block_count;
                if(m_move_block_count == 0)
                {
                    if (m_locomotor_cmp != null)
                        m_locomotor_cmp.Enable();
                }
            }
            if (def_cmp.BlocksOtherSkillsWhenActive)
                --m_active_block_count;
            m_active_skill_ids.Remove(skill.ID);
        }

        public bool IsMoveAllowed()
        {
            return m_move_block_count == 0;
        }
    }
}
