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
    }

    public partial class AnimationComponent : RenderEntityComponent
    {
        //配置数据
        string m_animation_path;
        //运行数据
        float m_animation_speed = 1.0f;
        Animation m_unity_animation_cmp;

        public float AniamtionSpeed
        {
            get { return m_animation_speed; }
            set { m_animation_speed = value; }
        }

        #region 初始化
        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            variables.TryGetValue("animation_path", out m_animation_path);
        }

        protected override void PostInitializeComponent()
        {
            ModelComponent model_component = ParentObject.GetComponent<ModelComponent>();
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
        }

        protected override void OnDestruct()
        {
            m_unity_animation_cmp = null;
        }
        #endregion

        public void PlayerAnimation(string animation_name, bool loop = false, float speed = -1.0f, float fade_length = 0.2f)
        {
            AnimationState state = m_unity_animation_cmp[animation_name];
            if (state == null)
                return;
            state.speed = speed > 0 ? speed : m_animation_speed;
            state.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
            m_unity_animation_cmp.CrossFade(animation_name, fade_length);
        }

        public void QueueAnimation(string animation_name, bool loop = false, float speed = -1.0f, float fade_length = 0.2f)
        {
            AnimationState state = m_unity_animation_cmp.CrossFadeQueued(animation_name, fade_length, QueueMode.CompleteOthers);
            if (state == null)
                return;
            state.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
            state.speed = speed > 0 ? speed : m_animation_speed;
        }
    }
}