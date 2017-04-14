using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class WeaponSkillComponent : SkillComponent
    {
        #region 配置数据
        //目标查找类型
        int m_target_gathering_type = 0;
        //目标查找参数
        FixPoint m_target_gathering_param1;
        FixPoint m_target_gathering_param2;
        #endregion

        #region 运行数据
        WeaponInflictTask m_task;
        List<int> m_defender_ids = new List<int>();
        SkillDefinitionComponent m_definition_cmp;
        #endregion

        #region 初始化/销毁
        protected override void PostInitializeComponent()
        {
            m_definition_cmp = ParentObject.GetComponent<SkillDefinitionComponent>();
        }

        protected override void OnDestruct()
        {
            if (m_task != null)
            {
                m_task.Cancel();
                m_task = null;
            }
            m_defender_ids.Clear();

            m_definition_cmp = null;
        }
        #endregion

        #region 技能的Activate流程
        public override void PostActivate(FixPoint start_time)
        {
            SaveDefenderIDs(GetOwnerSkill().GetTargets());
            Target skill_target = GetOwnerSkill().GetTarget();
            FireWeapon(skill_target);
        }

        private void FireWeapon(Target skill_target)
        {
            //构建damage info，准备伤害task
            Entity attacker = GetOwnerEntity();
            Damage damage_info = RecyclableObject.Create<Damage>();
            damage_info.m_attacker_id = attacker.ID;
            //damage_info.m_damage_type = 
            
            //计算延迟伤害时间
            FixPoint inflict_delay = GetInflictDelayTime(attacker, skill_target);
            ScheduleInflict(damage_info, inflict_delay);
        }

        //获取攻击时间
        private FixPoint GetInflictDelayTime(Entity attacker, Target skill_target)
        {
            //skill target可能是位置目标，可能是entity目标
            return FixPoint.Zero;
        }
        #endregion

        private void ScheduleInflict(Damage damage_info, FixPoint inflict_delay_time)
        {
            var task_scheduler = GetLogicWorld().GetTaskScheduler();
            if(m_task == null)
            {
                m_task = new WeaponInflictTask(this, damage_info);
            }
            task_scheduler.Schedule(m_task, GetCurrentTime(), inflict_delay_time);
        }

        public void Inflict(Damage damage, Entity attacker)
        {
            Skill owner_skill = GetOwnerSkill();
            //判断是否需要重新查找目标
            if(m_target_gathering_type != 0)
            {
                TargetGatheringManager target_gathering_manager = GetLogicWorld().GetTargetGatheringManager();
                List<Target> targets = target_gathering_manager.BuildTargetList(m_target_gathering_type,
                    m_target_gathering_param1, m_target_gathering_param2, owner_skill, GetOwnerEntity());
                SaveDefenderIDs(targets);
            }

            //对所有技能目标造成伤害，触发generator效果
            EntityManager entity_manager = GetLogicWorld().GetEntityManager();
            for(int i = 0; i < m_defender_ids.Count; ++i)
            {
                Entity defender = entity_manager.GetObject(m_defender_ids[i]);
                if (defender == null)
                    continue;
                GetImpactDamage(damage, attacker, defender);
                DamagableComponent damageable_cmp = defender.GetComponent<DamagableComponent>();
                if (damageable_cmp == null)
                    continue;
                damageable_cmp.TakeDamage(damage);
                ActivateImpactEffectGenerator(attacker, defender);
            }
        }

        private void GetImpactDamage(Damage damage, Entity attacker, Entity defender)
        {
            damage.m_damage_amount = FixPoint.One;
        }

        private void ActivateImpactEffectGenerator(Entity attacker, Entity defender)
        {

        }

        //普通攻击的目标可以是技能激活时期设置的目标，也可以是触发伤害时重新查找的目标
        private void SaveDefenderIDs(List<Target> skill_targets)
        {
            m_defender_ids.Clear();
            for(int i = 0; i < skill_targets.Count; ++i)
            {
                m_defender_ids.Add(skill_targets[i].GetEntityID());
            }
        }
    }


    //******************************************************************
    //普通攻击生效的Task
    //******************************************************************
    public class WeaponInflictTask : Task<LogicWorld>
    {
        WeaponSkillComponent m_weapon_skill_component;
        Damage m_damage_info;
        public WeaponInflictTask(WeaponSkillComponent weapon_skill_component, Damage damage_info)
        {
            m_weapon_skill_component = weapon_skill_component;
            m_damage_info = damage_info;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            if (m_weapon_skill_component == null)
                return;
            EntityManager entity_manager = logic_world.GetEntityManager();
            Entity attacker = entity_manager.GetObject(m_damage_info.m_attacker_id);
            if(attacker == null)
            {
                LogWrapper.LogError("WeaponInflictTask:Run, attacker null");
                return;
            }
            m_weapon_skill_component.Inflict(m_damage_info, attacker);
        }
    }
}
