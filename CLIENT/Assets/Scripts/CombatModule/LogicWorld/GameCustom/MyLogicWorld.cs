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

    public class MyLogicWorld : LogicWorld
    {
        protected TaskScheduler<LogicWorld> m_turn_scheduler;

        public MyLogicWorld(IOutsideWorld outside_world, bool need_render_message)
            : base(outside_world, need_render_message)
        {
            m_turn_scheduler = new TaskScheduler<LogicWorld>(this);
        }

        public override void Destruct()
        {
            m_turn_scheduler.Destruct();
            m_turn_scheduler = null;
            base.Destruct();
        }

        protected override ICommandHandler CreateCommandHandler()
        {
            return new CommandHandler(this);
        }

        #region GETTER
        public TaskScheduler<LogicWorld> GetTurnTaskScheduler()
        {
            return m_turn_scheduler;
        }
        #endregion
        
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
    }
}