using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Combat
{
    public class MyRenderWorld : RenderWorld
    {
        CameraController m_camera_controller;

        public MyRenderWorld()
        {
        }

        public override void Initialize(CombatClient combat_client, LogicWorld logic_world)
        {
            base.Initialize(combat_client, logic_world);
            m_camera_controller = new CameraController(this);
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
            }
            else
            {
                RenderEntity render_entity = m_render_entity_manager.GetObject(binding.EntityID);
                if (render_entity == null)
                    return;
                if (m_combat_client.LocalPlayerPstid == m_logic_world.GetPlayerManager().Objectid2Pstid(render_entity.GetOwnerPlayerID()))
                {
                    m_current_operate_entityi_id = binding.EntityID;
                    Debug.Log("RenderWorld.OnPick(), YOU CHOOSE YOUR ENTITY " + hit.transform.name);
                }
                else
                {
                    m_current_operate_entityi_id = -1;
                    Debug.Log("RenderWorld.OnPick(), " + hit.transform.name + " IS NOT YOUR Entity");
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
    }
}