using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class SPPlayerWorldSynchronizer : WorldSynchronizer
    {
        int m_forward_start_time = 0;
        int m_unturned_frame_count = 0;
        bool m_game_over = false;

        const int MAX_FRONT_PREDICT_PER_UPDATE = 100;

        public SPPlayerWorldSynchronizer(ILogicWorld logic_world)
        {
            m_logic_world = logic_world;
            m_command_synchronizer = new SPPredictedCommandSynchronizer();
        }

        public override void Start(int start_time)
        {
            m_forward_start_time = start_time;
            base.Start(start_time);
        }

        public override bool PushLocalCommand(Command command)
        {
            return m_command_synchronizer.AddCommand(command);
        }
        public override bool PushClientCommand(Command command)
        {
            return false;
        }
        public override bool PushServerCommand(Command command)
        {
            return false;
        }

        public override bool ForwardFrame(int forward_end_time)
        {
            if (m_game_over)
                return false;
            int frame_diff = (forward_end_time - m_forward_start_time) / SyncParam.FRAME_TIME;
            if (frame_diff <= 0)
                return false;
            else if (frame_diff > MAX_FRONT_PREDICT_PER_UPDATE)
                frame_diff = MAX_FRONT_PREDICT_PER_UPDATE;
            for (int i = 0; i < frame_diff; ++i)
            {
                m_game_over = UpdateLogicFrame();
                ++m_unturned_frame_count;
                if (m_unturned_frame_count == SyncParam.FRAME_COUNT_PER_SYNCTURN)
                {
                    ++m_synchronized_turn;
                    m_unturned_frame_count = 0;
                    List<Command> commands = m_command_synchronizer.GetCommands(m_synchronized_turn);
                    if (commands.Count > 0)
                    {
                        for (int j = 0; j < commands.Count; ++j)
                            m_logic_world.HandleCommand(commands[j]);
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

    public class SPWatcherWorldSynchronizer : WorldSynchronizer
    {
        int m_forward_start_time = 0;
        int m_unturned_frame_count = 0;
        bool m_game_over = false;

        public SPWatcherWorldSynchronizer(ILogicWorld logic_world)
        {
            m_logic_world = logic_world;
            m_command_synchronizer = new SPTrustedCommandSynchronizer();
        }

        public override void Start(int start_time)
        {
            m_forward_start_time = start_time;
            base.Start(start_time);
        }

        public override bool PushLocalCommand(Command command)
        {
            return false;
        }
        public override bool PushClientCommand(Command command)
        {
            return false;
        }
        public override bool PushServerCommand(Command command)
        {
            return m_command_synchronizer.AddCommand(command);
        }

        public override bool ForwardFrame(int forward_end_time)
        {
            if (m_game_over)
                return false;
            int frame_diff = (forward_end_time - m_forward_start_time) / SyncParam.FRAME_TIME;
            if (frame_diff <= 0)
                return false;
            int ready_turn = m_command_synchronizer.GetReadyTurn();
            int turn_diff = ready_turn - m_synchronized_turn;
            if (turn_diff <= 0)
                return false;
            int max_frame_diff = turn_diff * SyncParam.FRAME_COUNT_PER_SYNCTURN - m_unturned_frame_count;
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
                    if (commands.Count > 0)
                    {
                        for (int j = 0; j < commands.Count; ++j)
                            m_logic_world.HandleCommand(commands[j]);
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
        bool m_game_over = false;

        public SPCheckerWorldSynchronizer(ILogicWorld logic_world)
        {
            m_logic_world = logic_world;
            m_command_synchronizer = new SPTrustedCommandSynchronizer();
        }        

        public override bool PushLocalCommand(Command command)
        {
            return false;
        }
        public override bool PushClientCommand(Command command)
        {
            return m_command_synchronizer.AddCommand(command);
        }
        public override bool PushServerCommand(Command command)
        {
            return false;
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
                if (commands.Count > 0)
                {
                    for (int j = 0; j < commands.Count; ++j)
                        m_logic_world.HandleCommand(commands[j]);
                }
            }
            return true;
        }
    }
}