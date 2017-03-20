using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Combat
{
    public class RenderWorld : IDestruct
    {
        public static Vector3 LogiocPosition2RenderPosition(Vector3I v3i)
        {
            return new Vector3((float)v3i.x / IntMath.METER_MAGNIFICATION, (float)v3i.y / IntMath.METER_MAGNIFICATION, (float)v3i.z / IntMath.METER_MAGNIFICATION);
        }

        public static Vector3I RenderPosition2LogiocPosition(Vector3 v3)
        {
            return new Vector3I(v3.x * IntMath.METER_MAGNIFICATION, v3.y * IntMath.METER_MAGNIFICATION, v3.z * IntMath.METER_MAGNIFICATION);
        }

        protected CombatClient m_combat_client;
        protected LogicWorld m_logic_world;
        protected RenderEntityManager m_render_entity_manager;
        protected TaskScheduler<RenderWorld> m_scheduler;
        protected IRenderMessageProcessor m_render_message_processor;

        public RenderWorld(CombatClient combat_client, LogicWorld logic_world)
        {
            m_combat_client = combat_client;
            m_logic_world = logic_world;
            m_render_entity_manager = new RenderEntityManager(logic_world, this);
            m_scheduler = new TaskScheduler<RenderWorld>(this);
            m_render_message_processor = CreateRenderMessageProcessor();
        }

        public virtual void Destruct()
        {
            m_combat_client = null;
            m_logic_world = null;
            m_scheduler.Destruct();
            m_scheduler = null;
            m_render_entity_manager.Destruct();
            m_render_entity_manager = null;
        }

        protected virtual IRenderMessageProcessor CreateRenderMessageProcessor()
        {
            return new DummyRenderMessageProcessor();
        }

        #region GETTER
        public LogicWorld GetLogicWorld()
        {
            return m_logic_world;
        }
        public RenderEntityManager GetRenderEntityManager()
        {
            return m_render_entity_manager;
        }
        #endregion

        #region RESOURCE
        public virtual void LoadScene(string scene_name)
        {
            Scene scene = SceneManager.GetSceneByPath(scene_name);
            if (scene == null)
            {
                LogWrapper.LogError("MyRenderWorld LoadScene(), ", scene_name);
                return;
            }
            GameGlobal.Instance.m_loadscene_callback += SceneLoadedCallback;
            SceneManager.LoadScene(scene_name);
        }

        void SceneLoadedCallback(int scene_build_index)
        {
            //假设回调时，就是我需要的场景刚加载完，Unity没有提供方法来查询尚未加载的场景的name和index间的关系
            GameGlobal.Instance.m_loadscene_callback -= SceneLoadedCallback;
            OnSceneWasLoaded();
            m_combat_client.OnSceneLoaded();
        }

        protected virtual void OnSceneWasLoaded()
        {
        }
        #endregion

        #region UPDATE
        public virtual void OnGameStart()
        {
        }

        public virtual void OnUpdate(int delta_ms, int current_time)
        {
            UpdateMovingEntities();
            ProcessRenderMessages();
            m_scheduler.Update(current_time);
        }

        protected void ProcessRenderMessages()
        {
            List<RenderMessage> msgs = m_logic_world.GetAllRenderMessages();
            int count = msgs.Count;
            if (count == 0)
                return;
            for (int i = 0; i < count; ++i)
                m_render_message_processor.Process(msgs[i]);
            m_logic_world.ClearRenderMessages();
        }
        #endregion
        
        #region 移动物体
        protected List<ModelComponent> m_moving_entities = new List<ModelComponent>();
        public void RegisterMovingEntity(ModelComponent model_component)
        {
            m_moving_entities.Add(model_component);
        }

        public void UnregisterMovingEntity(ModelComponent model_component)
        {
            m_moving_entities.Remove(model_component);
        }

        protected void UpdateMovingEntities()
        {
            for (int i = 0; i < m_moving_entities.Count; ++i)
                m_moving_entities[i].UpdatePosition();
        }
        #endregion
    }
}