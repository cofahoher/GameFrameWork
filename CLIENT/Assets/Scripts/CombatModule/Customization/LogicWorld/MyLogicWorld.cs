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

            m_grid_graph = new SquareGridGraph();
            //m_grid_graph = new HexagonGridGraph();
            FixPoint grid_size = FixPoint.One;
            FixPoint seeker_radius = FixPoint.One / FixPoint.FixPointDigit[4];
            FixPoint x_size = new FixPoint(40);
            FixPoint z_size = new FixPoint(30);
            Vector3FP left_bottom_position = new Vector3FP(new FixPoint(-20), FixPoint.Zero, new FixPoint(-15));
            m_grid_graph.GenerateAsPlaneMap(grid_size, x_size, z_size, FixPoint.Zero, left_bottom_position, seeker_radius);
            m_grid_graph.CoverArea(new Vector3FP(FixPoint.Zero, FixPoint.Zero, FixPoint.FixPointDigit[5]), new Vector3FP(FixPoint.FixPointDigit[7] + FixPoint.Half, FixPoint.Zero, FixPoint.Half));
            m_grid_graph.CoverArea(new Vector3FP(FixPoint.FixPointDigit[7] + FixPoint.Half, FixPoint.Zero, FixPoint.Zero), new Vector3FP(FixPoint.Half, FixPoint.Zero, FixPoint.FixPointDigit[5]));
            m_grid_graph.CoverArea(new Vector3FP(-FixPoint.FixPointDigit[7] - FixPoint.Half, FixPoint.Zero, FixPoint.Two), new Vector3FP(FixPoint.Half, FixPoint.Zero, FixPoint.FixPointDigit[7] + FixPoint.Half));
            m_space_manager = new CellSpaceManager(this, x_size, z_size, left_bottom_position);
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