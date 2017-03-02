using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class AttributeFormulaVariableInterface : IExpressionEngionVariableInterface, IRecyclable, IDestruct
    {
        #region Create/Recycle
        public static AttributeFormulaVariableInterface Create()
        {
            return ResuableObjectPool<IRecyclable>.Instance.Create<AttributeFormulaVariableInterface>();
        }

        public static void Recycle(AttributeFormulaVariableInterface instance)
        {
            ResuableObjectPool<IRecyclable>.Instance.Recycle(instance);
        }
        #endregion

        AttributeFormulaEvaluationContext m_context;

        public AttributeFormulaVariableInterface()
        {
        }

        public void Reset()
        {
            m_context = null;
        }

        public void Destruct()
        {
            m_context = null;
        }

        public void SetContext(AttributeFormulaEvaluationContext context)
        {
            m_context = context;
        }

        public void LookupValiable(FormulaVariable variable)
        {
        }

        public int GetVariable(FormulaVariable variable)
        {
            return 0;
        }
    }
}