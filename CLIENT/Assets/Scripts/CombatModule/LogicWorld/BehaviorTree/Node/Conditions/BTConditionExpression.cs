using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTConditionExpression : BTCondition
    {
        //配置数据
        string m_expression;

        //运行数据
        ExpressionProgram m_program;

        public BTConditionExpression()
        {
        }

        public BTConditionExpression(BTConditionExpression prototype)
            : base(prototype)
        {
            m_expression = prototype.m_expression;
        }

        protected override void ResetRuntimeData()
        {
            if (m_program != null)
            {
                RecyclableObject.Recycle(m_program);
                m_program = null;
            }
        }

        protected override bool IsSatisfy()
        {
            if (m_program == null)
            {
                m_program = RecyclableObject.Create<ExpressionProgram>();
                m_program.Compile(m_expression);
            }
            FixPoint result = m_program.Evaluate(this);
            if (result != FixPoint.Zero)
                return true;
            else
                return false;
        }
    }
}