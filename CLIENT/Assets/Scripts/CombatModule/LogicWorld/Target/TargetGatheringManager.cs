using System.Collections;
using System.Collections.Generic;

namespace Combat
{
    public class TargetGatheringType
    {
        public static readonly int NoTarget = (int)CRC.Calculate("NoTarget");
        public static readonly int DefaultTarget = (int)CRC.Calculate("DefaultTarget");
        public static readonly int Source = (int)CRC.Calculate("Source");
        public static readonly int ForwardAreaNotAlly = (int)CRC.Calculate("ForwardAreaNotAlly");
        public static readonly int ForwardAreaEnemy = (int)CRC.Calculate("ForwardAreaEnemy");
        public static readonly int SurroundingEnemy = (int)CRC.Calculate("SurroundingEnemy");
    }

    public class TargetGatheringManager : IDestruct
    {
        LogicWorld m_logic_world;
        EntityManager m_entity_manager;
        ISpaceManager m_space_manager;

        public TargetGatheringManager(LogicWorld logic_world)
        {
            m_logic_world = logic_world;
            m_entity_manager = m_logic_world.GetEntityManager();
            m_space_manager = m_logic_world.GetSpaceManager();
        }

        public void Destruct()
        {
            m_logic_world = null;
            m_entity_manager = null;
            m_space_manager = null;
        }

        public void BuildTargetList(Entity source_entity, int target_gathering_type, FixPoint target_gathering_param1, FixPoint target_gathering_param2, List<Target> targets)
        {
            if (target_gathering_type == TargetGatheringType.DefaultTarget)
            {
                TargetingComponent targeting_component = source_entity.GetComponent(TargetingComponent.ID) as TargetingComponent;
                if (targeting_component != null)
                {
                    Entity entity = targeting_component.GetCurrentTarget();
                    if (entity != null)
                    {
                        Target target = RecyclableObject.Create<Target>();
                        target.Construct(m_logic_world);
                        target.SetEntityTarget(entity);
                        targets.Add(target);
                    }
                }
            }
            else if (target_gathering_type == TargetGatheringType.Source)
            {
                Target target = RecyclableObject.Create<Target>();
                target.Construct(m_logic_world);
                target.SetEntityTarget(source_entity);
                targets.Add(target);
            }

            if (m_space_manager == null)
                return;
            PositionComponent position_cmp = source_entity.GetComponent(PositionComponent.ID) as PositionComponent;
            if (position_cmp == null)
                return;
            Player source_player = source_entity.GetOwnerPlayer();
            
            if (target_gathering_type == TargetGatheringType.ForwardAreaNotAlly)
            {
                List<int> ids = m_space_manager.CollectEntity_ForwardArea(position_cmp.CurrentPosition, position_cmp.Facing2D, target_gathering_param1, target_gathering_param2);
                for (int i = 0; i < ids.Count; ++i)
                {
                    Entity entity = m_entity_manager.GetObject(ids[i]);
                    if (entity == null)
                        continue;
                    if (source_player.GetFaction(entity.GetOwnerPlayerID()) == FactionRelation.Ally)
                        continue;
                    Target target = RecyclableObject.Create<Target>();
                    target.Construct(m_logic_world);
                    target.SetEntityTarget(entity);
                    targets.Add(target);
                }
            }
            else if (target_gathering_type == TargetGatheringType.ForwardAreaEnemy)
            {
                List<int> ids = m_space_manager.CollectEntity_ForwardArea(position_cmp.CurrentPosition, position_cmp.Facing2D, target_gathering_param1, target_gathering_param2, source_entity.ID);
                for (int i = 0; i < ids.Count; ++i)
                {
                    Entity entity = m_entity_manager.GetObject(ids[i]);
                    if (entity == null)
                        continue;
                    if (source_player.GetFaction(entity.GetOwnerPlayerID()) != FactionRelation.Enemy)
                        continue;
                    Target target = RecyclableObject.Create<Target>();
                    target.Construct(m_logic_world);
                    target.SetEntityTarget(entity);
                    targets.Add(target);
                }
            }
            else if (target_gathering_type == TargetGatheringType.SurroundingEnemy)
            {
                List<int> ids = m_space_manager.CollectEntity_SurroundingArea(position_cmp.CurrentPosition, target_gathering_param1, source_entity.ID);
                for (int i = 0; i < ids.Count; ++i)
                {
                    Entity entity = m_entity_manager.GetObject(ids[i]);
                    if (entity == null)
                        continue;
                    if (source_player.GetFaction(entity.GetOwnerPlayerID()) != FactionRelation.Enemy)
                        continue;
                    Target target = RecyclableObject.Create<Target>();
                    target.Construct(m_logic_world);
                    target.SetEntityTarget(entity);
                    targets.Add(target);
                }
            }
            //OVER
        }
    }
}
