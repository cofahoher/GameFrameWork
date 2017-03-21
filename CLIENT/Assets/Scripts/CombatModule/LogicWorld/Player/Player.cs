using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class Player : Object
    {
        SortedDictionary<int, Entity> m_entities = new SortedDictionary<int, Entity>();

        public Player()
        {
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
        }
        public void RemoveEntity(Entity entity)
        {
            m_entities.Remove(entity.ID);
        }

        public SortedDictionary<int, Entity> GetAllEntities()
        {
            return m_entities;
        }
        #endregion

        #region Faction
        public FactionRelation GetFaction(int player_id)
        {
            FactionComponent faction_component = GetComponent<FactionComponent>();
            if (faction_component != null)
            {
                int faction_id_1 = faction_component.FactionID;
                Player player = GetLogicWorld().GetPlayerManager().GetObject(player_id);
                if (player != null)
                {
                    faction_component = player.GetComponent<FactionComponent>();
                    if (faction_component != null)
                    {
                        int faction_id_2 = faction_component.FactionID;
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
}