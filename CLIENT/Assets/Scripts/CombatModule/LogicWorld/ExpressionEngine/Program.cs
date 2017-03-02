using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public abstract class Program
    {
        public Program()
        {
        }

        public bool Compile(string formula_string, IExpressionEngionVariableInterface face)
        {
            return false;
        }

        public bool IsConstant()
        {
            return false;
        }

        public int Evaluate(IExpressionEngionVariableInterface face = null)
        {
            return 0;
        }
    }
}