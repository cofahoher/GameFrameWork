using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class Player : Object
    {
        SortedDictionary<int, Entity> m_entities = new SortedDictionary<int, Entity>();

        public void OnWorldBuilt()
        {
            var enumerator = m_components.GetEnumerator();
            while (enumerator.MoveNext())
            {
                PlayerComponent player_component = enumerator.Current.Value as PlayerComponent;
                if (player_component != null)
                    player_component.OnWorldBuilt();
            }
        }

        protected override void OnDestruct()
        {
            m_entities.Clear();
        }

        #region GETTER
        public int ProxyID
        {
            get { return m_context.m_object_proxy_id; }
        }
        #endregion

        #region ILogicOwnerInfo
        public override int GetOwnerPlayerID()
        {
            return m_context.m_object_id;
        }
        public override Player GetOwnerPlayer()
        {
            return this;
        }
        #endregion

        #region Entities
        public void AddEntity(Entity entity)
        {
            m_entities[entity.ID] = entity;
            //SendSignal(SignalType.AddEntity, entity);
        }

        public void RemoveEntity(Entity entity)
        {
            //SendSignal(SignalType.RemoveEntity, entity);
            m_entities.Remove(entity.ID);
        }

        public SortedDictionary<int, Entity> GetAllEntities()
        {
            return m_entities;
        }

        public void OnKillEntity(Entity the_killer, Entity the_dead)
        {
            KillingInfo info = RecyclableObject.Create<KillingInfo>();
            info.m_the_killer = the_killer;
            info.m_the_dead = the_dead;
            SendSignal(SignalType.KillEntity, info);
            RecyclableObject.Recycle(info);
        }

        public void OnEntityBeKilled(Entity the_killer, Entity the_dead)
        {
            KillingInfo info = RecyclableObject.Create<KillingInfo>();
            info.m_the_killer = the_killer;
            info.m_the_dead = the_dead;
            SendSignal(SignalType.EntityBeKilled, info);
            RecyclableObject.Recycle(info);
        }
        #endregion

        #region Faction
        public int GetFaction(int player_id)
        {
            FactionComponent faction_component = GetComponent(FactionComponent.ID) as FactionComponent;
            if (faction_component != null)
            {
                int faction_id_1 = faction_component.FactionIndex;
                Player player = GetLogicWorld().GetPlayerManager().GetObject(player_id);
                if (player != null)
                {
                    faction_component = player.GetComponent(FactionComponent.ID) as FactionComponent;
                    if (faction_component != null)
                    {
                        int faction_id_2 = faction_component.FactionIndex;
                        return GetLogicWorld().GetFactionManager().GetRelationShip(faction_id_1, faction_id_2);
                    }
                }                
            }
            if (ID == player_id)
                return FactionRelation.Ally;
            else
                return FactionRelation.Enemy;
        }

        public bool IsAlly(int player_id)
        {
            return GetFaction(player_id) == FactionRelation.Ally;
        }

        public bool IsEnemy(int player_id)
        {
            return GetFaction(player_id) == FactionRelation.Enemy;
        }

        public bool IsNeutral(int player_id)
        {
            return GetFaction(player_id) == FactionRelation.Neutral;
        }
        #endregion
    }

    public class KillingInfo : IRecyclable
    {
        public Entity m_the_killer;
        public Entity m_the_dead;
        public void Reset()
        {
            m_the_killer = null;
            m_the_dead = null;
        }
    }
}