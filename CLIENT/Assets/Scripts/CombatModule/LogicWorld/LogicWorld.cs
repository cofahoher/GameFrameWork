using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class LogicWorld : ILogicWorld, IRenderMessageGenerator
    {
        protected IOutsideWorld m_outside_world;
        protected bool m_need_render_message = false;
        protected List<RenderMessage> m_render_messages;

        protected int m_current_time = 0;
        protected int m_current_frame = 0;
        protected int m_current_turn_index = 0;
        protected int m_current_turn_time = 0;
        protected bool m_game_over = false;
        protected TaskScheduler<LogicWorld> m_scheduler;

        protected RandomGenerator m_random_generator;

        protected PlayerManager m_player_manager;
        protected EntityManager m_entity_manager;
        protected EffectManager m_effect_manager;
        protected ICommandHandler m_command_handler;
        protected FactionManager m_faction_manager;

        public LogicWorld()
        {
        }

        public virtual void Initialize(IOutsideWorld outside_world, bool need_render_message)
        {
            m_outside_world = outside_world;
            m_need_render_message = need_render_message;
            if (m_need_render_message)
                m_render_messages = new List<RenderMessage>();

            m_scheduler = new TaskScheduler<LogicWorld>(this);

            m_random_generator = new RandomGenerator();

            m_player_manager = new PlayerManager(this);
            m_entity_manager = new EntityManager(this);
            m_effect_manager = new EffectManager(this);
            m_command_handler = CreateCommandHandler();
            m_faction_manager = new FactionManager(this);
        }

        public virtual void Destruct()
        {
            m_scheduler.Destruct();
            m_scheduler = null;
            m_player_manager.Destruct();
            m_player_manager = null;
            m_entity_manager.Destruct();
            m_entity_manager = null;
            m_effect_manager.Destruct();
            m_effect_manager = null;
            m_command_handler.Destruct();
            m_command_handler = null;
        }

        #region GETTER
        public bool NeedRenderMessage
        {
            get { return m_need_render_message; }
        }
        public int CurrentTime
        {
            get { return m_current_time; }
        }
        public int CurrentFrame
        {
            get { return m_current_frame; }
        }
        public int CurrentTurnIndex
        {
            get { return m_current_turn_index; }
        }
        public int CurrentTurnTime
        {
            get { return m_current_turn_time; }
        }
        public TaskScheduler<LogicWorld> GetTaskScheduler()
        {
            return m_scheduler;
        }
        public PlayerManager GetPlayerManager()
        {
            return m_player_manager;
        }
        public EntityManager GetEntityManager()
        {
            return m_entity_manager;
        }
        public EffectManager GetEffectManager()
        {
            return m_effect_manager;
        }
        public FactionManager GetFactionManager()
        {
            return m_faction_manager;
        }
        #endregion

        #region ILogicWorld 可被继承修改
        public virtual void OnStart()
        {
            m_current_time = 0;
            m_current_frame = 0;
            m_current_turn_index = 0;
            m_current_turn_time = 0;
            m_outside_world.OnGameStart();
        }

        public virtual bool OnUpdate(int delta_ms)
        {
            m_current_time += delta_ms;
            ++m_current_frame;
            m_scheduler.Update(m_current_time);
            return m_game_over;
        }

        public bool IsGameOver()
        {
            return m_game_over;
        }

        public void HandleCommand(Command command)
        {
            if (m_game_over)
                return;
            m_command_handler.Handle(command);
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
                return;
            m_render_messages.Add(render_message);
        }

        public void AddSimpleRenderMessage(int type, int entity_id = -1)
        {
            if (m_render_messages == null)
                return;
            SimpleRenderMessage render_message = SimpleRenderMessage.Create(type, entity_id);
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

        #region 可被继承修改
        protected virtual ICommandHandler CreateCommandHandler()
        {
            return new DummyCommandHandler();
        }

        public virtual void BuildLogicWorld(WorldCreationContext world_context)
        {
            m_random_generator.ResetSeed(world_context.m_world_seed);
            m_player_manager.SetPstidAndProxyid(world_context.m_pstid2proxyid, world_context.m_proxyid2pstid);
            ObjectConfig object_config = GlobalConfigManager.Instance.GetObjectConfig();
            for (int i = 0; i < world_context.m_players.Count; ++i)
            {
                ObjectCreationContext context = world_context.m_players[i];
                context.m_logic_world = this;
                context.m_type_data = object_config.GetTypeData(context.m_object_type_id);
                context.m_proto_data = object_config.GetProtoData(context.m_object_proto_id);
                m_player_manager.CreateObject(context);
            }
            for (int i = 0; i < world_context.m_entities.Count; ++i)
            {
                ObjectCreationContext context = world_context.m_entities[i];
                context.m_logic_world = this;
                context.m_type_data = object_config.GetTypeData(context.m_object_type_id);
                context.m_proto_data = object_config.GetProtoData(context.m_object_proto_id);
                m_entity_manager.CreateObject(context);
            }
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
    }
}