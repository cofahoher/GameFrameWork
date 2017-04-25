using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Combat
{
    public class MyRenderWorld : RenderWorld
    {
        CameraController m_camera_controller;
        bool m_draw_grid = true;
        GridGraph m_grid_graph = null;
        List<Vector3> m_current_path = new List<Vector3>();

        public MyRenderWorld()
        {
        }

        public override void Initialize(CombatClient combat_client, LogicWorld logic_world)
        {
            base.Initialize(combat_client, logic_world);
            m_camera_controller = new CameraController(this);
#if UNITY_EDITOR
            MyLogicWorld my_logic_world = logic_world as MyLogicWorld;
            if (m_draw_grid && my_logic_world != null)
            {
                m_grid_graph = my_logic_world.GetGridGraph();
                InitializeDrawGrid();
                GameGlobal.Instance.m_draw_gizmos_callback += DrawGridAndPath;
            }
#endif
        }

        public override void Destruct()
        {
            m_camera_controller.Destruct();
            m_camera_controller = null;
            base.Destruct();
        }

        protected override IRenderMessageProcessor CreateRenderMessageProcessor()
        {
            return new RenderMessageProcessor(this);
        }

        #region GETTER
        public CameraController GetCameraController()
        {
            return m_camera_controller;
        }
        #endregion

        #region RESOURCE
        protected override void OnSceneWasLoaded()
        {
            GameObject camera_go = GameObject.Find("GameGlobal/GameMainCamera/Camera");
            if (camera_go == null)
                LogWrapper.LogError("RenderWorld.LoadScene, GameGlobal/GameMainCamera/Camera");
            else
                m_camera_controller.SetCameraUnityObject(camera_go);
        }
        #endregion

        public override void OnUpdate(int delta_ms, int current_time)
        {
#if UNITY_EDITOR
            MousePick();
            UpdateKeyboardEvent();
#else
            MobilePick();
#endif
            base.OnUpdate(delta_ms, current_time);
        }

        #region 测试操作：点选Entity和键盘控制
        void MousePick()
        {
            if (!Input.GetMouseButtonUp(0))
                return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit))
                return;
            OnPick(hit);
        }

        void MobilePick()
        {
            if (Input.touchCount != 1)
                return;
            if (Input.GetTouch(0).phase != TouchPhase.Began)
                return;
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            if (!Physics.Raycast(ray, out hit))
                return;
            OnPick(hit);
        }

        int m_current_operate_entityi_id = -1;

        void OnPick(RaycastHit hit)
        {
            GameObject go = hit.transform.gameObject;
            UnityObjectBinding binding = go.GetComponent<UnityObjectBinding>();
            if (binding == null)
            {
                Debug.Log("RenderWorld.OnPick(), YOU CHOOSE A POINT" + hit.point.ToString());
                if (m_current_operate_entityi_id < 0)
                    return;
                RenderEntity render_entity = m_render_entity_manager.GetObject(m_current_operate_entityi_id);
                if (render_entity == null)
                    return;
                LocomotorComponent locomotor_component = render_entity.GetLogicEntity().GetComponent<LocomotorComponent>();
                if (locomotor_component == null || !locomotor_component.IsEnable())
                    return;
                //EntityMoveCommand cmd = Command.Create<EntityMoveCommand>();
                //cmd.m_entity_id = m_current_operate_entityi_id;
                //cmd.m_move_type = EntityMoveCommand.DestinationType;
                //cmd.m_vector = Vector3_To_Vector3FP(hit.point);
                //PushLocalCommand(cmd);
                if (m_draw_grid && m_grid_graph != null)
                {
                    PositionComponent position_component = render_entity.GetLogicEntity().GetComponent<PositionComponent>();
                    if (!m_grid_graph.FindPath(position_component.CurrentPosition, Vector3_To_Vector3FP(hit.point)))
                        Debug.LogError("FindPath Failed!");
                    else
                    {
                        List<Vector3FP> path = m_grid_graph.GetPath();
                        m_current_path.Clear();
                        for (int i = 0; i < path.Count; ++i)
                        {
                            m_current_path.Add(Vector3FP_To_Vector3(path[i]));
                        }
                    }
                }
            }
            else
            {
                RenderEntity render_entity = m_render_entity_manager.GetObject(binding.EntityID);
                if (render_entity == null)
                    return;
                if (m_combat_client.LocalPlayerPstid == m_logic_world.GetPlayerManager().Objectid2Pstid(render_entity.GetOwnerPlayerID()))
                {
                    m_current_operate_entityi_id = binding.EntityID;
                    Debug.Log("RenderWorld.OnPick(), YOU CHOOSE YOUR ENTITY " + hit.transform.parent.name);
                }
                else
                {
                    m_current_operate_entityi_id = -1;
                    Debug.Log("RenderWorld.OnPick(), " + hit.transform.parent.name + " IS NOT YOUR Entity");
                }
            }
        }

        Vector3FP m_direction = new Vector3FP();
        int m_wsad_state = 0;
        const int W_STATE = 1 << 0;
        const int S_STATE = 1 << 1;
        const int A_STATE = 1 << 2;
        const int D_STATE = 1 << 3;

        void UpdateKeyboardEvent()
        {
            if (m_current_operate_entityi_id < 0)
                return;
            int old_state = m_wsad_state;
            if (Input.GetKeyDown(KeyCode.W))
                m_wsad_state |= W_STATE;
            if (Input.GetKeyDown(KeyCode.S))
                m_wsad_state |= S_STATE;
            if (Input.GetKeyDown(KeyCode.A))
                m_wsad_state |= A_STATE;
            if (Input.GetKeyDown(KeyCode.D))
                m_wsad_state |= D_STATE;
            if (Input.GetKeyUp(KeyCode.W))
                m_wsad_state &= ~W_STATE;
            if (Input.GetKeyUp(KeyCode.S))
                m_wsad_state &= ~S_STATE;
            if (Input.GetKeyUp(KeyCode.A))
                m_wsad_state &= ~A_STATE;
            if (Input.GetKeyUp(KeyCode.D))
                m_wsad_state &= ~D_STATE;
            if (m_wsad_state == old_state)
                return;
            if (m_wsad_state == 0)
            {
                EntityMoveCommand cmd = Command.Create<EntityMoveCommand>();
                cmd.m_entity_id = m_current_operate_entityi_id;
                cmd.m_move_type = EntityMoveCommand.StopMoving;
                PushLocalCommand(cmd);
            }
            else
            {
                RenderEntity render_entity = m_render_entity_manager.GetObject(m_current_operate_entityi_id);
                if (render_entity == null)
                    return;
                m_direction.MakeZero();
                if ((m_wsad_state & W_STATE) != 0)
                    m_direction.z += FixPoint.One;
                if ((m_wsad_state & S_STATE) != 0)
                    m_direction.z -= FixPoint.One;
                if ((m_wsad_state & A_STATE) != 0)
                    m_direction.x -= FixPoint.One;
                if ((m_wsad_state & D_STATE) != 0)
                    m_direction.x += FixPoint.One;
                m_direction.Normalize();
                EntityMoveCommand cmd = Command.Create<EntityMoveCommand>();
                cmd.m_entity_id = m_current_operate_entityi_id;
                cmd.m_move_type = EntityMoveCommand.DirectionType;
                cmd.m_vector = m_direction;
                PushLocalCommand(cmd);
            }
        }
        #endregion

#if UNITY_EDITOR
        List<Vector3> m_draw_nodes = new List<Vector3>();
        Vector3 m_draw_size = new Vector3(0.3f, 0.1f, 0.3f);
        readonly Color UNWALKABLE_COLOR = new Color(1f, 0, 0, 0.5f);
        readonly Color WALKABLE_COLOR = new Color(0, 1f, 0, 0.5f);
        readonly Color PATH_COLOR = new Color(0, 0, 1f, 1f);

        void InitializeDrawGrid()
        {
            if (m_grid_graph == null)
                return;
            List<GridNode> logic_nodes = m_grid_graph.GetAllNodes();
            GridNode node;
            for (int i = 0; i < logic_nodes.Count; ++i)
            {
                node = logic_nodes[i];
                Vector3 pos = Vector3FP_To_Vector3(m_grid_graph.Node2Position(node));
                pos.y = 0.1f;
                m_draw_nodes.Add(pos);
            }
            float grid_size = (float)m_grid_graph.GetGridSize() * 0.75f;
            m_draw_size = new Vector3(grid_size, 0.1f, grid_size);
        }

        public void DrawGridAndPath()
        {
            if (m_grid_graph == null)
                return;
            List<GridNode> logic_nodes = m_grid_graph.GetAllNodes();
            for (int i = 0; i < m_draw_nodes.Count; ++i)
            {
                if (logic_nodes[i].Walkable)
                {
                    Gizmos.color = WALKABLE_COLOR;
                    Gizmos.DrawWireCube(m_draw_nodes[i], m_draw_size);
                }
                else
                {
                    Gizmos.color = UNWALKABLE_COLOR;
                    Gizmos.DrawWireCube(m_draw_nodes[i], m_draw_size);
                }
            }
            Gizmos.color = PATH_COLOR;
            for (int i = 1; i < m_current_path.Count; ++i)
            {
                Gizmos.DrawLine(m_current_path[i], m_current_path[i-1]);
            }
        }
#endif
    }
}