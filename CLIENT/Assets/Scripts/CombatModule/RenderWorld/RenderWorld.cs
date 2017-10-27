using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Combat
{
    public class RenderWorld : GeneralComposableObject<RenderWorld, FixPoint>, IRenderWorld, IDestruct
    {
        public static Vector3 Vector3FP_To_Vector3(Vector3FP v3fp)
        {
            return new Vector3((float)v3fp.x, (float)v3fp.y, (float)v3fp.z);
        }
        public static Vector3FP Vector3_To_Vector3FP(Vector3 v3)
        {
            return new Vector3FP(FixPoint.CreateFromFloat(v3.x), FixPoint.CreateFromFloat(v3.y), FixPoint.CreateFromFloat(v3.z));
        }

        public virtual RenderEffectData GetRenderEffectData(int id)
        {
            return null;
        }

        public virtual SoundData GetSoundData(int id)
        {
            return null;
        }

        protected List<string> m_loading_scenes = new List<string>();
        protected FixPoint m_current_time = FixPoint.Zero;
        FixPoint m_total_update_time = FixPoint.Zero;
        protected CombatClient m_combat_client;
        protected LogicWorld m_logic_world;
        protected RenderEntityManager m_render_entity_manager;
        protected TaskScheduler<RenderWorld> m_scheduler;
        protected IRenderMessageProcessor m_render_message_processor;

        public RenderWorld()
        {
        }

        public virtual void Initialize(CombatClient combat_client, LogicWorld logic_world)
        {
#if ALLOW_UPDATE
            AddComponent<RenderWorldEveryFrameUpdater>(true);
#endif
            m_current_time = FixPoint.Zero;
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
        public FixPoint CurrentTime
        {
            get { return m_current_time; }
        }
        public LogicWorld GetLogicWorld()
        {
            return m_logic_world;
        }
        public RenderEntityManager GetRenderEntityManager()
        {
            return m_render_entity_manager;
        }
        public TaskScheduler<RenderWorld> GetTaskScheduler()
        {
            return m_scheduler;
        }
        #endregion

        #region RESOURCE
        public virtual void BuildRenderWorld(LevelData level_data)
        {
            LoadScene(level_data.m_scene_name);
        }

        public virtual void LoadScene(string scene_name)
        {
            Scene scene = SceneManager.GetSceneByPath(scene_name);
            if (scene == null)
            {
                LogWrapper.LogError("MyRenderWorld LoadScene(), ", scene_name);
                return;
            }
            m_loading_scenes.Add(scene_name);
            if (m_loading_scenes.Count == 1)
            {
                SceneManager.sceneLoaded += SceneLoadedCallback;
            }
            SceneManager.LoadScene(scene_name);
        }

        void SceneLoadedCallback(Scene scene, LoadSceneMode mod)
        {
            bool find = true;
            for (int i = 0; i < m_loading_scenes.Count; ++i)
            {
                //ZZWTODO fix me 只匹配最后几个字符并不正确，必要但不充分
                if (m_loading_scenes[i].EndsWith(scene.name))
                {
                    m_loading_scenes.RemoveAt(i);
                    find = true;
                    break;
                }
            }
            if (!find)
                return;
            if (m_loading_scenes.Count == 0)
            {
                SceneManager.sceneLoaded -= SceneLoadedCallback;
                OnSceneWasLoaded();
                m_combat_client.OnRenderWorldBuilt();
            }
        }

        protected virtual void OnSceneWasLoaded()
        {
        }
        #endregion

        #region UPDATE
        public virtual void OnGameStart()
        {
            m_current_time = FixPoint.Zero;
        }

        public virtual void OnUpdate(int current_time_ms)
        {
            FixPoint current_time = new FixPoint(current_time_ms) / FixPoint.Thousand;
            if (m_logic_world.IsSuspending)
            {
                FixPoint delta_time = current_time - m_total_update_time;
                if (delta_time < FixPoint.Zero)
                    return;
                m_total_update_time = current_time;
                OnUpdateSuspending(delta_time);
            }
            else
            {
                m_total_update_time = current_time;
                current_time -= m_logic_world.SuspendedTime;
                FixPoint delta_time = current_time - m_current_time;
                if (delta_time < FixPoint.Zero)
                    return;
                m_current_time = current_time;
                UpdateMovingEntities();
                ProcessRenderMessages();
                m_scheduler.Update(m_current_time);
                UpdateGeneralComponent(delta_time, current_time);
            }
        }

        protected void ProcessRenderMessages()
        {
            List<RenderMessage> msgs = m_logic_world.GetAllRenderMessages();
            if (msgs == null)
                return;
            int count = msgs.Count;
            if (count == 0)
                return;
            for (int i = 0; i < count; ++i)
                m_render_message_processor.Process(msgs[i]);
            m_logic_world.ClearRenderMessages();
        }
        #endregion

        #region 暂停
        public virtual void OnSuspend()
        {
            OnUpdate(m_logic_world.TotalUpdateTime);
            UnityEngine.Time.timeScale = 0f;
        }

        public virtual void OnResume()
        {
            UnityEngine.Time.timeScale = 1f;
        }

        public virtual void OnUpdateSuspending(FixPoint delta_time)
        {
        }
        #endregion

        #region GeneralComposableObject
        protected override RenderWorld GetSelf()
        {
            return this;
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
            for (int i = 0; i < m_moving_entities.Count; )
            {
                if (m_moving_entities[i].UpdatePosition())
                    ++i;
                else
                    m_moving_entities.RemoveAt(i);
            }
        }
        #endregion

        #region Command
        public void PushLocalCommand(Command cmd)
        {
            PredictCommand(cmd);
            m_combat_client.GetSyncClient().PushLocalCommand(cmd);
        }

        protected virtual void PredictCommand(Command cmd)
        {
            RenderEntity render_entity = m_render_entity_manager.GetObject(cmd.m_entity_id);
            if (render_entity == null)
                return;
            PredictLogicComponent predict_logic_component = render_entity.GetComponent(PredictLogicComponent.ID) as PredictLogicComponent;
            if (predict_logic_component == null)
                return;
            predict_logic_component.PredictCommand(cmd);
        }

        public virtual void OnLogicWorldHandleCommand(Command cmd, bool result)
        {
            RenderEntity render_entity = m_render_entity_manager.GetObject(cmd.m_entity_id);
            if (render_entity == null)
                return;
            PredictLogicComponent predict_logic_component = render_entity.GetComponent(PredictLogicComponent.ID) as PredictLogicComponent;
            if (predict_logic_component == null)
                return;
            predict_logic_component.ConfirmCommand(cmd, result);
        }
        #endregion

        public virtual bool OnEntityOutOfEdge(RenderEntity render_entity)
        {
            return false;
        }
    }

    class RenderTask
    {
        public static TTask Create<TTask>() where TTask : Task<RenderWorld>, new()
        {
            return ResuableObjectPool<IRecyclable>.Instance.Create<TTask>();
        }
        public static void Recycle(Task<RenderWorld> instance)
        {
            ResuableObjectPool<IRecyclable>.Instance.Recycle(instance);
        }
    }
}