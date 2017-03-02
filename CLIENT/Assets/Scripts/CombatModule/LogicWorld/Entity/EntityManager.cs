using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class EntityManager : ObjectManager<Entity>
    {
        public EntityManager(LogicWorld logic_world)
            : base(logic_world, IDGenerator.ENTITY_FIRST_ID)
        {
        }

        protected override void AfterObjectCreated(Entity entity)
        {
            Player player = entity.GetOwnerPlayer();
            player.AddEntity(entity);
        }

        protected override void PreDestroyObject(Entity entity)
        {
            Player player = entity.GetOwnerPlayer();
            player.RemoveEntity(entity);
        }
    }
}