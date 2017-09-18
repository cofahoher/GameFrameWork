using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class NSkill : ISkill, IRecyclable
    {
        NSkillData m_data;
        Formula m_mana_cost = RecyclableObject.Create<Formula>();
        Formula m_cooldown_time = RecyclableObject.Create<Formula>();

        Dictionary<int, object> m_interskill_data = new Dictionary<int, object>();

        #region 初始化/销毁
        public void Construct()
        {
        }

        public void Reset()
        {
            m_interskill_data.Clear();
        }
        #endregion

        #region 跨技能数据
        public void SetData<T>(int key, T value)
        {
            m_interskill_data[key] = value;
        }

        public T GetData<T>(int key)
        {
            object value;
            if (!m_interskill_data.TryGetValue(key, out value))
                return default(T);
            return (T)value;
        }

        public void SetData<T>(string str_key, T value)
        {
            int key = (int)CRC.Calculate(str_key);
            m_interskill_data[key] = value;
        }

        public T GetData<T>(string str_key)
        {
            int key = (int)CRC.Calculate(str_key);
            object value;
            if (!m_interskill_data.TryGetValue(key, out value))
                return default(T);
            return (T)value;
        }
        #endregion

        #region ISkill
        #endregion
    }
}