using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class EntityUtil
    {
        static FixPoint ms_death_delay = FixPoint.One / FixPoint.Ten;
        public static void KillEntity(Entity entity, int killer_id)
        {
            DeathComponent death_component = entity.GetComponent(DeathComponent.ID) as DeathComponent;
            if (death_component != null)
            {
                death_component.KillOwner(killer_id);
            }
            else
            {
                //Player player = entity.GetOwnerPlayer();
                //Entity killer = entity.GetLogicWorld().GetEntityManager().GetObject(killer_id);
                //player.OnEntityBeKilled(killer, entity);
                //if (killer != null)
                //{
                //    Player killer_player = killer.GetOwnerPlayer();
                //    killer_player.OnKillEntity(killer, entity);
                //}

                //没有死亡组件的就从简了
                entity.SendSignal(SignalType.Die);
                entity.GetLogicWorld().AddSimpleRenderMessage(RenderMessageType.Die, entity.ID);
                entity.DeletePending = true;
                var schedeler = entity.GetLogicWorld().GetTaskScheduler();
                DeleteEntityTask delete_task = LogicTask.Create<DeleteEntityTask>();
                delete_task.Construct(entity.ID);
                schedeler.Schedule(delete_task, entity.GetCurrentTime(), ms_death_delay);
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

        public static bool IsCategory(Entity entity, int category)
        {
            EntityDefinitionComponent component = entity.GetComponent(EntityDefinitionComponent.ID) as EntityDefinitionComponent;
            if (component == null)
                return false;
            return component.IsCategory(category);
        }

        public static Attribute GetAttribute(Entity entity, string attribute_name)
        {
            int attribute_id = (int)CRC.Calculate(attribute_name);
            return GetAttribute(entity, attribute_id);
        }

        public static Attribute GetAttribute(Entity entity, int attribute_id)
        {
            if (entity == null)
                return null;
            AttributeManagerComponent attribute_manager_component = entity.GetComponent(AttributeManagerComponent.ID) as AttributeManagerComponent;
            if (attribute_manager_component == null)
                return null;
            Attribute attribute = attribute_manager_component.GetAttributeByID(attribute_id);
            return attribute;
        }

        public static FixPoint GetAttributeValue(Entity entity, string attribute_name)
        {
            int attribute_id = (int)CRC.Calculate(attribute_name);
            return GetAttributeValue(entity, attribute_id);
        }

        public static FixPoint GetAttributeValue(Entity entity, int attribute_id)
        {
            if (entity == null)
                return FixPoint.Zero;
            AttributeManagerComponent attribute_manager_component = entity.GetComponent(AttributeManagerComponent.ID) as AttributeManagerComponent;
            if (attribute_manager_component == null)
                return FixPoint.Zero;
            Attribute attribute = attribute_manager_component.GetAttributeByID(attribute_id);
            if (attribute == null)
                return FixPoint.Zero;
            return attribute.Value;
        }
    }
}