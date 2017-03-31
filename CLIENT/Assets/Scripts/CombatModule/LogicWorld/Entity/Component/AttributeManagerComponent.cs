using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class AttributeManagerComponent : EntityComponent
    {
        SortedDictionary<string, FixPoint> m_base_value = new SortedDictionary<string, FixPoint>();
        SortedDictionary<string, Attribute> m_attributes = new SortedDictionary<string,Attribute>();

        #region 初始化
        public void AddAttribute(string name, FixPoint value)
        {
            m_base_value[name] = value;
        }

        public override void InitializeComponent()
        {
            var enumerator = m_base_value.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string name = enumerator.Current.Key;
                FixPoint base_value = enumerator.Current.Value;
                CreateAttribute(name, base_value);
            }
        }

        void CreateAttribute(string name, FixPoint base_value)
        {
            AttributeDefinition definition = AttributeSystem.Instance.GetDefinitionByName(name);
            if (definition == null)
                return;
            List<string> referenced_names = definition.GetReferencedAttributes();
            for (int i = 0; i < referenced_names.Count; ++i)
            {
                Attribute referenced_attribute;
                if (!m_attributes.TryGetValue(referenced_names[i], out referenced_attribute))
                {
                    FixPoint referenced_attribute_base_value;
                    if (!m_base_value.TryGetValue(referenced_names[i], out referenced_attribute_base_value))
                        referenced_attribute_base_value = FixPoint.Zero;
                    CreateAttribute(referenced_names[i], referenced_attribute_base_value);
                }
            }
            Attribute attribute = Attribute.Create();
            attribute.Construct(this, definition, base_value);
            m_attributes[name] = attribute;
        }

        public override void OnDestruct()
        {
            var enumerator = m_attributes.GetEnumerator();
            while (enumerator.MoveNext())
                Attribute.Recycle(enumerator.Current.Value);
            m_attributes.Clear();
        }
        #endregion

        public Attribute GetAttribute(string attribute_name)
        {
            Attribute attribute;
            m_attributes.TryGetValue(attribute_name, out attribute);
            return attribute;
        }

        public FixPoint GetAttributeValue(string attribute_name)
        {
            Attribute attribute;
            if (!m_attributes.TryGetValue(attribute_name, out attribute))
                return FixPoint.Zero;
            return attribute.Value;
        }

        public FixPoint GetAttributeBaseValue(string attribute_name)
        {
            Attribute attribute;
            if (!m_attributes.TryGetValue(attribute_name, out attribute))
                return FixPoint.Zero;
            return attribute.BaseValue;
        }
    }
}