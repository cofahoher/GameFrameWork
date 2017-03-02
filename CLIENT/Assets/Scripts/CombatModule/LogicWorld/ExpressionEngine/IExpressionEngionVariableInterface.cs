using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class FormulaVariable
    {
        List<string> m_scope_name;
        string m_variable_name;
        List<int> m_scope_handler;
        int m_variable_handler;
    }

    public interface IExpressionEngionVariableInterface
    {
        void LookupValiable(FormulaVariable variable);
        int GetVariable(FormulaVariable variable);
    }

    //public interface IExpressionEngionScope
    //{
    //    IExpressionEngionScope GetScope();
    //    int GetVariable();
    //}
}