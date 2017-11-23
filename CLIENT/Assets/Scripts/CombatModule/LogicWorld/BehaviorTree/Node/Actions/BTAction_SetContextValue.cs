using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTAction_SetContextValue : BTAction
    {
        //配置数据
        int m_context_key = -1;
        string m_context_value_expression;

        //运行数据
        ExpressionProgram m_program;

        public BTAction_SetContextValue()
        {
        }

        public BTAction_SetContextValue(BTAction_SetContextValue prototype)
            : base(prototype)
        {
            m_context_key = prototype.m_context_key;
            m_context_value_expression = prototype.m_context_value_expression;
        }

        protected override void ResetRuntimeData()
        {
            if (m_program != null)
            {
                RecyclableObject.Recycle(m_program);
                m_program = null;
            }
        }

        public override void ClearRunningTrace()
        {
        }

        protected override void OnActionEnter()
        {
        }

        protected override void OnActionUpdate(FixPoint delta_time)
        {
            if (m_program == null)
            {
                m_program = RecyclableObject.Create<ExpressionProgram>();
                m_program.Compile(m_context_value_expression);
            }
            FixPoint context_value = m_program.Evaluate(this);
            m_context.SetData(m_context_key, context_value);
        }

        protected override void OnActionExit()
        {
        }
    }
}