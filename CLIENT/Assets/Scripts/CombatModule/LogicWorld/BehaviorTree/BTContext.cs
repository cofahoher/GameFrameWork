using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class BTContext : IRecyclable
    {
        LogicWorld m_logic_world;
        Dictionary<int, FixPoint> m_data = new Dictionary<int, FixPoint>();
        Dictionary<int, object> m_data_ext = new Dictionary<int, object>();
        BTActionBuffer m_action_buffer = new BTActionBuffer();

        public void Reset()
        {
            m_logic_world = null;
            m_data.Clear();
            m_data_ext.Clear();
            m_action_buffer.Clear();
        }

        public void SetLogicWorld(LogicWorld logic_world)
        {
            m_logic_world = logic_world;
        }

        public LogicWorld GetLogicWorld()
        {
            return m_logic_world;
        }

        public BTActionBuffer GetActionBuffer()
        {
            return m_action_buffer;
        }

        #region 数值
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
        #endregion

        #region 对象
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
        #endregion
    }
}