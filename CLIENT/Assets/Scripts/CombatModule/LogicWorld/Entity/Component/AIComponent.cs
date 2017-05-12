using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class AIComponent : EntityComponent
    {
        int m_test = 0;

        protected override void OnEnable()
        {
            ++m_test;
        }

        protected override void OnDisable()
        {
            --m_test;
        }
    }
}