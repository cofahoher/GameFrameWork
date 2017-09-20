using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    class ActiveRegionBundles
    {
        public RegionCallbackManager m_manager = null;
        public int m_update_interval = 1000;
        public List<int> m_active_regions = new List<int>();
        public int m_next_update_index = 0;

        public ActiveRegionBundles(RegionCallbackManager manager, int update_interval)
        {
            m_manager = manager;
            m_update_interval = update_interval;
        }

        public void AddRegion(EntityGatheringRegion region)
        {
            m_active_regions.Add(region.ID);
        }

        public void RemoveRegion(EntityGatheringRegion region)
        {
            int index = m_active_regions.IndexOf(region.ID);
            if (index < 0)
                return;
            m_active_regions.RemoveAt(index);
            if (index > m_next_update_index)
            {
            }
            else if (index < m_next_update_index)
            {
                --m_next_update_index;
            }
            else
            {
                if (m_next_update_index >= m_active_regions.Count)
                    m_next_update_index = 0;
            }
        }
        public void OnUpdate(int delta_ms)
        {
            if (m_active_regions.Count == 0)
                return;
            int update_cnt = m_active_regions.Count * delta_ms / 1000;
            if (update_cnt < 1)
                update_cnt = 1;
            EntityGatheringRegion region;
            int cur_count = m_active_regions.Count;
            while (update_cnt > 0)
            {
                --update_cnt;
                region = m_manager.GetRegion(m_active_regions[m_next_update_index]);
                if (region == null)
                    m_active_regions.RemoveAt(m_next_update_index);
                else
                    region.OnUpdate();
                int new_count = m_active_regions.Count;
                if (new_count < cur_count)
                    cur_count = new_count;
                else
                    ++m_next_update_index;
                if (new_count != 0)
                    m_next_update_index = m_next_update_index % new_count;
                else
                    m_next_update_index = 0;
            }
        }
    }

    public class RegionCallbackManager : IDestruct
    {
        LogicWorld m_logic_world;
        IDGenerator m_id_generator = new IDGenerator(IDGenerator.REGION_CALLBACK_FIRST_ID);
        SortedDictionary<int, EntityGatheringRegion> m_regions = new SortedDictionary<int, EntityGatheringRegion>();
        List<ActiveRegionBundles> m_active_region_bundles = new List<ActiveRegionBundles>();

        public RegionCallbackManager(LogicWorld logic_world)
        {
            m_logic_world = logic_world;
        }

        public LogicWorld GetLogicWorld()
        {
            return m_logic_world;
        }

        public void Destruct()
        {
            m_id_generator.Destruct();
            m_id_generator = null;

            var enumerator = m_regions.GetEnumerator();
            while (enumerator.MoveNext())
            {
                EntityGatheringRegion region = enumerator.Current.Value;
                region.Deactivate();
                RecyclableObject.Recycle(region);
            }
            m_regions.Clear();
            m_active_region_bundles.Clear();

            m_logic_world = null;
        }

        public int GenerateID()
        {
            return m_id_generator.GenID();
        }

        public EntityGatheringRegion CreateRegion()
        {
            EntityGatheringRegion region = RecyclableObject.Create<EntityGatheringRegion>();
            region.PreConstruct(this);
            m_regions[region.ID] = region;
            return region;
        }

        public void DestroyRegion(EntityGatheringRegion region)
        {
            if (m_regions.Remove(region.ID))
            {
                region.Deactivate();
                RecyclableObject.Recycle(region);
            }
        }

        public EntityGatheringRegion GetRegion(int region_id)
        {
            EntityGatheringRegion region;
            if (m_regions.TryGetValue(region_id, out region))
                return region;
            else
                return null;
        }

        public void Activate(EntityGatheringRegion region)
        {
            int update_interval = region.UpdateInterval;
            bool done = false;
            for (int i = 0; i < m_active_region_bundles.Count; ++i)
            {
                if (m_active_region_bundles[i].m_update_interval == update_interval)
                {
                    m_active_region_bundles[i].AddRegion(region);
                    done = true;
                    break;
                }
            }
            if (!done)
            {
                ActiveRegionBundles bundle = new ActiveRegionBundles(this, update_interval);
                m_active_region_bundles.Add(bundle);
                bundle.AddRegion(region);
            }
        }

        public void Deactivate(EntityGatheringRegion region)
        {
            for (int i = 0; i < m_active_region_bundles.Count; ++i)
            {
                if (m_active_region_bundles[i].m_update_interval != region.UpdateInterval)
                {
                    m_active_region_bundles[i].RemoveRegion(region);
                    break;
                }
            }
        }

        public void OnUpdate(int delta_ms)
        {
            for (int i = 0; i < m_active_region_bundles.Count; ++i)
                m_active_region_bundles[i].OnUpdate(delta_ms);
        }
    }
}