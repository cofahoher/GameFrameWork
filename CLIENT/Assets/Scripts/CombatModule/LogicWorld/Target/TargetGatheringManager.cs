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
        //由技能额外数据指定的目标
        public static readonly int SpecifiedTarget = (int)CRC.Calculate("SpecifiedTarget");


        //前方矩形范围（param1长，param2宽）
        public static readonly int ForwardRectangle = (int)CRC.Calculate("ForwardRectangle");
        //周围圆环形范围（param1外半径，param2内半径）
        public static readonly int SurroundingRing = (int)CRC.Calculate("SurroundingRing");
        //周围扇形形范围（param1半径，param2角度）
        public static readonly int ForwardSector = (int)CRC.Calculate("ForwardSector");
        //所有范围
        public static readonly int All = (int)CRC.Calculate("All");
    }

    public class TargetGatheringParam
    {
        //Example：前方   三米长   两米宽  范围内的  敌方      英雄
        //         type   param1  param2           faction   category
        public int m_type = 0;
        public FixPoint m_param1 = FixPoint.Zero;
        public FixPoint m_param2 = FixPoint.Zero;
        public int m_faction = FactionRelation.Enemy;
        public int m_category = 0;
        //ZZWTODO 还需要别的条件：比如距离最近，血量最低；数量；是否包括自己；附近的某个友方和自己（如果有友方）；还有隐身是否盖被选取的问题
        public int m_sorting_method = 0;
        public int m_max_count = -1;

        public void Reset()
        {
            m_type = 0;
            m_param1 = FixPoint.Zero;
            m_param2 = FixPoint.Zero;
            m_faction = FactionRelation.Enemy;
            m_category = 0;
            m_sorting_method = 0;
            m_max_count = -1;
        }

        public void CopyFrom(TargetGatheringParam rhs)
        {
            m_type = rhs.m_type;
            m_param1 = rhs.m_param1;
            m_param2 = rhs.m_param2;
            m_faction = rhs.m_faction;
            m_category = rhs.m_category;
            m_sorting_method = rhs.m_sorting_method;
            m_max_count = rhs.m_max_count;
        }
    }

    public class TargetGatheringManager : IDestruct
    {
        LogicWorld m_logic_world;
        EntityManager m_entity_manager;
        List<int> m_temp_targets = new List<int>();

        public TargetGatheringManager(LogicWorld logic_world)
        {
            m_logic_world = logic_world;
            m_entity_manager = m_logic_world.GetEntityManager();
        }

        public void Destruct()
        {
            m_logic_world = null;
            m_entity_manager = null;
        }

        public void BuildTargetList(Entity source_entity, TargetGatheringParam param, List<Target> targets)
        {
            m_temp_targets.Clear();
            if (GatherEntitySpecial(source_entity, param, m_temp_targets))
            {
            }
            else
            {
                PositionComponent position_cmp = source_entity.GetComponent(PositionComponent.ID) as PositionComponent;
                if (position_cmp == null)
                    return;
                GatherGeneral(position_cmp.GetSpacePartition(), source_entity.GetOwnerPlayer(), position_cmp.CurrentPosition, position_cmp.Facing2D, param, m_temp_targets);
            }
            for (int i = 0; i < m_temp_targets.Count; ++i)
            {
                Target target = RecyclableObject.Create<Target>();
                target.Construct();
                target.SetEntityTarget(m_temp_targets[i]);
                targets.Add(target);
            }
        }

        public void BuildTargetList(Entity source_entity, TargetGatheringParam param, List<int> targets)
        {
            if (GatherEntitySpecial(source_entity, param, targets))
            {
            }
            else
            {
                PositionComponent position_cmp = source_entity.GetComponent(PositionComponent.ID) as PositionComponent;
                if (position_cmp == null)
                    return;
                GatherGeneral(position_cmp.GetSpacePartition(), source_entity.GetOwnerPlayer(), position_cmp.CurrentPosition, position_cmp.Facing2D, param, targets);
            }
        }

        public void BuildTargetList(ISpacePartition partition, Player player, Vector3FP position, Vector2FP facing, TargetGatheringParam param, List<Target> targets)
        {
            m_temp_targets.Clear();
            GatherGeneral(partition, player, position, facing, param, m_temp_targets);
            for (int i = 0; i < m_temp_targets.Count; ++i)
            {
                Target target = RecyclableObject.Create<Target>();
                target.Construct();
                target.SetEntityTarget(m_temp_targets[i]);
                targets.Add(target);
            }
        }

        public void BuildTargetList(ISpacePartition partition, Player player, Vector3FP position, Vector2FP facing, TargetGatheringParam param, List<int> targets)
        {
            GatherGeneral(partition, player, position, facing, param, targets);
        }

        bool GatherEntitySpecial(Entity source_entity, TargetGatheringParam param, List<int> targets)
        {
            int gathering_type = param.m_type;
            if (gathering_type == TargetGatheringType.Default)
            {
                TargetingComponent targeting_component = source_entity.GetComponent(TargetingComponent.ID) as TargetingComponent;
                if (targeting_component != null)
                {
                    Entity entity = targeting_component.GetCurrentTarget();
                    if (entity != null)
                        targets.Add(entity.ID);
                }
                return true;
            }
            else if (gathering_type == TargetGatheringType.Source)
            {
                targets.Add(source_entity.ID);
                return true;
            }
            return false;
        }

        void GatherGeneral(ISpacePartition partition, Player player, Vector3FP position, Vector2FP facing, TargetGatheringParam param, List<int> targets)
        {
            if (partition == null)
                return;

            List<int> ids = null;
            int gathering_type = param.m_type;
            if (gathering_type == TargetGatheringType.ForwardRectangle)
            {
                ids = partition.CollectEntity_ForwardRectangle(position, facing, param.m_param1, param.m_param2);
            }
            else if (gathering_type == TargetGatheringType.SurroundingRing)
            {
                ids = partition.CollectEntity_SurroundingRing(position, param.m_param1, param.m_param2);
            }
            else if (gathering_type == TargetGatheringType.ForwardSector)
            {
                ids = partition.CollectEntity_ForwardSector(position, facing, param.m_param1, param.m_param2);
            }
            else if (gathering_type == TargetGatheringType.All)
            {
                ids = partition.CollectEntity_All();
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
                if (player != null && !FactionRelation.IsFactionSatisfied(player.GetFaction(entity.GetOwnerPlayerID()), param.m_faction))
                    continue;
                targets.Add(ids[i]);
            }
        }

        public PositionComponent GetNearestEnemy(Entity source_entity)
        {
            Player source_player = source_entity.GetOwnerPlayer();
            PositionComponent source_position_cmp = source_entity.GetComponent(PositionComponent.ID) as PositionComponent;
            Vector3FP source_position = source_position_cmp.CurrentPosition;
            List<int> ids = source_position_cmp.GetSpacePartition().CollectEntity_All();

            PositionComponent potential_enemy = null;
            FixPoint min_distance = FixPoint.MaxValue;
            for (int i = 0; i < ids.Count; ++i)
            {
                Entity entity = m_entity_manager.GetObject(ids[i]);
                if (entity == null)
                    continue;
                if (entity.GetComponent(DamagableComponent.ID) == null)
                    continue;
                if (!FactionRelation.IsFactionSatisfied(source_player.GetFaction(entity.GetOwnerPlayerID()), FactionRelation.Enemy))
                    continue;
                PositionComponent target_position_component = entity.GetComponent(PositionComponent.ID) as PositionComponent;
                Vector3FP offset = source_position - target_position_component.CurrentPosition;
                FixPoint distance = FixPoint.FastDistance(offset.x, offset.z);
                if (distance < min_distance)
                {
                    potential_enemy = target_position_component;
                    min_distance = distance;
                }
            }
            return potential_enemy;
        }
    }
}
