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
        GridGraph m_grid_graph = new GridGraph();

        public MyLogicWorld()
        {
        }

        public override void Initialize(IOutsideWorld outside_world, bool need_render_message)
        {
            base.Initialize(outside_world, need_render_message);
            m_turn_scheduler = new TaskScheduler<LogicWorld>(this);

            FixPoint seeker_radius = FixPoint.One / FixPoint.FixPointDigit[4];
            m_grid_graph.GenerateAsPlaneMap(new FixPoint(40), new FixPoint(30), FixPoint.One, seeker_radius, new Vector3FP(new FixPoint(-20), FixPoint.Zero, new FixPoint(-15)));
            m_grid_graph.CoverArea(new Vector3FP(FixPoint.Zero, FixPoint.Half, FixPoint.FixPointDigit[5]), new Vector3FP(FixPoint.FixPointDigit[7] + FixPoint.Half, FixPoint.Half, FixPoint.Half));
            m_grid_graph.CoverArea(new Vector3FP(FixPoint.FixPointDigit[7] + FixPoint.Half, FixPoint.Half, FixPoint.Zero), new Vector3FP(FixPoint.Half, FixPoint.Half, FixPoint.FixPointDigit[5]));
            m_grid_graph.CoverArea(new Vector3FP(-FixPoint.FixPointDigit[7] - FixPoint.Half, FixPoint.Half, FixPoint.Two), new Vector3FP(FixPoint.Half, FixPoint.Half, FixPoint.FixPointDigit[7] + FixPoint.Half));
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
        public GridGraph GetGridGraph()
        {
            return m_grid_graph;
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