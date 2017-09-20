using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class ManaComponent : EntityComponent
    {
        public const string DEFAULT_MANA_TYPE_NAME = "mana";
        public static readonly int DEFAULT_MANA_TYPE_ID = (int)CRC.Calculate(DEFAULT_MANA_TYPE_NAME);

        //运行数据
        FixPoint m_current_max_mana = FixPoint.Zero;
        FixPoint m_current_mana = FixPoint.MinusOne;

        public override void InitializeComponent()
        {
            if (m_current_mana < 0)
                m_current_mana = m_current_max_mana;
        }

        public FixPoint GetCurrentManaPoint(int mana_type)
        {
            return m_current_mana;
        }

        public bool ChangeMana(int mana_type, FixPoint delta_mana)
        {
            if (delta_mana == FixPoint.Zero)
                return true;
            FixPoint previous_mana = m_current_mana;
            m_current_mana += delta_mana;
            if (m_current_mana < FixPoint.Zero)
                m_current_mana = FixPoint.Zero;
            else if (m_current_mana > m_current_max_mana)
                m_current_mana = m_current_max_mana;
            delta_mana = m_current_mana - previous_mana;
#if COMBAT_CLIENT
            ChangeManaRenderMessage msg = RenderMessage.Create<ChangeManaRenderMessage>();
            msg.Construct(ParentObject.ID, mana_type, delta_mana, m_current_mana);
            GetLogicWorld().AddRenderMessage(msg);
#endif
            return true;
        }
    }
}