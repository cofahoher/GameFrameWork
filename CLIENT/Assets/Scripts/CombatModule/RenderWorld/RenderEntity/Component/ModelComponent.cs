using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Combat
{
    public class ModelComponent : RenderEntityComponent
    {
        //配置数据
        string m_asset_name;
        //运行数据
        GameObject m_unity_go;
        PositionComponent m_position_component;

        public GameObject GetUnityGameObject()
        {
            return m_unity_go;
        }

        #region 初始化
        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            variables.TryGetValue("asset", out m_asset_name);
        }

        public override void InitializeComponent()
        {
            ObjectProtoData proto_data = ParentObject.GetCreationContext().m_proto_data;
            if (proto_data == null)
                return;
            var dic = proto_data.m_component_variables;
            if (dic == null)
                return;
            string asset;
            if (dic.TryGetValue("asset", out asset))
                m_asset_name = asset;
        }

        protected override void PostInitializeComponent()
        {
            m_unity_go = UnityResourceManager.Instance.CreateGameObject(m_asset_name);
            if (m_unity_go == null)
                return;
            Entity logic_entity = GetLogicEntity();
            m_position_component = logic_entity.GetComponent<PositionComponent>();
            if (m_position_component != null)
            {
                m_unity_go.transform.position = RenderWorld.LogiocPosition2RenderPosition(m_position_component.CurrentPosition);
                m_unity_go.transform.localEulerAngles = new Vector3(0, (float)m_position_component.CurrentAngle, 0);
            }
            UnityObjectBinding binding = m_unity_go.GetComponent<UnityObjectBinding>();
            if (binding == null)
                binding = m_unity_go.AddComponent<UnityObjectBinding>();
            binding.EntityID = logic_entity.ID;
        }

        public override void OnDestruct()
        {
            if (m_unity_go != null)
            {
                GameObject.Destroy(m_unity_go);
                m_unity_go = null;
            }
        }
        #endregion

        public void UpdatePosition()
        {
            m_unity_go.transform.position = RenderWorld.LogiocPosition2RenderPosition(m_position_component.CurrentPosition);
            m_unity_go.transform.localEulerAngles = new Vector3(0, (float)m_position_component.CurrentAngle, 0);
        }
    }
}