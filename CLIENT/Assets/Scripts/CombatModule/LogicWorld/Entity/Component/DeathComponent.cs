using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class DeathComponent : EntityComponent, INeedTaskService
    {
        //配置数据
        int m_born_generator_cfgid = 0;
        int m_die_generator_cfgid = 0;
        int m_killer_generator_cfgid = 0;
        FixPoint m_life_time = FixPoint.Zero;
        FixPoint m_hide_delay = FixPoint.Zero;
        FixPoint m_delete_delay = FixPoint.Zero;
        bool m_can_resurrect = false;

        //运行数据
        EffectGenerator m_born_generator;
        EffectGenerator m_die_generator;
        EffectGenerator m_killer_generator;
        ComponentCommonTask m_die_task;
        int m_die_silently = 0;

        #region GETTER
        public bool DieSilently
        {
            get { return m_die_silently > 0; }
            set
            {
                if (value)
                    ++m_die_silently;
                else
                    --m_die_silently;
            }
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
            if (dic.TryGetValue("born_generator_id", out value))
                m_born_generator_cfgid = int.Parse(value);
            if (dic.TryGetValue("die_generator_id", out value))
                m_die_generator_cfgid = int.Parse(value);
            if (dic.TryGetValue("killer_generator_id", out value))
                m_killer_generator_cfgid = int.Parse(value);
            if (dic.TryGetValue("life_time", out value))
                m_life_time = FixPoint.Parse(value);
            SetLifeTime(m_life_time);

            EffectManager effect_manager = GetLogicWorld().GetEffectManager();
            if (m_born_generator_cfgid != 0)
                m_born_generator = effect_manager.CreateGenerator(m_born_generator_cfgid, GetOwnerEntity());
            if (m_die_generator_cfgid != 0)
                m_die_generator = effect_manager.CreateGenerator(m_die_generator_cfgid, GetOwnerEntity());
            if (m_killer_generator_cfgid != 0)
                m_killer_generator = effect_manager.CreateGenerator(m_killer_generator_cfgid, GetOwnerEntity());
        }

        protected override void PostInitializeComponent()
        {
            if (m_born_generator != null)
                ApplyGenerator(m_born_generator);
        }

        protected override void OnDestruct()
        {
            if (m_die_task != null)
            {
                m_die_task.Cancel();
                LogicTask.Recycle(m_die_task);
                m_die_task = null;
            }
            EffectManager effect_manager = GetLogicWorld().GetEffectManager();
            if (m_born_generator != null)
            {
                effect_manager.DestroyGenerator(m_born_generator.ID, GetOwnerEntityID());
                m_born_generator = null;
            }
            if (m_die_generator != null)
            {
                effect_manager.DestroyGenerator(m_die_generator.ID, GetOwnerEntityID());
                m_die_generator = null;
            }
            if (m_killer_generator != null)
            {
                effect_manager.DestroyGenerator(m_killer_generator.ID, GetOwnerEntityID());
                m_killer_generator = null;
            }
        }

        public override void OnResurrect()
        {
            StateComponent state_component = ParentObject.GetComponent(StateComponent.ID) as StateComponent;
            if (state_component != null)
                state_component.RemoveState(StateSystem.DEAD_STATE, 0);
            GetLogicWorld().AddSimpleRenderMessage(RenderMessageType.Show, GetOwnerEntityID());
        }
        #endregion

        public void SetLifeTime(FixPoint life_time)
        {
            m_life_time = life_time;
            if (m_die_task != null)
                m_die_task.Cancel();
            if (m_life_time > FixPoint.Zero)
            {
                if (m_die_task == null)
                {
                    m_die_task = LogicTask.Create<ComponentCommonTask>();
                    m_die_task.Construct(this);
                }
                var schedeler = GetLogicWorld().GetTaskScheduler();
                schedeler.Schedule(m_die_task, GetCurrentTime(), m_life_time);
            }
        }

        public void KillOwner(int killer_id)
        {
            //ZZWTODO Resurrect

            if (m_die_task != null)
                m_die_task.Cancel();
            LogicWorld logic_world = GetLogicWorld();

            Entity killer = logic_world.GetEntityManager().GetObject(killer_id);

            if (!DieSilently && killer_id != ParentObject.ID && m_killer_generator != null && killer != null)
            {
                EffectApplicationData app_data = RecyclableObject.Create<EffectApplicationData>();
                app_data.m_original_entity_id = ParentObject.ID;
                app_data.m_source_entity_id = ParentObject.ID;
                m_killer_generator.Activate(app_data, killer);
                RecyclableObject.Recycle(app_data);
            }

            var schedeler = logic_world.GetTaskScheduler();
            if (DieSilently)
            {
                logic_world.AddSimpleRenderMessage(RenderMessageType.Hide, ParentObject.ID);
            }
            else
            {
                HideEntityTask hide_task = LogicTask.Create<HideEntityTask>();
                hide_task.Construct(ParentObject.ID);
                schedeler.Schedule(hide_task, GetCurrentTime(), m_hide_delay);
            }

            ParentObject.DeletePending = true;
            ParentObject.SendSignal(SignalType.Die);
            logic_world.AddSimpleRenderMessage(RenderMessageType.Die, ParentObject.ID);

            StateComponent state_component = ParentObject.GetComponent(StateComponent.ID) as StateComponent;
            if (state_component != null)
                state_component.AddState(StateSystem.DEAD_STATE, 0);

            if (!m_can_resurrect)
            {
                DeleteEntityTask delete_task = LogicTask.Create<DeleteEntityTask>();
                delete_task.Construct(ParentObject.ID);
                schedeler.Schedule(delete_task, GetCurrentTime(), m_delete_delay);
            }

            logic_world.OnKillEntity(killer, GetOwnerEntity());
        }

        public void OnTaskService(FixPoint delta_time)
        {
            KillOwner(ParentObject.ID);
        }

        public void ApplyDieGenerator()
        {
            if (m_die_generator != null)
                ApplyGenerator(m_die_generator);
        }

        void ApplyGenerator(EffectGenerator generator)
        {
            EffectApplicationData app_data = RecyclableObject.Create<EffectApplicationData>();
            app_data.m_original_entity_id = ParentObject.ID;
            app_data.m_source_entity_id = ParentObject.ID;
            generator.Activate(app_data, GetOwnerEntity());
            RecyclableObject.Recycle(app_data);
        }

        void Resurrect()
        {
            StateComponent state_component = ParentObject.GetComponent(StateComponent.ID) as StateComponent;
            if (state_component != null)
                state_component.RemoveState(StateSystem.DEAD_STATE, 0);
        }
    } 

    class HideEntityTask : Task<LogicWorld>
    {
        int m_entity_id = 0;

        public void Construct(int entity_id)
        {
            m_entity_id = entity_id;
        }

        public override void OnReset()
        {
            m_entity_id = 0;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            Entity entity = logic_world.GetEntityManager().GetObject(m_entity_id);
            if (entity == null)
                return;
            DeathComponent death_component = entity.GetComponent(DeathComponent.ID) as DeathComponent;
            if (death_component != null)
                death_component.ApplyDieGenerator();
            logic_world.AddSimpleRenderMessage(RenderMessageType.Hide, m_entity_id);
            LogicTask.Recycle(this);
        }
    }       

    class DeleteEntityTask : Task<LogicWorld>
    {
        int m_entity_id;

        public void Construct(int entity_id)
        {
            m_entity_id = entity_id;
        }

        public override void OnReset()
        {
            m_entity_id = 0;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            logic_world.GetEntityManager().DestroyObject(m_entity_id);
            LogicTask.Recycle(this);
        }
    }
}