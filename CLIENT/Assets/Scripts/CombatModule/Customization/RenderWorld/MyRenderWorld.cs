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
#else
            MobilePick();
#endif
            base.OnUpdate(delta_ms, current_time);
        }

        #region 测试点选Entity
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

        public void OnPick(RaycastHit hit)
        {
            GameObject go = hit.transform.gameObject;
            UnityObjectBinding binding = go.GetComponent<UnityObjectBinding>();
            if (binding == null)
            {
                Debug.Log("RenderWorld.OnPick(), YOU CHOOSE A POINT" + hit.point.ToString());
                if (m_current_operate_entityi_id < 0)
                    return;
                EntityMoveCommand cmd = new EntityMoveCommand();
                cmd.m_entity_id = m_current_operate_entityi_id;
                cmd.m_destination = RenderPosition2LogiocPosition(hit.point);
                m_combat_client.GetSyncClient().PushLocalCommand(cmd);
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
        #endregion
    }
}