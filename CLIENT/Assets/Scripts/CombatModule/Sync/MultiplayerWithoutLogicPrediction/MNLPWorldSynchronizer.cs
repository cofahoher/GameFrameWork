using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class MNLPPlayerWorldSynchronizer : WorldSynchronizer
    {
        int m_forward_start_time = 0;
        int m_unturned_frame_count = 0;
        bool m_game_over = false;
        bool m_blocked = false;

        public MNLPPlayerWorldSynchronizer(ILogicWorld logic_world, ICommandSynchronizer command_synchronizer)
            : base(logic_world, command_synchronizer)
        {
        }

        public override void Start(int start_time)
        {
            m_forward_start_time = start_time;
            base.Start(start_time);
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
                m_forward_start_time += SyncParam.FRAME_TIME;
                if (m_unturned_frame_count == SyncParam.FRAME_COUNT_PER_SYNCTURN)
                {
                    ++m_synchronized_turn;
                    m_unturned_frame_count = 0;
                    if ((m_synchronized_turn + 1) * SyncParam.FRAME_COUNT_PER_SYNCTURN != m_logic_world.GetCurrentFrame())
                        m_unturned_frame_count = 0;
                    List<Command> commands = m_command_synchronizer.GetCommands(m_synchronized_turn);
                    if (commands != null && commands.Count > 0)
                    {
                        for (int j = 0; j < commands.Count; ++j)
                            m_logic_world.HandleCommand(commands[j]);
                    }
                    if (m_game_over)
                        break;
                }
            }
            return true;
        }
    }

    public class MNLPServerWorldSynchronizer : WorldSynchronizer
    {
        int m_forward_start_time = 0;
        int m_unturned_frame_count = 0;
        bool m_game_over = false;

        public MNLPServerWorldSynchronizer(ILogicWorld logic_world, ICommandSynchronizer command_synchronizer)
            : base(logic_world, command_synchronizer)
        {
            m_synchronized_turn = -1;
            m_unturned_frame_count = 0;
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
            return false;
        }

        public override bool ForwardFrame(int forward_end_time)
        {
            if (m_game_over && m_unturned_frame_count == 0)
                return false;
            int frame_diff = (forward_end_time - m_forward_start_time) / SyncParam.FRAME_TIME;
            if (frame_diff <= 0)
                return false;
            for (int i = 0; i < frame_diff; ++i)
            {
                m_game_over = UpdateLogicFrame();
                ++m_unturned_frame_count;
                m_forward_start_time += SyncParam.FRAME_TIME;
                if (m_unturned_frame_count == SyncParam.FRAME_COUNT_PER_SYNCTURN)
                {
                    if ((m_synchronized_turn + 2) * SyncParam.FRAME_COUNT_PER_SYNCTURN != m_logic_world.GetCurrentFrame())
                        m_unturned_frame_count = 0;
                    ++m_synchronized_turn;
                    m_unturned_frame_count = 0;
                    List<Command> commands = m_command_synchronizer.GetCommands(m_synchronized_turn);
                    if (commands != null && commands.Count > 0)
                    {
                        for (int j = 0; j < commands.Count; ++j)
                            m_logic_world.HandleCommand(commands[j]);
                    }
                    if (m_game_over)
                        break;
                }
            }
            return true;
        }

        public override bool ForwardTurn()
        {
            return false;
        }
    }
}