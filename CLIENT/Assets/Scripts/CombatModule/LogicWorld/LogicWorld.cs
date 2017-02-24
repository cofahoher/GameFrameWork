using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    enum AttackPhase
    {
        UltimateSKillPhase,
        CommonAttackPhase,
    }

    class TurnInfo
    {
        int m_player_index;
        AttackPhase m_attack_phase;
    }

    public class LogicWorld : ILogicWorld, IRenderMessageGenerator
    {
        IOutsideWorld m_outside_world;
        bool m_need_render_message = false;
        List<RenderMessage> m_render_messages;

        int m_current_time = 0;
        int m_current_frame = 0;
        int m_current_turn_index = 0;
        int m_current_turn_time = 0;
        bool m_game_over = false;
        TaskScheduler m_scheduler;
        TaskScheduler m_turn_scheduler;

        RandomGenerator m_random_generator;

        PlayerManager m_player_manager;
        EntityManager m_entity_manager;
        EffectManager m_effect_manager;
        CommandHandler m_command_handler;
        FactionManager m_faction_manager;

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
        public TaskScheduler GetTaskScheduler()
        {
            return m_scheduler;
        }
        public TaskScheduler GetTurnTaskScheduler()
        {
            return m_turn_scheduler;
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

        public LogicWorld(IOutsideWorld outside_world, bool need_render_message)
        {
            m_outside_world = outside_world;
            m_need_render_message = need_render_message;
            if (m_need_render_message)
                m_render_messages = new List<RenderMessage>();

            m_scheduler = new TaskScheduler(this);
            m_turn_scheduler = new TaskScheduler(this);

            m_random_generator = new RandomGenerator();

            m_player_manager = new PlayerManager(this);
            m_entity_manager = new EntityManager(this);
            m_effect_manager = new EffectManager(this);
            m_command_handler = new CommandHandler(this);
            m_faction_manager = new FactionManager(this);
        }

        public void Destruct()
        {
            m_scheduler.Destruct();
            m_scheduler = null;
            m_turn_scheduler.Destruct();
            m_turn_scheduler = null;
            m_player_manager.Destruct();
            m_player_manager = null;
            m_entity_manager.Destruct();
            m_entity_manager = null;
            m_effect_manager.Destruct();
            m_effect_manager = null;
            m_command_handler.Destruct();
            m_command_handler = null;
        }

        public void OnStart()
        {
            m_current_time = 0;
            m_current_frame = 0;
            m_current_turn_index = 0;
            m_current_turn_time = 0;
        }

        public bool OnUpdate(int delta_ms)
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
            m_command_handler.Handle(command);
        }

        public void CopyFrom(ILogicWorld parallel_world)
        {
        }

        #region 回合制
        public void OnTurnBegin()
        {
            ++m_current_turn_index;
            m_current_turn_time = m_current_turn_index * 10;
            m_turn_scheduler.Update(m_current_turn_time);
        }

        public void OnTurnEnd()
        {
            m_current_turn_time += 1;
            m_turn_scheduler.Update(m_current_turn_time);
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

        public void BuildLogicWorld(WorldCreationContext world_context)
        {
            m_random_generator.ResetSeed(world_context.m_world_seed);
            m_player_manager.SetPstidAndProxyid(world_context.m_pstid2proxyid, world_context.m_proxyid2pstid);
            ObjectConfig object_config = m_outside_world.GetObjectConfig();
            for (int i = 0; i < world_context.m_players.Count; ++i)
            {
                ObjectCreationContext context = world_context.m_players[i];
                context.m_logic_world = this;
                context.m_class_type = typeof(Player);
                context.m_type_data = object_config.GetTypeData(context.m_object_type_id);
                context.m_proto_data = object_config.GetProtoData(context.m_object_proto_id);
                m_player_manager.CreateObject(context);
            }
            for (int i = 0; i < world_context.m_entities.Count; ++i)
            {
                ObjectCreationContext context = world_context.m_entities[i];
                context.m_logic_world = this;
                context.m_class_type = typeof(Entity);
                context.m_type_data = object_config.GetTypeData(context.m_object_type_id);
                context.m_proto_data = object_config.GetProtoData(context.m_object_proto_id);
                m_entity_manager.CreateObject(context);
            }
        }

        public void OnGameOver(bool is_dropout, long winner_player_pstid)
        {
            m_game_over = true;
            m_outside_world.OnGameOver(is_dropout, m_current_frame, winner_player_pstid);
        }
    }
}