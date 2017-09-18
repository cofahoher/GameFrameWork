using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTAction_Test : BTAction
    {
        int m_int = 0;
        FixPoint m_fp = FixPoint.Zero;
        string m_string;
        bool m_bool = false;
        int m_crcint = 0;
        Formula m_formula = RecyclableObject.Create<Formula>();

        protected override void ResetRuntimeData()
        {
        }

        protected override void OnActionEnter()
        {
        }

        protected override void OnActionUpdate(FixPoint delta_time)
        {
        }

        protected override void OnActionExit()
        {
        }
    }

    public partial class BTAction_Test2 : BTAction
    {
        protected override void ResetRuntimeData()
        {
        }

        protected override void OnActionEnter()
        {
        }

        protected override void OnActionUpdate(FixPoint delta_time)
        {
        }

        protected override void OnActionExit()
        {
        }
    }
}