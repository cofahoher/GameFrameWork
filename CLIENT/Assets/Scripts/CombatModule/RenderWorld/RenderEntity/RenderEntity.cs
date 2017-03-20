using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Combat
{
    public class RenderEntity : Object
    {
        RenderWorld m_render_world;
        Entity m_entity;

        public RenderEntity(RenderWorld render_world)
        {
            m_render_world = render_world;
        }

        protected override void OnDestruct()
        {
            m_render_world = null;
            m_entity = null;
        }

        #region ILogicOwnerInfo
        public override Object GetOwnerObject()
        {
            return GetOwnerPlayer();
        }
        public override int GetOwnerPlayerID()
        {
            return m_context.m_owner_id;
        }
        public override Player GetOwnerPlayer()
        {
            return m_entity.GetOwnerPlayer();
        }
        public override int GetOwnerEntityID()
        {
            return m_context.m_object_id;
        }
        public override Entity GetOwnerEntity()
        {
            return m_entity;
        }
        #endregion

        #region 初始化
        protected override void PreInitializeObject(ObjectCreationContext context)
        {
            m_entity = context.m_logic_world.GetEntityManager().GetObject(m_context.m_object_id);
        }

        protected override bool IsSuitableComponent(int component_type_id)
        {
            return ComponentTypeRegistry.IsRenderComponent(component_type_id);
        }
        #endregion

        #region GETTER
        public RenderWorld GetRenderWorld()
        {
            return m_render_world;
        }

        public Entity GetLogicEntity()
        {
            return m_entity;
        }
        #endregion
    }
}