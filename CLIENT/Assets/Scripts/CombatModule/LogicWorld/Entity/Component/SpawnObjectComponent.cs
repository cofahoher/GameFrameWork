using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class SpawnObjectComponent : EntityComponent, INeedTaskService, ISignalListener
    {
        //ZZWTODO 应该根据具体游戏，搞一套生成规则，这里只需要一个配置ID

        //配置数据
        int m_object_type_id = 0;
        int m_object_proto_id = 0;
        FixPoint m_object_distance = FixPoint.One;
        int m_init_count = 0;
        int m_max_count = 0;
        FixPoint m_update_interval = FixPoint.Ten;

        //运行数据
        SignalListenerContext m_listener_context;
        ComponentCommonTask m_task;
        Dictionary<int, Vector2FP> m_current_objects = new Dictionary<int,Vector2FP>();
        FixPoint m_min_x;
        FixPoint m_max_x;
        FixPoint m_min_z;
        FixPoint m_max_z;

        #region 初始化/销毁
        protected override void PostInitializeComponent()
        {
            ResetSpawnAreaRange();
            if (m_update_interval < FixPoint.One)
                m_update_interval = FixPoint.One;
            m_listener_context = SignalListenerContext.CreateForEntityComponent(GetLogicWorld().GenerateSignalListenerID(), ParentObject.ID, m_component_type_id);
            m_task = LogicTask.Create<ComponentCommonTask>();
            m_task.Construct(this);
            var schedeler = GetLogicWorld().GetTaskScheduler();
            schedeler.Schedule(m_task, GetCurrentTime(), m_update_interval, m_update_interval);
            for (int i = 0; i < m_init_count; ++i)
                SpawnOneObject();
        }

        protected override void OnDestruct()
        {
            SignalListenerContext.Recycle(m_listener_context);
            m_listener_context = null;
            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
        }

        public override void OnDeletePending()
        {
            //ZZWTODO
        }

        public override void OnResurrect()
        {
            //ZZWTODO
        }
        #endregion

        #region ISignalListener
        public void ReceiveSignal(ISignalGenerator generator, int signal_type, System.Object signal = null)
        {
            switch (signal_type)
            {
            case SignalType.Die:
                OnEntityDie(generator as Entity);
                break;
            default:
                break;
            }
        }

        void OnEntityDie(Entity target)
        {
            m_current_objects.Remove(target.ID);
        }

        public void OnGeneratorDestroyed(ISignalGenerator generator)
        {
        }
        #endregion

        public void OnTaskService(FixPoint delta_time)
        {
            if (m_current_objects.Count >= m_max_count)
                return;
            SpawnOneObject();
        }

        void SpawnOneObject()
        {
            Vector2FP random_position = new Vector2FP();
            if (!RandomPosition(ref random_position))
                return;

            Player player = GetOwnerPlayer();
            LogicWorld logic_world = GetLogicWorld();
            EntityManager entity_manager = logic_world.GetEntityManager();
            IConfigProvider config = logic_world.GetConfigProvider();
            BirthPositionInfo birth_info = new BirthPositionInfo(random_position.x, new FixPoint(0), random_position.z, new FixPoint(90));

            ObjectCreationContext object_context = new ObjectCreationContext();
            object_context.m_object_proxy_id = player.ProxyID;
            object_context.m_object_type_id = m_object_type_id;
            object_context.m_object_proto_id = m_object_proto_id;
            object_context.m_birth_info = birth_info;
            object_context.m_type_data = config.GetObjectTypeData(object_context.m_object_type_id);
            object_context.m_proto_data = config.GetObjectProtoData(object_context.m_object_proto_id);
            object_context.m_logic_world = logic_world;
            object_context.m_owner_id = ParentObject.ID;
            object_context.m_is_ai = true;
            object_context.m_is_local = player.IsLocal;
            Entity obj = entity_manager.CreateObject(object_context);
            m_current_objects[obj.ID] = random_position;
            obj.AddListener(SignalType.Die, m_listener_context);
        }

        void ResetSpawnAreaRange()
        {
            FixPoint half_distance = m_object_distance >> 1;
            LevelData level_data = GetLogicWorld().GetLevelData();
            m_min_x = level_data.m_center_x - (level_data.m_length_x >> 1) + half_distance;
            m_max_x = level_data.m_center_x + (level_data.m_length_x >> 1) - half_distance;
            m_min_z = level_data.m_center_z - (level_data.m_length_z >> 1) + half_distance;
            m_max_z = level_data.m_center_z + (level_data.m_length_z >> 1) - half_distance;
        }

        bool RandomPosition(ref Vector2FP random_position)
        {
            //ZZWTODO 随机分布，分成grid，随机选一个然后标志占用
            RandomGeneratorFP random_generator_fp = GetLogicWorld().GetRandomGeneratorFP();
            for (int i = 0; i < 50; ++i)
            {
                random_position.x = random_generator_fp.RandBetween(m_min_x, m_max_x);
                random_position.z = random_generator_fp.RandBetween(m_min_z, m_max_z);
                bool overlap = false;
                var enumerator = m_current_objects.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Value.FastDistance(ref random_position) < m_object_distance)
                    {
                        overlap = true;
                        break;
                    }
                }
                if (!overlap)
                    return true;
            }
            return false;
        }
    }
}