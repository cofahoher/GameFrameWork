using System.Collections;
using System.Collections.Generic;

namespace Combat
{
    public class TargetGatheringType
    {
        //不需要目标，或者不配，默认是0
        public static readonly int NoTarget = (int)CRC.Calculate("NoTarget");
        //技能的默认目标是有其他组件提供的（现在是TargetingComponent），EffectGeneretor的默认目标是Activate时传递的
        public static readonly int Default = (int)CRC.Calculate("Default");
        //查找技能是提供的目标Entitiy（技能释放者，或者EffectGeneretor的拥有者）
        public static readonly int Source = (int)CRC.Calculate("Source");

        //前方矩形范围（param1长，param2宽）
        public static readonly int ForwardRectangle = (int)CRC.Calculate("ForwardRectangle");
        //周围圆环形范围（param1外半径，param2内半径）
        public static readonly int SurroundingRing = (int)CRC.Calculate("SurroundingRing");
        //周围扇形形范围（param1半径，param2角度）
        public static readonly int ForwardSector = (int)CRC.Calculate("ForwardSector");
    }

    public class TargetGatheringParam
    {
        //Example：前方   三米长  两米宽 范围内的  敌方     英雄
        //         type  param1  param2          faction  category
        public int m_type = 0;
        public FixPoint m_param1 = FixPoint.Zero;
        public FixPoint m_param2 = FixPoint.Zero;
        public int m_fation = FactionRelation.Enemy;
        public int m_category = 0;
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

        public void BuildTargetList(Entity source_entity, TargetGatheringParam param, List<Target> targets)
        {
            int gathering_type = param.m_type;
            if (gathering_type == TargetGatheringType.Default)
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
                return;
            }
            else if (gathering_type == TargetGatheringType.Source)
            {
                Target target = RecyclableObject.Create<Target>();
                target.Construct(m_logic_world);
                target.SetEntityTarget(source_entity);
                targets.Add(target);
                return;
            }

            if (m_space_manager == null)
                return;
            PositionComponent position_cmp = source_entity.GetComponent(PositionComponent.ID) as PositionComponent;
            if (position_cmp == null)
                return;
            Player source_player = source_entity.GetOwnerPlayer();

            List<int> ids = null;
            if (gathering_type == TargetGatheringType.ForwardRectangle)
            {
                ids = m_space_manager.CollectEntity_ForwardRectangle(position_cmp.CurrentPosition, position_cmp.Facing2D, param.m_param1, param.m_param2, source_entity.ID);
            }
            else if (gathering_type == TargetGatheringType.SurroundingRing)
            {
                ids = m_space_manager.CollectEntity_SurroundingRing(position_cmp.CurrentPosition, param.m_param1, param.m_param2, source_entity.ID);
            }
            else if (gathering_type == TargetGatheringType.ForwardSector)
            {
                ids = m_space_manager.CollectEntity_ForwardSector(position_cmp.CurrentPosition, position_cmp.Facing2D, param.m_param1, param.m_param2, source_entity.ID);
            }

            if (ids == null)
                return;
            for (int i = 0; i < ids.Count; ++i)
            {
                Entity entity = m_entity_manager.GetObject(ids[i]);
                if (entity == null)
                    continue;
                if (param.m_category != 0)
                {
                    EntityDefinitionComponent definition_component = entity.GetComponent(EntityDefinitionComponent.ID) as EntityDefinitionComponent;
                    if (definition_component == null)
                        continue;
                    if (!definition_component.IsCategory(param.m_category))
                        continue;
                }
                //PositionComponent position_component = entity.GetComponent(PositionComponent.ID) as PositionComponent;
                //if (position_component.Height <= FixPoint.Zero)
                //    continue;
                if (!FactionRelation.IsFactionSatisfied(source_player.GetFaction(entity.GetOwnerPlayerID()), param.m_fation))
                    continue;
                Target target = RecyclableObject.Create<Target>();
                target.Construct(m_logic_world);
                target.SetEntityTarget(entity);
                targets.Add(target);
            }
            //OVER
        }
    }
}
