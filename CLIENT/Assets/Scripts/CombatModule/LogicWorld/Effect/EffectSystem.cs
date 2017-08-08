using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class EffectUniquenessType
    {
        public static readonly int None = (int)CRC.Calculate("None");
        public static readonly int DifferentConflictID = (int)CRC.Calculate("DifferentConflictID");
        public static readonly int DifferentSourceObject = (int)CRC.Calculate("DifferentSourceObject");
    }

    public class EffectRejectionType
    {
        public static readonly int Older = (int)CRC.Calculate("Older");
        public static readonly int Newer = (int)CRC.Calculate("Newer");
    }

    public class EffectRejectionAction
    {
        public static readonly int Destruct = (int)CRC.Calculate("Destruct");
    }

    public class EffectSystem : Singleton<EffectSystem>
    {
        Dictionary<int, EffectCategoryData> m_categories = new Dictionary<int, EffectCategoryData>();
        EffectCategoryData m_default_category;

        private EffectSystem()
        {
            m_default_category = new EffectCategoryData();
            m_default_category.m_category = 0;
            m_default_category.m_uniqueness_type = EffectUniquenessType.DifferentConflictID;
            m_default_category.m_rejection_type = EffectRejectionType.Older;
            m_default_category.m_rejection_action = EffectRejectionAction.Destruct;
        }

        public void RegisterEffectCategory(EffectCategoryData data)
        {
            m_categories[data.m_category] = data;
        }

        public override void Destruct()
        {
        }

        public EffectCategoryData GetCategory(int id)
        {
            return m_default_category;
        }
    }
}