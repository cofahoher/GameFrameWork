using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Combat
{
    public class RenderEffectInfo : IRecyclable
    {
        public int m_render_effect_cfgid = 0;
        public GameObject m_go;
        public RemoveRenderEffectTask m_task;

        public void Reset()
        {
            m_render_effect_cfgid = 0;
            m_go = null;
            m_task = null;
        }
    }

    public class RenderEffectNode
    {
        public Transform m_parent_tr;
        public List<RenderEffectInfo> m_render_effects = new List<RenderEffectInfo>();
    }

    public partial class RenderEffectManagerComponent : RenderEntityComponent
    {
        //配置数据
        Dictionary<string, string> m_binding_name_to_path;
        //运行数据
        Dictionary<string, RenderEffectNode> m_nodes = new Dictionary<string, RenderEffectNode>();

        #region 初始化/销毁
        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            m_binding_name_to_path = variables;
        }

        protected override void PostInitializeComponent()
        {
        }

        protected override void OnDestruct()
        {
            var enumerator = m_nodes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                RenderEffectNode node = enumerator.Current.Value;
                for (int i = 0; i < node.m_render_effects.Count; ++i)
                    RecycleRenderEffectInfo(node.m_render_effects[i], null);
            }
            m_nodes.Clear();
        }
        #endregion

        public void PlayRenderEffect(int render_effect_cfg_id, FixPoint play_time)
        {
            //ZZWTODO 为了精准识别每一个effect，可以考虑在逻辑端生成一个id
            RenderEffectData config = GetRenderWorld().GetRenderEffectData(render_effect_cfg_id);
            if (config == null)
                return;
            RenderEffectNode node;
            if (!m_nodes.TryGetValue(config.m_binding, out node))
            {
                string path;
                if (!m_binding_name_to_path.TryGetValue(config.m_binding, out path))
                    return;
                ModelComponent model_component = ParentObject.GetComponent(ModelComponent.ID) as ModelComponent;
                if (model_component == null)
                    return;
                Transform tr = model_component.GetUnityGameObject().transform.FindChild(path);
                if (tr == null)
                    return;
                node = new RenderEffectNode();
                node.m_parent_tr = tr;
                m_nodes[config.m_binding] = node;
            }
            //RenderEffectInfo info = null;
            //for (int i = 0; i < node.m_render_effects.Count; ++i)
            //{
            //    if (node.m_render_effects[i].m_render_effect_cfgid == render_effect_cfg_id)
            //    {
            //        info = node.m_render_effects[i];
            //        break;
            //    }
            //}
            //if (info != null)
            //{
            //}
            GameObject effect_go = UnityResourceManager.Instance.CreateGameObject(config.m_prefab);
            if (effect_go == null)
                return;
            effect_go.transform.parent = node.m_parent_tr;
            effect_go.transform.localPosition = Vector3.zero;
            effect_go.transform.localScale = Vector3.one;
            effect_go.transform.localEulerAngles = Vector3.zero;
            RenderEffectInfo info = RecyclableObject.Create<RenderEffectInfo>();
            info.m_render_effect_cfgid = render_effect_cfg_id;
            info.m_go = effect_go;
            if (play_time > FixPoint.Zero)
            {
                var task_scheduler = GetRenderWorld().GetTaskScheduler();
                RemoveRenderEffectTask task = RenderTask.Create<RemoveRenderEffectTask>();
                task.Construct(this, render_effect_cfg_id);
                task_scheduler.Schedule(task, GetRenderWorld().CurrentTime, play_time);
                info.m_task = task;
            }
            node.m_render_effects.Add(info);
        }

        public void StopRenderEffect(int render_effect_cfg_id)
        {
            RenderEffectData config = GetRenderWorld().GetRenderEffectData(render_effect_cfg_id);
            if (config == null)
                return;
            RenderEffectNode node;
            if (!m_nodes.TryGetValue(config.m_binding, out node))
                return;
            int i = 0;
            RenderEffectInfo info = null;
            for (i = 0; i < node.m_render_effects.Count; ++i)
            {
                if (node.m_render_effects[i].m_render_effect_cfgid == render_effect_cfg_id)
                {
                    info = node.m_render_effects[i];
                    break;
                }
            }
            if (info == null)
                return;
            RecycleRenderEffectInfo(info, config);
            node.m_render_effects.RemoveAt(i);
        }

        void RecycleRenderEffectInfo(RenderEffectInfo info, RenderEffectData config)
        {
            if (info.m_task != null)
            {
                RenderTask.Recycle(info.m_task);
                info.m_task = null;
            }
            if (info.m_go != null)
            {
                if (config == null)
                    config = GetRenderWorld().GetRenderEffectData(info.m_render_effect_cfgid);
                info.m_go.transform.parent = null;
                UnityResourceManager.Instance.RecycleGameObject(config.m_prefab, info.m_go);
                info.m_go = null;
            }
            RecyclableObject.Recycle(info);
        }

        public void RecycleAllRenderEffects()
        {
            var enumerator = m_nodes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                RenderEffectNode node = enumerator.Current.Value;
                for (int i = 0; i < node.m_render_effects.Count; ++i)
                {
                    RenderEffectInfo info = node.m_render_effects[i];
                    RenderEffectData config = GetRenderWorld().GetRenderEffectData(info.m_render_effect_cfgid);
                    if (info.m_task != null)
                    {
                        RenderTask.Recycle(info.m_task);
                        info.m_task = null;
                    }
                    if (info.m_go != null)
                    {
                        info.m_go.transform.parent = null;
                        UnityResourceManager.Instance.RecycleGameObject(config.m_prefab, info.m_go);
                        info.m_go = null;
                    }
                    node.m_render_effects.RemoveAt(i);
                    RecyclableObject.Recycle(info);
                }
            }
        }
    }

    public class RemoveRenderEffectTask : Task<RenderWorld>
    {
        RenderEffectManagerComponent m_component;
        int m_render_effect_cfgid = 0;

        public void Construct(RenderEffectManagerComponent component, int render_effect_cfgid)
        {
            m_component = component;
            m_render_effect_cfgid = render_effect_cfgid;
        }

        public override void OnReset()
        {
            m_component = null;
            m_render_effect_cfgid = 0;
        }

        public override void Run(RenderWorld context, FixPoint current_time, FixPoint delta_time_fp)
        {
            m_component.StopRenderEffect(m_render_effect_cfgid);
        }
    }
}