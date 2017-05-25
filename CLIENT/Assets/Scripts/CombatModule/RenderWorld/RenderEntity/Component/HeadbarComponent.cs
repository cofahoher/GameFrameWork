using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public partial class HeadbarComponent : RenderEntityComponent
    {
        //配置数据
        string m_root_path;
        string m_self_asset_path;
        string m_enemy_asset_path;
        string m_team_asset_path;
        string m_actorFoot_path;
        HearBarUI m_headBar;
        Transform m_aimingLine_trans;
        Transform m_bodyCtrl_trans;

        #region 初始化/销毁
        protected override void PostInitializeComponent()
        {
            ModelComponent model_component = ParentObject.GetComponent(ModelComponent.ID) as ModelComponent;
            if (model_component == null)
                return;
            GameObject go = model_component.GetUnityGameObject();
            if (go == null)
                return;
            Transform child = go.transform.FindChild(m_root_path);
            if (child == null)
                return;
            Entity entiy = GetLogicEntity();
            bool isLocal = entiy.GetCreationContext().m_is_local;
            string path = m_enemy_asset_path;
            if (isLocal)
            {
                path = m_self_asset_path;
                LoadFootHalo(child);
                LoadLine(child);
            }
            else if (isFriend(entiy))
            {
                path = m_team_asset_path;
            }
            GameObject headBar = UnityResourceManager.Instance.CreateGameObject(path);
            headBar.transform.parent = child;
            m_headBar = headBar.GetComponent<HearBarUI>();
        }

        bool isFriend(Entity otherEntity) {
            SortedDictionary<int, Entity> list = GetLogicWorld().GetEntityManager().GetAllObjects();
            foreach (KeyValuePair<int, Entity> kv in list)
            {
                Entity entity = kv.Value;
                if (entity.IsLocal)
                {
                    Player source_player = entity.GetOwnerPlayer();
                    if (source_player.IsAlly(otherEntity.GetOwnerPlayerID()))
                    {
                        return true;   
                    }
                }
            }
            return false;
        }

        void LoadFootHalo(Transform parent)
        {
            Transform trans = parent.Find("body/shenti");
            if (trans == null)
                return;
            GameObject footHalo = UnityResourceManager.Instance.CreateGameObject(m_actorFoot_path);
            footHalo.transform.parent = trans;
            footHalo.transform.localPosition = Vector3.zero;
            footHalo.transform.localScale = Vector3.one;
            footHalo.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        void LoadLine(Transform parent)
        {
            Transform trans = parent.Find("body/shenti");
            if (trans == null)
                return;
            m_aimingLine_trans = UnityResourceManager.Instance.CreateGameObject("Objects/UI/FightUI/AimingLine").transform;
            m_aimingLine_trans.parent = trans;
            m_aimingLine_trans.localPosition = new Vector3(0.1f, 0.2f, 0);
            m_aimingLine_trans.localScale = Vector3.one;
            m_aimingLine_trans.localRotation = Quaternion.Euler(0, 0, 0);
            m_aimingLine_trans.gameObject.SetActive(false);
            m_bodyCtrl_trans = parent.parent;
        }

        public void ShowAimingLine() {
            if (m_aimingLine_trans == null)
                return;
            m_aimingLine_trans.gameObject.SetActive(true);
        }

        public void Look(float angle) {
            float rotate_y = m_bodyCtrl_trans.eulerAngles.y;
            m_aimingLine_trans.localRotation = Quaternion.Euler(0, 0, angle - rotate_y);
        }

        public Vector3FP GetAimingLineFP() {
            if (m_aimingLine_trans == null)
                return Vector3FP.Zero;
            float angle = m_aimingLine_trans.eulerAngles.y - 90;
            if (angle < 0)
                angle = 360 + angle;
            FixPoint radian = FixPoint.Degree2Radian(-FixPoint.CreateFromFloat(angle));
            return new Vector3FP(FixPoint.Cos(radian), FixPoint.Zero, FixPoint.Sin(radian));  
        }

        public void HideAimingLine()
        {
            if (m_aimingLine_trans == null)
                return;
            m_aimingLine_trans.localRotation = Quaternion.Euler(0, 0, 0);
            m_aimingLine_trans.gameObject.SetActive(false);
        }

        public void ChangeHealth(FixPoint curHealth,FixPoint maxHealth)
        {
            m_headBar.ChangeHealth(curHealth, maxHealth);
        }

        protected override void OnDestruct()
        {
        }
        #endregion
    }
}
