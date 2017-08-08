using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class RegionCallbackManager : IDestruct
    {
        LogicWorld m_logic_world;
        IDGenerator m_id_generator = new IDGenerator(IDGenerator.REGION_CALLBACK_FIRST_ID);
        SortedDictionary<int, EntityGatheringRegion> m_regions = new SortedDictionary<int, EntityGatheringRegion>();
        List<int> m_active_regions = new List<int>();
        int m_next_update_index = 0;

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

        public void Activate(EntityGatheringRegion region)
        {
            m_active_regions.Add(region.ID);
        }

        public void Deactivate(EntityGatheringRegion region)
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

        public void OnUpdate()
        {
            if (m_active_regions.Count == 0)
                return;
            int update_cnt = m_active_regions.Count / 30;  //ZZWTODO 假设逻辑是30帧，一秒更新一遍
            if (update_cnt < 1)
                update_cnt = 1;
            EntityGatheringRegion region;
            int cur_count = m_active_regions.Count;
            while (update_cnt > 0)
            {
                --update_cnt;
                m_regions.TryGetValue(m_active_regions[m_next_update_index], out region);
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
}