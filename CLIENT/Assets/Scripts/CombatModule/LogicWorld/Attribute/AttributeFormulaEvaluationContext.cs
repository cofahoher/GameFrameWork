using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class AttributeFormulaEvaluationContext : IRecyclable, IDestruct
    {
        #region Create/Recycle
        public static AttributeFormulaEvaluationContext Create()
        {
            return ResuableObjectPool<IRecyclable>.Instance.Create<AttributeFormulaEvaluationContext>();
        }

        public static void Recycle(AttributeFormulaEvaluationContext instance)
        {
            ResuableObjectPool<IRecyclable>.Instance.Recycle(instance);
        }
        #endregion

        public Object m_object;
        public Attribute m_attribute;

        public void Reset()
        {
            m_object = null;
            m_attribute = null;
        }

        public void Destruct()
        {
            m_object = null;
            m_attribute = null;
        }

        public void Initialize(Object obj, Attribute attribute)
        {
            m_object = obj;
            m_attribute = attribute;
        }
    }
}