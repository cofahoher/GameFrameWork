using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class SceneSpace : IDestruct
    {
        public int m_space_id = 0;
        public Vector3FP m_min_position;
        public Vector3FP m_max_position;
        public ISpacePartition m_paitition;
        public GridGraph m_graph;

        public virtual void Destruct()
        {
            m_paitition.Destruct();
            m_paitition = null;
            m_graph.Destruct();
            m_graph = null;
        }

        public bool IsInRange(Vector3FP position)
        {
            if (position.x < m_min_position.x || position.z < m_min_position.z || position.x > m_max_position.x || position.z > m_max_position.z)
                return false;
            return true;
        }

        public virtual SceneSpace GetNeighbourSpace(Vector3FP position)
        {
            return this;
        }

        public virtual void OnEntityDestroy(Entity entity)
        {
        }
    }

    public class LogicWorld : GeneralComposableObject<LogicWorld, FixPoint>, ILogicWorld, IRenderMessageGenerator
    {
        protected IOutsideWorld m_outside_world;
        protected RandomGeneratorI m_random_generator_int;
        protected RandomGeneratorFP m_random_generator_fp;

        protected IRenderWorld m_render_world;
        protected bool m_need_render_message = false;
        protected List<RenderMessage> m_render_messages;

        protected FixPoint m_current_time = FixPoint.Zero;
        protected int m_current_frame = 0;
        protected bool m_game_over = false;
        protected bool m_collapsing = false;
        protected TaskScheduler<LogicWorld> m_scheduler;

        protected IDGenerator m_signal_listener_id_generator;
        protected IDGenerator m_attribute_modifier_id_generator;
        protected IDGenerator m_damage_modifier_id_generator;

        protected PlayerManager m_player_manager;
        protected EntityManager m_entity_manager;
        protected SkillManager m_skill_manager;
        protected EffectManager m_effect_manager;
        protected FactionManager m_faction_manager;

        protected TargetGatheringManager m_target_gathering_manager;
        protected RegionCallbackManager m_region_callback_manager;

        protected ICommandHandler m_command_handler;

        public LogicWorld()
        {
        }

        #region 初始化/销毁
        public virtual void Initialize(IOutsideWorld outside_world, int world_seed, bool need_render_message)
        {
#if ALLOW_UPDATE
            AddComponent<LogicWorldEveryFrameUpdater>(true);
#endif
            m_outside_world = outside_world;
            m_random_generator_int = new RandomGeneratorI(world_seed);
            m_random_generator_fp = new RandomGeneratorFP(world_seed);

            m_need_render_message = need_render_message;
            if (m_need_render_message)
                m_render_messages = new List<RenderMessage>();

            m_scheduler = new TaskScheduler<LogicWorld>(this);

            m_signal_listener_id_generator = new IDGenerator(IDGenerator.SIGNAL_LISTENER_FIRST_ID);
            m_attribute_modifier_id_generator = new IDGenerator(IDGenerator.ATTRIBUTE_MODIFIER_FIRST_ID);
            m_damage_modifier_id_generator = new IDGenerator(IDGenerator.DAMAGE_MODIFIER_FIRST_ID);

            m_player_manager = new PlayerManager(this);
            m_entity_manager = new EntityManager(this);
            m_skill_manager = new SkillManager(this);
            m_effect_manager = new EffectManager(this);
            m_faction_manager = new FactionManager(this);

            m_target_gathering_manager = new TargetGatheringManager(this);
            m_region_callback_manager = new RegionCallbackManager(this);

            m_command_handler = CreateCommandHandler();

            PostInitialize();
        }

        protected virtual ICommandHandler CreateCommandHandler()
        {
            return new DummyCommandHandler();
        }

        protected virtual void PostInitialize()
        {
        }

        public void SetIRenderWorld(IRenderWorld render_world)
        {
            m_render_world = render_world;
        }

        public virtual void BuildLogicWorld(WorldCreationContext world_context)
        {
            m_player_manager.SetPstidAndProxyid(world_context.m_pstid2proxyid, world_context.m_proxyid2pstid);
            IConfigProvider config = GetConfigProvider();
            for (int i = 0; i < world_context.m_players.Count; ++i)
            {
                ObjectCreationContext context = world_context.m_players[i];
                context.m_logic_world = this;
                context.m_type_data = config.GetObjectTypeData(context.m_object_type_id);
                context.m_proto_data = config.GetObjectProtoData(context.m_object_proto_id);
                m_player_manager.CreateObject(context);
            }
            OnPlayersCreated();
            for (int i = 0; i < world_context.m_entities.Count; ++i)
            {
                ObjectCreationContext context = world_context.m_entities[i];
                context.m_logic_world = this;
                context.m_type_data = config.GetObjectTypeData(context.m_object_type_id);
                context.m_proto_data = config.GetObjectProtoData(context.m_object_proto_id);
                m_entity_manager.CreateObject(context);
            }
            m_player_manager.OnWorldBuilt();
        }

        protected virtual void OnPlayersCreated()
        {
        }

        public virtual void Destruct()
        {
            m_collapsing = true;

            m_scheduler.Destruct();
            m_scheduler = null;
            m_random_generator_int.Destruct();
            m_random_generator_int = null;
            m_random_generator_fp.Destruct();
            m_random_generator_fp = null;
            m_signal_listener_id_generator.Destruct();
            m_signal_listener_id_generator = null;
            m_attribute_modifier_id_generator.Destruct();
            m_attribute_modifier_id_generator = null;
            m_damage_modifier_id_generator.Destruct();
            m_damage_modifier_id_generator = null;

            m_entity_manager.Destruct();
            m_entity_manager = null;
            m_player_manager.Destruct();
            m_player_manager = null;

            m_skill_manager.Destruct();
            m_skill_manager = null;
            m_effect_manager.Destruct();
            m_effect_manager = null;
            m_faction_manager.Destruct();
            m_faction_manager = null;

            m_target_gathering_manager.Destruct();
            m_target_gathering_manager = null;
            m_region_callback_manager.Destruct();
            m_region_callback_manager = null;

            m_command_handler.Destruct();
            m_command_handler = null;

            DestroyAllGeneralComponent();
        }
        #endregion

        #region GETTER
        public bool IsCollapsing
        {
            get { return m_collapsing; }
        }
        public bool NeedRenderMessage
        {
            get { return m_need_render_message; }
        }
        public FixPoint CurrentTime
        {
            get { return m_current_time; }
        }
        public int CurrentFrame
        {
            get { return m_current_frame; }
        }
        public FixPoint GetCurrentTime()
        {
            return m_current_time;
        }
        public IConfigProvider GetConfigProvider()
        {
            return m_outside_world.GetConfigProvider();
        }
        public LevelData GetLevelData()
        {
            return m_outside_world.GetLevelData();
        }
        public TaskScheduler<LogicWorld> GetTaskScheduler()
        {
            return m_scheduler;
        }
        public RandomGeneratorI GetRandomGeneratorI()
        {
            return m_random_generator_int;
        }
        public RandomGeneratorFP GetRandomGeneratorFP()
        {
            return m_random_generator_fp;
        }
        public IDGenerator GetSignalListenerIDGenerator()
        {
            return m_signal_listener_id_generator;
        }
        public IDGenerator GetAttributeModifierIDGenerator()
        {
            return m_attribute_modifier_id_generator;
        }
        public IDGenerator GetDamageModifierIDGenerator()
        {
            return m_damage_modifier_id_generator;
        }
        public PlayerManager GetPlayerManager()
        {
            return m_player_manager;
        }
        public EntityManager GetEntityManager()
        {
            return m_entity_manager;
        }
        public SkillManager GetSkillManager()
        {
            return m_skill_manager;
        }
        public EffectManager GetEffectManager()
        {
            return m_effect_manager;
        }
        public FactionManager GetFactionManager()
        {
            return m_faction_manager;
        }
        public TargetGatheringManager GetTargetGatheringManager()
        {
            return m_target_gathering_manager;
        }
        public RegionCallbackManager GetRegionCallbackManager()
        {
            return m_region_callback_manager;
        }

        public virtual SceneSpace GetDefaultSceneSpace()
        {
            return null;
        }
        #endregion

        #region ILogicWorld 可被继承修改
        public virtual void OnStart()
        {
            m_current_time = FixPoint.Zero;
            m_current_frame = 0;
            m_outside_world.OnGameStart();
        }

        public virtual bool OnUpdate(int delta_ms)
        {
            FixPoint delta_time = new FixPoint(delta_ms) / FixPoint.Thousand;
            m_current_time += delta_time;
            ++m_current_frame;
            m_region_callback_manager.OnUpdate(delta_ms);
            m_scheduler.Update(m_current_time);
            UpdateGeneralComponent(delta_time, m_current_time);
            return m_game_over;
        }

        public bool IsGameOver()
        {
            return m_game_over;
        }

        public virtual void HandleCommand(Command command)
        {
            if (m_game_over)
                return;
            bool result = m_command_handler.Handle(command);
            //ZZWTODO 也可以用RenderMessage
            if (m_render_world != null && command.m_player_pstid == m_outside_world.GetLocalPlayerPstid())
                m_render_world.OnLogicWorldHandleCommand(command, result);
        }

        public virtual void CopyFrom(ILogicWorld parallel_world)
        {
        }

        public int GetCurrentFrame()
        {
            return m_current_frame;
        }

        public uint GetCRC()
        {
            return 0;
        }
        #endregion

        #region RenderMessage
        public bool CanGenerateRenderMessage()
        {
            return m_need_render_message;
        }

        public void AddRenderMessage(RenderMessage render_message)
        {
            if (m_render_messages == null)
            {
                RenderMessage.Recycle(render_message);
                return;
            }
            m_render_messages.Add(render_message);
        }

        public void AddSimpleRenderMessage(int type, int entity_id = -1, int simple_data = 0)
        {
            if (m_render_messages == null)
                return;
            SimpleRenderMessage render_message = RenderMessage.Create<SimpleRenderMessage>();
            render_message.Construct(type, entity_id, simple_data);
            m_render_messages.Add(render_message);
        }

        public List<RenderMessage> GetAllRenderMessages()
        {
            return m_render_messages;
        }

        public void ClearRenderMessages()
        {
            if (m_render_messages != null)
                m_render_messages.Clear();
        }
        #endregion

        #region 事件

        public virtual void OnCauseDamage(Entity attacker, Entity victim, Damage damage)
        {
        }

        public virtual void OnKillEntity(Entity killer, Entity the_dead)
        {
        }

        public virtual void OnEntityChangeLevel(Object obj, int old_level, int new_level)
        {
        }

        public virtual bool OnEntityOutOfEdge(Entity entity, Vector3FP position)
        {
            return false;
        }

        public virtual void OnGameOver(GameResult game_result)
        {
            if (m_game_over)
                return;
            m_game_over = true;
            game_result.m_end_frame = m_current_frame;
            m_outside_world.OnGameOver(game_result);
        }
        #endregion

        #region 创建Entity
        public Entity CreateEntity(ObjectCreationContext context)
        {
            IConfigProvider config = m_outside_world.GetConfigProvider();
            context.m_logic_world = this;
            context.m_type_data = config.GetObjectTypeData(context.m_object_type_id);
            context.m_proto_data = config.GetObjectProtoData(context.m_object_proto_id);
            return m_entity_manager.CreateObject(context);
        }
        #endregion

        #region ID
        public int GenerateSignalListenerID()
        {
            return m_signal_listener_id_generator.GenID();
        }
        public int GenerateAttributeModifierID()
        {
            return m_attribute_modifier_id_generator.GenID();
        }
        public int GenerateDamageModifierID()
        {
            return m_damage_modifier_id_generator.GenID();
        }
        #endregion

        #region GeneralComposableObject
        protected override LogicWorld GetSelf()
        {
            return this;
        }
        #endregion
    }

    class LogicTask
    {
        public static TTask Create<TTask>() where TTask : Task<LogicWorld>, new()
        {
            return ResuableObjectPool<IRecyclable>.Instance.Create<TTask>();
        }
        public static void Recycle(Task<LogicWorld> instance)
        {
            ResuableObjectPool<IRecyclable>.Instance.Recycle(instance);
        }
    }
}
