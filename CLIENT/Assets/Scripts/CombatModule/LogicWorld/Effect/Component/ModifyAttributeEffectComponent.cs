using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class AttributeModifierConfig : IRecyclable
    {
        public int m_attribute_id = 0;
        public int m_attribute_category = 0;
        public Formula m_value = RecyclableObject.Create<Formula>();
        public void Reset()
        {
            m_attribute_id = 0;
            m_attribute_category = 0;
            m_value.Reset();
        }
    }

    public partial class ModifyAttributeEffectComponent : EffectComponent
    {
        //配置数据
        int m_count = 0;
        AttributeModifierConfig[] m_modefier_configs;
        //运行数据
        int[] m_modifier_ids;

        #region 初始化/销毁
        static List<string> m_attribute_name_key = new List<string>();
        static List<string> m_attribute_category_key = new List<string>();
        static List<string> m_value_key = new List<string>();
        static void Prepare(int new_count)
        {
            int old_count = m_attribute_name_key.Count;
            if (new_count <= old_count)
                return;
            for (int i = old_count; i < new_count; ++i)
            {
                char index = (char)((int)'1' + i);
                m_attribute_name_key.Add("name" + index);
                m_attribute_category_key.Add("category" + index);
                m_value_key.Add("value" + index);
            }
        }

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("count", out value))
            {
                m_count = int.Parse(value);
                m_modefier_configs = new AttributeModifierConfig[m_count];
                Prepare(m_count);
            }
            for (int i = 0; i < m_count; ++i)
            {
                AttributeModifierConfig modifier = RecyclableObject.Create<AttributeModifierConfig>();
                m_modefier_configs[i] = modifier;
                if (variables.TryGetValue(m_attribute_name_key[i], out value))
                    modifier.m_attribute_id = (int)CRC.Calculate(value);
                if (variables.TryGetValue(m_attribute_category_key[i], out value))
                    modifier.m_attribute_category = (int)CRC.Calculate(value);
                if (variables.TryGetValue(m_value_key[i], out value))
                    modifier.m_value.Compile(value);
            }
        }

        protected override void OnDestruct()
        {
            for (int i = 0; i < m_count; ++i)
                RecyclableObject.Recycle(m_modefier_configs[i]);
            m_modefier_configs = null;
        }
        #endregion

        public override void Apply()
        {
        }

        public override void Unapply()
        {
        }
    }
}