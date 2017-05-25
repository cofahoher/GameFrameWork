using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class RenderEntityManager : ObjectManager<RenderEntity>
    {
        RenderWorld m_render_world;

        public RenderEntityManager(LogicWorld logic_world, RenderWorld render_world)
            : base(logic_world, IDGenerator.INVALID_FIRST_ID)
        {
            m_render_world = render_world;
        }

        public override void Destruct()
        {
            base.Destruct();
            m_render_world = null;
        }

        protected override RenderEntity CreateObjectInstance(ObjectCreationContext context)
        {
            return new RenderEntity(m_render_world);
        }

        protected override void AfterObjectCreated(RenderEntity entity)
        {
            ObjectCreationContext context = entity.GetCreationContext();
            if (!context.m_is_local || context.m_is_ai)
                return;
            if (entity.GetLogicEntity().GetComponent(LocomotorComponent.ID) != null)
            {
                Component component = entity.AddComponent(PredictLogicComponent.ID);
                if (component != null)
                {
                    component.InitializeComponent();
                    component.OnObjectCreated();
                }
            }
        }

        protected override void PreDestroyObject(RenderEntity entity)
        {
        }
    }
}