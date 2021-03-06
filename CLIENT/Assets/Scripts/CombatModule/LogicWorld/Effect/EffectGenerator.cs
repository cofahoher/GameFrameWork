﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class EffectGenerator : IRecyclable
    {
        LogicWorld m_logic_world;
        int m_id = 0;
        EffectGeneratorData m_data;
        List<EffectGeneratorEntry> m_entries = new List<EffectGeneratorEntry>();

        bool m_remove_self_when_idle = false;
        bool m_is_active = false;

        #region GETTER
        public LogicWorld GetLogicWorld()
        {
            return m_logic_world;
        }

        public int ID
        {
            get { return m_id; }
        }

        public bool RemoveBySelf
        {
            get { return m_data.m_remove_by_self; }
        }

        public bool RemoveSelfWhenIdle
        {
            get { return m_remove_self_when_idle; }
            set
            {
                m_remove_self_when_idle = value;
                if (m_remove_self_when_idle)
                    CheckIdle();
            }
        }

        public bool Active
        {
            get { return m_is_active; }
        }
        #endregion

        #region 初始化/销毁
        public void Construct(LogicWorld logic_world, int id, EffectGeneratorData data)
        {
            m_logic_world = logic_world;
            m_id = id;
            m_data = data;
            for (int i = 0; i < m_data.m_entries.Count; ++i)
            {
                EffectGeneratorEntry entry = RecyclableObject.Create<EffectGeneratorEntry>();
                entry.Construct(this, m_data.m_entries[i], i);
                m_entries.Add(entry);
            }
        }

        public void Reset()
        {
            m_logic_world = null;
            m_id = 0;
            m_data = null;
            for (int i = 0; i < m_entries.Count; ++i)
                RecyclableObject.Recycle(m_entries[i]);
            m_entries.Clear();
            m_remove_self_when_idle = false;
            m_is_active = false;
        }
        #endregion

        public EffectGeneratorEntry GetEntry(int index)
        {
            if (index < 0 || index >= m_entries.Count)
                return null;
            return m_entries[index];
        }

        public void Activate(EffectApplicationData app_data, List<Target> targets)
        {
            Deactivate();
            for (int i = 0; i < m_entries.Count; ++i)
                m_entries[i].Activate(app_data, targets);
            m_is_active = true;
        }

        public void Activate(EffectApplicationData app_data, Entity target)
        {
            Deactivate();
            for (int i = 0; i < m_entries.Count; ++i)
                m_entries[i].Activate(app_data, target);
            m_is_active = true;
        }

        public void Deactivate()
        {
            if (!m_is_active)
                return;
            m_is_active = false;
            if (!m_data.m_deactivate_entry_when_deactive)
                return;
            for (int i = 0; i < m_entries.Count; ++i)
                m_entries[i].Deactivate();
            CheckIdle();
        }

        public void DeactivateOnOneTarget(Entity target)
        {
            for (int i = 0; i < m_entries.Count; ++i)
                m_entries[i].DeactivateOnOneTarget(target);
        }

        public void ForceDeactivate()
        {
            m_is_active = false;
            for (int i = 0; i < m_entries.Count; ++i)
                m_entries[i].Deactivate();
        }

        public void CheckIdle()
        {
            if (!m_remove_self_when_idle)
                return;
            for (int i = 0; i < m_entries.Count; ++i)
            {
                if (!m_entries[i].Idle)
                    return;
            }
            m_logic_world.GetEffectManager().DestroyGenerator(m_id, 0);
        }
    }
}