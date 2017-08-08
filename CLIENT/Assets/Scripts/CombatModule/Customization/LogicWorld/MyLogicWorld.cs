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
        SceneSpace m_scene_space;

        protected override void PostInitialize()
        {
            m_turn_scheduler = new TaskScheduler<LogicWorld>(this);

            FixPoint grid_size = FixPoint.One;
            FixPoint seeker_radius = FixPoint.One / FixPoint.FixPointDigit[4];
            FixPoint x_size = new FixPoint(40);
            FixPoint z_size = new FixPoint(30);
            Vector3FP left_bottom_position = new Vector3FP(new FixPoint(-20), FixPoint.Zero, new FixPoint(-15));

            SquareGridGraph grid_graph = new SquareGridGraph();
            //m_grid_graph = new HexagonGridGraph();
            grid_graph.GenerateAsPlaneMap(grid_size, x_size, z_size, FixPoint.Zero, left_bottom_position, seeker_radius);
            grid_graph.CoverArea(new Vector3FP(FixPoint.Zero, FixPoint.Zero, FixPoint.FixPointDigit[5]), new Vector3FP(FixPoint.FixPointDigit[7] + FixPoint.Half, FixPoint.Zero, FixPoint.Half));
            grid_graph.CoverArea(new Vector3FP(FixPoint.FixPointDigit[7] + FixPoint.Half, FixPoint.Zero, FixPoint.Zero), new Vector3FP(FixPoint.Half, FixPoint.Zero, FixPoint.FixPointDigit[5]));
            grid_graph.CoverArea(new Vector3FP(-FixPoint.FixPointDigit[7] - FixPoint.Half, FixPoint.Zero, FixPoint.Two), new Vector3FP(FixPoint.Half, FixPoint.Zero, FixPoint.FixPointDigit[7] + FixPoint.Half));

            CellSpacePartition space_partition = new CellSpacePartition(this, x_size, z_size, left_bottom_position);

            m_scene_space = new SceneSpace();
            m_scene_space.m_space_id = 0;
            m_scene_space.m_min_position = left_bottom_position;
            m_scene_space.m_max_position = new Vector3FP(left_bottom_position.x + x_size, FixPoint.Zero, left_bottom_position.z + z_size);
            m_scene_space.m_graph = grid_graph;
            m_scene_space.m_paitition = space_partition;
        }

        public override void Destruct()
        {
            m_turn_scheduler.Destruct();
            m_turn_scheduler = null;
            base.Destruct();
            m_scene_space.Destruct();
            m_scene_space = null;
        }

        public override SceneSpace GetDefaultSceneSpace()
        {
            return m_scene_space;
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