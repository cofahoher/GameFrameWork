using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class EntityUtil
    {
        public static void KillEntity(Entity entity)
        {
            DeathComponent death_component = entity.GetComponent(DeathComponent.ID) as DeathComponent;
            if (death_component != null)
            {
                death_component.KillOwner();
            }
            else
            {
                entity.DeletePending = true;
                var schedeler = entity.GetLogicWorld().GetTaskScheduler();
                DeleteEntityTask delete_task = LogicTask.Create<DeleteEntityTask>();
                delete_task.Construct(entity.ID);
                schedeler.Schedule(delete_task, entity.GetCurrentTime(), FixPoint.PrecisionFP);
            }
        }

        public static EffectGeneratorRegistry GetEffectGeneratorRegistry(Entity entity)
        {
            if (entity == null)
                return null;
            EffectManagerComponent cmp = entity.GetComponent(EffectManagerComponent.ID) as EffectManagerComponent;
            if (cmp == null)
                return null;
            return cmp.GetEffectGeneratorRegistry();
        }

        public static EffectRegistry GetEffectRegistry(Entity entity)
        {
            if (entity == null)
                return null;
            EffectManagerComponent cmp = entity.GetComponent(EffectManagerComponent.ID) as EffectManagerComponent;
            if (cmp == null)
                return null;
            return cmp.GetEffectRegistry();
        }
    }
}