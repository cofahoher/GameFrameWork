using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public partial class AimingComponent : RenderEntityComponent
    {
        string m_root_path;
        string m_aiming_line_parent_path;
        string m_aiming_line_asset;
        string m_rotate_turret_asset;
        Transform m_aiming_line_trans;
        Transform m_bodyCtrl_trans;
        Transform m_rotate_turret_trans;
        bool m_is_aiming_line_state = true;
        float m_rotate_angle;

        protected override void PostInitializeComponent()
        {
            ModelComponent model_component = ParentObject.GetComponent(ModelComponent.ID) as ModelComponent;
            if (model_component == null)
                return;
            GameObject go = model_component.GetUnityGameObject();
            if (go == null)
                return;
            m_bodyCtrl_trans = go.transform.FindChild(m_root_path);
            if (m_rotate_turret_asset != null)
            {
                m_is_aiming_line_state = false;
                m_rotate_turret_trans = go.transform.FindChild(m_rotate_turret_asset);
                Transform aiming_line_parent = go.transform.FindChild(m_aiming_line_parent_path);
                LoadAimingLine(aiming_line_parent);
            }
            else
            {
                bool isLocal = GetLogicEntity().GetCreationContext().m_is_local;
                if (isLocal)
                {
                    Transform aiming_line_parent = go.transform.FindChild(m_aiming_line_parent_path);
                    LoadAimingLine(aiming_line_parent);
                }
                m_is_aiming_line_state = true;
            }
        }

        void LoadAimingLine(Transform parent)
        {
            m_aiming_line_trans = UnityResourceManager.Instance.CreateGameObject(m_aiming_line_asset).transform;
            m_aiming_line_trans.parent = parent;
            m_aiming_line_trans.localPosition = new Vector3(0.1f, 0.2f, 0.01f);
            m_aiming_line_trans.localScale = Vector3.one;
            m_aiming_line_trans.localRotation = Quaternion.Euler(0, 0, 0);
            m_aiming_line_trans.gameObject.SetActive(false);
            m_rotate_angle = 0;
        }

        public void ShowAimingLine()
        {
            if (m_aiming_line_trans == null)
                return;
            m_aiming_line_trans.gameObject.SetActive(true);
        }

        public void Look(float angle)
        {
            if (m_is_aiming_line_state)
            {
                float rotate_y = m_bodyCtrl_trans.eulerAngles.y;
                m_aiming_line_trans.localRotation = Quaternion.Euler(0, 0, angle - rotate_y);
            }
            else 
            {
                m_rotate_angle = angle;
                m_rotate_turret_trans.rotation = Quaternion.Euler(m_rotate_turret_trans.eulerAngles.x, angle, m_rotate_turret_trans.eulerAngles.z);
            }
        }

        public bool IsNeedTimelyRotate()
        {
            return !m_is_aiming_line_state;
        }

        public float GetInitAngle()
        {
            if (m_is_aiming_line_state)
            {
                return m_bodyCtrl_trans.eulerAngles.y;
            }
            else 
            {
                return m_rotate_angle;
            }
        }

        public void HideAimingLine()
        {
            if (m_aiming_line_trans == null)
                return;
            m_aiming_line_trans.gameObject.SetActive(false);
        }

        public Vector3FP GetAimingLineFP()
        {
            float angle = 0;
            if (m_is_aiming_line_state)
            {
                if (m_aiming_line_trans == null)
                    return Vector3FP.Zero;
                 angle = m_aiming_line_trans.eulerAngles.y - 90;
            }
            else 
            {
                if (m_rotate_turret_trans == null)
                    return Vector3FP.Zero;
                angle = m_rotate_angle;
            }
            if (angle < 0)
                angle = 360 + angle;
            FixPoint radian = FixPoint.Degree2Radian(-FixPoint.CreateFromFloat(angle));
            return new Vector3FP(FixPoint.Cos(radian), FixPoint.Zero, FixPoint.Sin(radian));  
        }

        protected override void OnDestruct()
        {
        }
    }
}
