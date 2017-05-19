using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class BTContext : IRecyclable
    {
        LogicWorld m_logic_world;
        Dictionary<int, FixPoint> m_data = new Dictionary<int, FixPoint>();
        Dictionary<int, object> m_data_ext = new Dictionary<int, object>();

        public void Reset()
        {
            m_logic_world = null;
            m_data.Clear();
            m_data_ext.Clear();
        }

        public void SetData(int key, FixPoint value)
        {
            m_data[key] = value;
        }

        public FixPoint GetData(int key)
        {
            FixPoint value;
            if (!m_data.TryGetValue(key, out value))
                return FixPoint.Zero;
            return value;
        }

        public void SetData(string str_key, FixPoint value)
        {
            int key = (int)CRC.Calculate(str_key);//key.GetHashCode();
            m_data[key] = value;
        }

        public FixPoint GetData(string str_key)
        {
            int key = (int)CRC.Calculate(str_key);//key.GetHashCode();
            FixPoint value;
            if (!m_data.TryGetValue(key, out value))
                return FixPoint.Zero;
            return value;
        }

        public void SetData<T>(int key, T value)
        {
            m_data_ext[key] = value;
        }

        public T GetData<T>(int key)
        {
            object value;
            if (!m_data_ext.TryGetValue(key, out value))
                return default(T);
            return (T)value;
        }

        public void SetData<T>(string str_key, T value)
        {
            int key = (int)CRC.Calculate(str_key);//key.GetHashCode();
            m_data_ext[key] = value;
        }

        public T GetData<T>(string str_key)
        {
            int key = (int)CRC.Calculate(str_key);//key.GetHashCode();
            object value;
            if (!m_data_ext.TryGetValue(key, out value))
                return default(T);
            return (T)value;
        }
    }
}