using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class AttributeManagerComponent : EntityComponent
    {
        SortedDictionary<string, int> m_base_value = new SortedDictionary<string,int>();
        SortedDictionary<string, Attribute> m_attributes = new SortedDictionary<string,Attribute>();

        public AttributeManagerComponent()
        {
        }

        public override void OnDestruct()
        {
            var enumerator = m_attributes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Attribute attribute = enumerator.Current.Value;
                attribute.Destruct();
            }
            m_attributes.Clear();
        }

        public void AddAttribute(string name, int value)
        {
            m_base_value[name] = value;
        }

        public override void InitializeComponent()
        {
            var enumerator = m_base_value.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string name = enumerator.Current.Key;
                int base_value = enumerator.Current.Value;
                CreateAttribute(name, base_value);
            }
        }

        void CreateAttribute(string name, int base_value)
        {
            AttributeDefinition definition = AttributeSystem.Instance.GetDefinition(name);
            if (definition == null)
                return;
            List<string> static_dependent_attributes = definition.GetStaticDependentAttributes();
            for (int i = 0; i < static_dependent_attributes.Count; ++i)
            {
                Attribute depent_attribute;
                if (!m_attributes.TryGetValue(static_dependent_attributes[i], out depent_attribute))
                {
                    int depent_attribute_base_value;
                    if (!m_base_value.TryGetValue(static_dependent_attributes[i], out depent_attribute_base_value))
                        depent_attribute_base_value = 0;
                    CreateAttribute(static_dependent_attributes[i], depent_attribute_base_value);
                }
            }
            Attribute attribute = new Attribute(this, definition, base_value);
            m_attributes[name] = attribute;
        }

        public Attribute GetAttribute(string attribute_name)
        {
            Attribute attribute;
            m_attributes.TryGetValue(attribute_name, out attribute);
            return attribute;
        }

        public int GetAttributeValue(string attribute_name)
        {
            Attribute attribute = GetAttribute(attribute_name);
            if (attribute != null)
                return attribute.Value;
            else
                return 0;
        }

        public int GetAttributeBaseValue(string attribute_name)
        {
            Attribute attribute = GetAttribute(attribute_name);
            if (attribute != null)
                return attribute.BaseValue;
            else
                return 0;
        }
    }
}