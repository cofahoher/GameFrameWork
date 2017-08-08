using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class AttributeModifier : IRecyclable
    {
        int m_id = 0;
        int m_category = 0;
        FixPoint m_value = FixPoint.Zero;

        public void Construct(int id, int category, FixPoint value)
        {
            m_id = id;
            m_category = category;
            m_value = value;
        }

        public void Reset()
        {
            m_id = 0;
            m_category = 0;
            m_value = FixPoint.Zero;
        }

        #region GETTER
        public int ID
        {
            get { return m_id; }
        }
        public int Category
        {
            get { return m_category; }
        }
        public FixPoint Value
        {
            get { return m_value; }
        }
        #endregion
    }
}