using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class MNLPPlayerWorldSynchronizer : WorldSynchronizer
    {
        public MNLPPlayerWorldSynchronizer(ILogicWorld logic_world, ICommandSynchronizer command_synchronizer)
            : base(logic_world, command_synchronizer)
        {
        }
    }

    public class MNLPServerWorldSynchronizer : WorldSynchronizer
    {
        ISyncServer m_sync_server;
        int m_forward_start_time = 0;
        int m_unturned_frame_count = 0;
        bool m_game_over = false;

        public MNLPServerWorldSynchronizer(ISyncServer sync_server, ILogicWorld logic_world, ICommandSynchronizer command_synchronizer)
            : base(logic_world, command_synchronizer)
        {
            m_sync_server = sync_server;
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
            return m_command_synchronizer.AddCommand(command);
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
}