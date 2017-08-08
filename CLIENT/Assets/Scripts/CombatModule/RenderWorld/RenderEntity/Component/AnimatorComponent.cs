using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Combat
{
    public partial class AnimatorComponent : RenderEntityComponent
    {
        //配置数据
        string m_animator_path;
        string m_locomotor_animation_name = AnimationName.RUN;
        //运行数据
        float m_animation_speed = 1.0f;
        string m_current_animation;
        Animator m_unity_animator_cmp;

        public float AniamtionSpeed
        {
            get { return m_animation_speed; }
            set { m_animation_speed = value; }
        }

        public string CurrentAnimation
        {
            get { return m_current_animation; }
        }

        public string LocomotorAnimationName
        {
            get { return m_locomotor_animation_name; }
            set { m_locomotor_animation_name = value; }
        }

        public bool LocomotorAnimationNameChanged
        {
            get { return m_locomotor_animation_name != AnimationName.RUN; }
        }

        public void ResetLocomotorAnimationName()
        {
            m_locomotor_animation_name = AnimationName.RUN;
        }

        #region 初始化/销毁
        protected override void PostInitializeComponent()
        {
            ModelComponent model_component = ParentObject.GetComponent(ModelComponent.ID) as ModelComponent;
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

        protected override void OnDestruct()
        {
            m_unity_animator_cmp = null;
        }
        #endregion

        public void PlayAnimation(string key, float speed = -1.0f)
        {
            m_unity_animator_cmp.speed = speed > 0 ? speed : m_animation_speed;
            m_unity_animator_cmp.Play(key);
            m_current_animation = key;
        }

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