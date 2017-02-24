using System.Collections;
namespace Combat
{
    public class EntityManager : ObjectManager<Entity>
    {
        public EntityManager(LogicWorld logic_world)
            : base(logic_world, IDGenerator.ENTITY_FIRST_ID)
        {
        }
    }
}