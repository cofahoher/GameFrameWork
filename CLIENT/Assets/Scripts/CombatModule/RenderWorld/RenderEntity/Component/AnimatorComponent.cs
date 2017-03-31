using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Combat
{
    public partial class AnimatorParameter
    {
        public const string MOVING = "moving";
    }

    public class AnimatorComponent : RenderEntityComponent
    {
        //配置数据
        string m_animator_path;
        //运行数据
        float m_animation_speed = 1.0f;
        Animator m_unity_animator_cmp;

        public float AniamtionSpeed
        {
            get { return m_animation_speed; }
            set { m_animation_speed = value; }
        }

        #region 初始化
        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            variables.TryGetValue("animator_path", out m_animator_path);
        }

        protected override void PostInitializeComponent()
        {
            ModelComponent model_component = ParentObject.GetComponent<ModelComponent>();
            if (model_component == null)
                return;
            GameObject go = model_component.GetUnityGameObject();
            if (go == null)
                return;
            Transform child = go.transform.FindChild(m_animator_path);
            if (child == null)
                return;
            m_unity_animator_cmp = child.GetComponent<Animator>();
        }

        public override void OnDestruct()
        {
            m_unity_animator_cmp = null;
        }
        #endregion

        public void SetParameter(string key, bool value)
        {
            m_unity_animator_cmp.SetBool(key, value);
        }

        public void SetParameter(string key, int value)
        {
            m_unity_animator_cmp.SetInteger(key, value);
        }

        public void SetParameter(string key, float value)
        {
            m_unity_animator_cmp.SetFloat(key, value);
        }

        public void Trigger(string key)
        {
            m_unity_animator_cmp.SetTrigger(key);
        }
    }
}