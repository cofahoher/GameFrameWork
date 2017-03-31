using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ExpressionVariable
    {
        List<string> m_scope_name;
        string m_variable_name;
        List<int> m_scope_handler;
        int m_variable_handler;
    }

    public interface IExpressionVariableProvider
    {
        void LookupValiable(ExpressionVariable variable);
        FixPoint GetVariable(ExpressionVariable variable);
    }

    //public interface IExpressionEngionScope
    //{
    //    IExpressionEngionScope GetScope();
    //    int GetVariable();
    //}
}