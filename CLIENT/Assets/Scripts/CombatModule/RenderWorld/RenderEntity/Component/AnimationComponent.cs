using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Combat
{
    public partial class AnimationName
    {
        public const string IDLE = "idle";
        public const string RUN = "run";
        public const string DIE = "die";
        public const string ATTACK = "attack";
        public const string SKILL = "skill";
    }

    public partial class AnimationComponent : RenderEntityComponent
    {
        //配置数据
        string m_animation_path;
        string m_locomotor_animation_name = AnimationName.RUN;
        //运行数据
        float m_locomotor_animation_speed = 1.0f;
        string m_current_animation;
        Animation m_unity_animation_cmp;

        public string CurrentAnimation
        {
            get { return m_current_animation; }
        }

        public string LocomotorAnimationName
        {
            get { return m_locomotor_animation_name; }
            set { m_locomotor_animation_name = value; }
        }

        public float LocomotorAnimationSpeed
        {
            get { return m_locomotor_animation_speed; }
            set
            {
                m_locomotor_animation_speed = value;
                if (m_current_animation == m_locomotor_animation_name)
                {
                    AnimationState state = m_unity_animation_cmp[m_current_animation];
                    if (state == null)
                        return;
                    state.speed = m_locomotor_animation_speed;
                }
            }
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
            Transform child = go.transform.FindChild(m_animation_path);
            if (child == null)
                return;
            m_unity_animation_cmp = child.GetComponent<Animation>();
            PlayerAnimation(AnimationName.IDLE, true);

            LocomotorComponent locomotor_componnet = GetLogicEntity().GetComponent(LocomotorComponent.ID) as LocomotorComponent;
            if (locomotor_componnet != null)
                m_locomotor_animation_speed = (float)locomotor_componnet.LocomotorSpeedRate;
        }

        protected override void OnDestruct()
        {
            m_unity_animation_cmp = null;
        }
        #endregion

        public void PlayerAnimation(string animation_name, bool loop = false, float speed = 1.0f, float fade_length = 0.2f)
        {
            AnimationState state = m_unity_animation_cmp[animation_name];
            if (state == null)
                return;
            if (animation_name == m_locomotor_animation_name)
                state.speed = m_locomotor_animation_speed;
            else
                state.speed = speed;
            state.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
            m_unity_animation_cmp.CrossFade(animation_name, fade_length);
            m_current_animation = animation_name;
        }

        public void QueueAnimation(string animation_name, bool loop = false, float speed = 1.0f, float fade_length = 0.2f)
        {
            AnimationState state = m_unity_animation_cmp.CrossFadeQueued(animation_name, fade_length, QueueMode.CompleteOthers);
            if (state == null)
                return;
            state.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
            if (animation_name == m_locomotor_animation_name)
                state.speed = m_locomotor_animation_speed;
            else
                state.speed = speed;
        }
    }
}