using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class SPPlayerWorldSynchronizer : WorldSynchronizer
    {
        const int MAX_FRAME_FORWARD_PER_UPDATE = 100;
        int m_synchronized_turn = -1;
        int m_forward_start_time = 0;
        int m_unturned_frame_count = 0;
        bool m_game_over = false;

        public SPPlayerWorldSynchronizer(ILogicWorld logic_world, ICommandSynchronizer command_synchronizer)
            : base(logic_world, command_synchronizer)
        {
        }

        public override int GetSynchronizedTurn()
        {
            return m_synchronized_turn;
        }

        public override void Start(int start_time)
        {
            m_forward_start_time = start_time;
            m_logic_world.OnStart();
        }

        public override bool ForwardFrame(int forward_end_time)
        {
            if (m_game_over && m_unturned_frame_count == 0)
                return false;
            int frame_diff = (forward_end_time - m_forward_start_time) / SyncParam.FRAME_TIME;
            if (frame_diff <= 0)
                return false;
            if (frame_diff > MAX_FRAME_FORWARD_PER_UPDATE)
                frame_diff = MAX_FRAME_FORWARD_PER_UPDATE;
            for (int i = 0; i < frame_diff; ++i)
            {
                m_game_over = UpdateLogicFrame();
                ++m_unturned_frame_count;
                if (m_unturned_frame_count == SyncParam.FRAME_COUNT_PER_SYNCTURN)
                {
                    ++m_synchronized_turn;
                    m_unturned_frame_count = 0;
                    List<Command> commands = m_command_synchronizer.GetCommands(m_synchronized_turn);
                    if (commands != null)
                    {
                        for (int j = 0; j < commands.Count; ++j)
                            m_logic_world.HandleCommand(commands[j]);
                        m_command_synchronizer.ClearCommands(m_synchronized_turn);
                    }
                    if (m_game_over)
                        break;
                }
                m_forward_start_time += SyncParam.FRAME_TIME;
            }
            return true;
        }

        public override bool ForwardTurn()
        {
            return false;
        }
    }

    public class SPWatcherWorldSynchronizer : WorldSynchronizer
    {
        int m_synchronized_turn = -1;
        int m_forward_start_time = 0;
        int m_unturned_frame_count = 0;
        bool m_game_over = false;
        bool m_blocked = false;

        public SPWatcherWorldSynchronizer(ILogicWorld logic_world, ICommandSynchronizer command_synchronizer)
            : base(logic_world, command_synchronizer)
        {
        }

        public override int GetSynchronizedTurn()
        {
            return m_synchronized_turn;
        }

        public override void Start(int start_time)
        {
            m_forward_start_time = start_time;
            m_logic_world.OnStart();
        }

        public override bool ForwardFrame(int forward_end_time)
        {
            if (m_game_over && m_unturned_frame_count == 0)
                return false;
            int frame_diff = (forward_end_time - m_forward_start_time) / SyncParam.FRAME_TIME;
            if (frame_diff <= 0)
                return false;
            int ready_turn = m_command_synchronizer.GetReadyTurn();
            int max_frame_diff = (ready_turn - m_synchronized_turn + 1) * SyncParam.FRAME_COUNT_PER_SYNCTURN - m_unturned_frame_count - 1;
            if (max_frame_diff <= 0)
            {
                m_blocked = true;
                return false;
            }
            m_blocked = false;
            if (frame_diff > max_frame_diff)
                frame_diff = max_frame_diff;
            for (int i = 0; i < frame_diff; ++i)
            {
                m_game_over = UpdateLogicFrame();
                ++m_unturned_frame_count;
                if (m_unturned_frame_count == SyncParam.FRAME_COUNT_PER_SYNCTURN)
                {
                    ++m_synchronized_turn;
                    m_unturned_frame_count = 0;
                    List<Command> commands = m_command_synchronizer.GetCommands(m_synchronized_turn);
                    if (commands != null)
                    {
                        for (int j = 0; j < commands.Count; ++j)
                            m_logic_world.HandleCommand(commands[j]);
                        m_command_synchronizer.ClearCommands(m_synchronized_turn);
                    }
                }
                m_forward_start_time += SyncParam.FRAME_TIME;
            }
            return true;
        }

        public override bool ForwardTurn()
        {
            return false;
        }
    }

    public class SPCheckerWorldSynchronizer : WorldSynchronizer
    {
        int m_synchronized_turn = -1;
        bool m_game_over = false;

        public SPCheckerWorldSynchronizer(ILogicWorld logic_world, ICommandSynchronizer command_synchronizer)
            : base(logic_world, command_synchronizer)
        {
        }

        public override int GetSynchronizedTurn()
        {
            return m_synchronized_turn;
        }

        public override void Start(int start_time)
        {
            m_logic_world.OnStart();
        }

        public override bool ForwardFrame(int forward_end_time)
        {
            return false;
        }

        public override bool ForwardTurn()
        {
            if (m_game_over)
                return false;
            int ready_turn = m_command_synchronizer.GetReadyTurn();
            int turn_diff = ready_turn - m_synchronized_turn;
            if (turn_diff <= 0)
                return false;
            for (int i = 0; i < turn_diff; ++i)
            {
                m_game_over = UpdateLogicTurn();
                ++m_synchronized_turn;
                List<Command> commands = m_command_synchronizer.GetCommands(m_synchronized_turn);
                if (commands != null)
                {
                    for (int j = 0; j < commands.Count; ++j)
                        m_logic_world.HandleCommand(commands[j]);
                    m_command_synchronizer.ClearCommands(m_synchronized_turn);
                }
            }
            return true;
        }
    }
}