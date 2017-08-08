using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Combat
{
    public class RenderEntity : Object
    {
        RenderWorld m_render_world;
        Entity m_entity;
        int m_hide_reference_count = 0;

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

        protected override bool IsLogicObject()
        {
            return false;
        }

        protected override bool IsSuitableComponent(int component_type_id)
        {
            return ComponentTypeRegistry.IsRenderComponent(component_type_id);
        }

        protected override bool OwnContext()
        {
            return false;
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

        public bool Hide
        {
            get { return m_hide_reference_count > 0; }
            set
            {
                if (value)
                {
                    if (++m_hide_reference_count == 1)
                        Show(false);
                }
                else
                {
                    if (--m_hide_reference_count == 0)
                        Show(true);
                }
            }
        }
        #endregion

        #region Hide/Show
        void Show(bool is_show)
        {
            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                RenderEntityComponent component = enumerator.Current.Value as RenderEntityComponent;
                if (component != null)
                    component.Show(is_show);
            }
        }
        #endregion
    }
}