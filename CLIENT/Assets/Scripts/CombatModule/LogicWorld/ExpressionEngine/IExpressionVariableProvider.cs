using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public delegate FixPoint ExpressionVariableDelegate(string variable_name);

    public class ExpressionVariable : IRecyclable, IDestruct
    {
        #region Create/Recycle
        public static ExpressionVariable Create()
        {
            return ResuableObjectPool<IRecyclable>.Instance.Create<ExpressionVariable>();
        }

        public static void Recycle(ExpressionVariable instance)
        {
            ResuableObjectPool<IRecyclable>.Instance.Recycle(instance);
        }
        #endregion

        public ExpressionVariableDelegate m_delegate;
        public string m_variable_name;

        public void Reset()
        {
        }

        public void Destruct()
        {
        }
    }


    public interface IExpressionVariableProvider
    {
        void LookupValiable(List<string> scopes, ExpressionVariable variable);
        FixPoint GetVariable(ExpressionVariable variable);
    }
}