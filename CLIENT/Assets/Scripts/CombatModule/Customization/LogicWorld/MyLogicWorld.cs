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
        FixPoint m_current_turn_index = FixPoint.Zero;
        FixPoint m_current_turn_time = FixPoint.Zero;
        TaskScheduler<LogicWorld> m_turn_scheduler;

        public MyLogicWorld()
        {
        }

        public override void Initialize(IOutsideWorld outside_world, bool need_render_message)
        {
            base.Initialize(outside_world, need_render_message);
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
        public FixPoint CurrentTurnIndex
        {
            get { return m_current_turn_index; }
        }
        public FixPoint CurrentTurnTime
        {
            get { return m_current_turn_time; }
        }
        public TaskScheduler<LogicWorld> GetTurnTaskScheduler()
        {
            return m_turn_scheduler;
        }
        #endregion

        #region ILogicWorld
        public override void OnStart()
        {
            m_current_turn_index = FixPoint.Zero;
            m_current_turn_time = FixPoint.Zero;
            base.OnStart();
        }
        #endregion

        #region 回合制
        public void OnTurnBegin()
        {
            m_current_turn_index += FixPoint.One;
            m_current_turn_time = m_current_turn_index * FixPoint.Ten;
            m_turn_scheduler.Update(m_current_turn_time);
        }

        public void OnTurnEnd()
        {
            m_current_turn_time += FixPoint.One;
            m_turn_scheduler.Update(m_current_turn_time);
        }
        #endregion
    }
}