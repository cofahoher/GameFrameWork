using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class AttributeFormulaVariableProvider : IExpressionVariableProvider, IRecyclable, IDestruct
    {
        #region Create/Recycle
        public static AttributeFormulaVariableProvider Create()
        {
            return ResuableObjectPool<IRecyclable>.Instance.Create<AttributeFormulaVariableProvider>();
        }

        public static void Recycle(AttributeFormulaVariableProvider instance)
        {
            ResuableObjectPool<IRecyclable>.Instance.Recycle(instance);
        }
        #endregion

        AttributeFormulaEvaluationContext m_context;

        public AttributeFormulaVariableProvider()
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

        public void LookupValiable(List<string> scopes, ExpressionVariable variable)
        {
        }

        public FixPoint GetVariable(ExpressionVariable variable)
        {
            return FixPoint.Zero;
        }
    }
}