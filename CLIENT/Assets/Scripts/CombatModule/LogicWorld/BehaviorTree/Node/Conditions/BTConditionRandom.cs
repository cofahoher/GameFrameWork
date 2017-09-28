using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTConditionRandom : BTCondition
    {
        FixPoint m_pass_rate = FixPoint.Zero;

        public BTConditionRandom()
        {
        }

        public BTConditionRandom(BTConditionRandom prototype)
            : base(prototype)
        {
            m_pass_rate = prototype.m_pass_rate;
        }

        protected override void ResetRuntimeData()
        {
        }

        protected override bool IsSatisfy()
        {
            FixPoint result = m_context.GetLogicWorld().GetRandomGeneratorFP().RandBetween(FixPoint.Zero, FixPoint.One);
            if (result < m_pass_rate)
                return true;
            else
                return false;
        }
    }
}