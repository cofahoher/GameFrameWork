using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class SyncParam
    {
        /* 
         * 同步的配置
         * FRAME_TIME是逻辑的更新间隔，SYNCTURN_TIME是同步回合的间隔
         * frame和syncturn都是0起的
         * 发送的command中包含的syncturn表示这个命令在哪个syncturn中发生，SyncTurnDone表示哪个syncturn结束
         * 客户端：对自己的command是立即发送的，但不立即处理，等到一个syncturn结束时，再处理该syncturn的中的command
         * 客户端：不频繁的发送SyncTurnDone，每累计SYNCTURN_COUNT_TO_SEND发送一次
         * LogicWorld的处理顺序是先更新frame，等到一个syncturn结束时再处理command
         */
        public const int FRAME_TIME = 30;
        public const int FRAME_COUNT_PER_SYNCTURN = 1;
        public const int SYNCTURN_TIME = FRAME_TIME * FRAME_COUNT_PER_SYNCTURN;
        public const int SYNCTURN_COUNT_TO_SEND = 30;
        public const int COMMAND_DELAY_SYNCTURN = 1;
        public const int MAX_LATENCY = 1000;
    }

    public interface ILogicWorld : IDestruct
    {
        void OnStart();
        bool OnUpdate(int delta_ms);
        bool IsGameOver();
        void HandleCommand(Command command);
        void CopyFrom(ILogicWorld parallel_world);
    }

    public interface IOutsideWorld : IDestruct
    {
        int GetCurrentTime();
        void OnGameStart();
        void OnGameOver(bool is_dropout, int end_frame, long winner_player_pstid);
        #region 配置，暂时先放这里
        LevelConfig GetLevelConfig();
        ObjectConfig GetObjectConfig();
        #endregion
    }

    public interface ISyncClient : IDestruct
    {
        void Init(ILogicWorld logic_world, IOutsideWorld outside_world);
        void AddPlayer(long player_pstid);
        void RemovePlayer(long player_pstid);
        void Start(int current_time, long local_player_pstid, int latency);
        void Stop();
        void Update(int current_time);
        void PushLocalCommand(Command command);
        void PushServerCommand(Command command);
        void AddOutputCommand(Command command);
        List<Command> GetOutputCommands();
        void ClearOutPutCommand();
    }

    public interface ISyncServer : IDestruct
    {
        void Init(ILogicWorld logic_world);
        void AddPlayer(long player_pstid);
        void RemovePlayer(long player_pstid);
        void Start(int current_time);
        void Update(int current_time);
        void PushClientCommand(Command command);
        void AddOutputCommand(Command command);
        List<Command> GetOutputCommands();
        void ClearOutPutCommand();
    }

    public interface IWorldSynchronizer : IDestruct
    {
        int GetSynchronizedTurn();
        void Start(int start_time);
        void AddPlayer(long player_pstid);
        void RemovePlayer(long player_pstid);
        bool PushLocalCommand(Command command);
        bool PushClientCommand(Command command);
        bool PushServerCommand(Command command);
        bool ForwardFrame(int forward_end_time);
        bool ForwardTurn();
    }

    public interface ICommandSynchronizer : IDestruct
    {
        void AddPlayer(long player_pstid);
        void RemovePlayer(long player_pstid);
        bool AddCommand(Command command);
        List<Command> GetCommands(int syncturn);
        int GetReadyTurn();
    }

    public class PlayerSyncData
    {
        public const int SYNC_STATE_OK = 0;
        public const int SYNC_STATE_IDLE = 1;
        public const int SYNC_STATE_DEAD = 2;
        int m_sync_state = SYNC_STATE_OK;
        int m_last_msg_syncturn = -1;
        public int SyncState
        {
            get { return m_sync_state; }
            set { m_sync_state = value; }
        }
        public int LastMsgSyncTurn
        {
            get { return m_last_msg_syncturn; }
            set { m_last_msg_syncturn = value; }
        }
    }

    public class PlayerTurnState
    {
        //已知的进行中的（尚未完成）最高的syncturn
        int m_syncturn = 0;
        //是否在同步中
        bool m_valid = true;
        public int SyncTurn
        {
            get { return m_syncturn; }
            set { m_syncturn = value; }
        }
        public bool Valid
        {
            get { return m_valid; }
            set { m_valid = value; }
        }
    }

    public class DummyLogicWorld : ILogicWorld
    {
        public void Destruct()
        {
        }
        public void OnStart()
        {
        }
        public bool OnUpdate(int delta_ms)
        {
            return false;
        }
        public bool IsGameOver()
        {
            return false;
        }
        public void HandleCommand(Command command)
        {
        }
        public void CopyFrom(ILogicWorld parallel_world)
        {
        }
    }
    
    public class SyncClient : ISyncClient
    {
        protected ILogicWorld m_logic_world;
        protected IOutsideWorld m_outside_world;
        protected IWorldSynchronizer m_world_syhchronizer;
        protected List<Command> m_output_commands = new List<Command>();

        public virtual void Destruct()
        {
            m_logic_world = null;
            m_outside_world = null;
            if (m_world_syhchronizer != null)
            {
                m_world_syhchronizer.Destruct();
                m_world_syhchronizer = null;
            }
        }
        public virtual void Init(ILogicWorld logic_world, IOutsideWorld outside_world)
        {
            m_logic_world = logic_world;
            m_outside_world = outside_world;
        }
        public virtual void AddPlayer(long player_pstid)
        {
            m_world_syhchronizer.AddPlayer(player_pstid);
        }
        public virtual void RemovePlayer(long player_pstid)
        {
            m_world_syhchronizer.RemovePlayer(player_pstid);
        }
        public virtual void Start(int current_time, long local_player_pstid, int latency)
        {
        }
        public virtual void Stop()
        {
        }
        public virtual void Update(int current_time)
        {
        }
        public virtual void PushLocalCommand(Command command)
        {
        }
        public virtual void PushServerCommand(Command command)
        {
        }
        public void AddOutputCommand(Command command)
        {
            m_output_commands.Add(command);
        }
        public List<Command> GetOutputCommands()
        {
            return m_output_commands;
        }
        public void ClearOutPutCommand()
        {
            m_output_commands.Clear();
        }
    }

    public class SyncServer : ISyncServer
    {
        protected ILogicWorld m_logic_world;
        protected IWorldSynchronizer m_world_syhchronizer;
        protected List<Command> m_output_commands = new List<Command>();

        public virtual void Destruct()
        {
            m_logic_world = null;
            if (m_world_syhchronizer != null)
            {
                m_world_syhchronizer.Destruct();
                m_world_syhchronizer = null;
            }
        }
        public virtual void Init(ILogicWorld logic_world)
        {
            m_logic_world = logic_world;
        }
        public virtual void AddPlayer(long player_pstid)
        {
        }
        public virtual void RemovePlayer(long player_pstid)
        {
        }
        public virtual void Start(int current_time)
        {
        }
        public virtual void Update(int current_time)
        {
        }
        public virtual void PushClientCommand(Command command)
        {
        }
        public void AddOutputCommand(Command command)
        {
            m_output_commands.Add(command);
        }
        public List<Command> GetOutputCommands()
        {
            return m_output_commands;
        }
        public void ClearOutPutCommand()
        {
            m_output_commands.Clear();
        }
    }

    public class WorldSynchronizer : IWorldSynchronizer
    {
        protected ILogicWorld m_logic_world;
        protected ICommandSynchronizer m_command_synchronizer;
        protected int m_synchronized_turn = -1;

        public virtual void Destruct()
        {
            m_logic_world = null;
            m_command_synchronizer.Destruct();
        }
        public int GetSynchronizedTurn()
        {
            return m_synchronized_turn;
        }
        public virtual void Start(int start_time)
        {
            m_logic_world.OnStart();
        }
        public virtual void AddPlayer(long player_pstid)
        {
            m_command_synchronizer.AddPlayer(player_pstid);
        }
        public virtual void RemovePlayer(long player_pstid)
        {
            m_command_synchronizer.RemovePlayer(player_pstid);
        }
        public virtual bool PushLocalCommand(Command command)
        {
            return false;
        }
        public virtual bool PushClientCommand(Command command)
        {
            return false;
        }
        public virtual bool PushServerCommand(Command command)
        {
            return false;
        }
        public virtual bool ForwardFrame(int forward_end_time)
        {
            return false;
        }
        public virtual bool ForwardTurn()
        {
            return false;
        }
        protected bool UpdateLogicFrame()
        {
            return m_logic_world.OnUpdate(SyncParam.FRAME_TIME);
        }
        protected bool UpdateLogicTurn()
        {
            for (int i = 0; i < SyncParam.FRAME_COUNT_PER_SYNCTURN; ++i)
            {
                if (m_logic_world.OnUpdate(SyncParam.FRAME_TIME))
                    return true;
            }
            return false;
        }
    }

    public class CommandSynchronizer : ICommandSynchronizer
    {
        protected SortedDictionary<int, List<Command>> m_syncturn2commands = new SortedDictionary<int, List<Command>>();

        public virtual void Destruct()
        {
            m_syncturn2commands.Clear();
        }
        public virtual void AddPlayer(long player_pstid)
        {
        }
        public virtual void RemovePlayer(long player_pstid)
        {
        }
        public virtual bool AddCommand(Command command)
        {
            if (command.Type == CommandType.SyncTurnDone)
                return true;
            int syncturn = command.SyncTurn;
            List<Command> commands;
            if (!m_syncturn2commands.TryGetValue(syncturn, out commands))
            {
                commands = new List<Command>();
                m_syncturn2commands[syncturn] = commands;
            }
            commands.Add(command);
            return true;
        }
        public List<Command> GetCommands(int syncturn)
        {
            List<Command> commands;
            m_syncturn2commands.TryGetValue(syncturn, out commands);
            return commands;
        }
        public virtual int GetReadyTurn()
        {
            return -1;
        }
    }
}