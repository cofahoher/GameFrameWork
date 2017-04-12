using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class AttributeManagerComponent : EntityComponent
    {
        SortedDictionary<int, FixPoint> m_base_value = new SortedDictionary<int, FixPoint>();
        SortedDictionary<int, Attribute> m_attributes = new SortedDictionary<int, Attribute>();

        #region 初始化
        public void SetAttributeBaseValue(int id, FixPoint value)
        {
            m_base_value[id] = value;
        }

        public override void InitializeComponent()
        {
            var enumerator = m_base_value.GetEnumerator();
            while (enumerator.MoveNext())
            {
                int id = enumerator.Current.Key;
                FixPoint base_value = enumerator.Current.Value;
                if (!m_attributes.ContainsKey(id))
                    CreateAttribute(id, base_value);
            }
        }

        void CreateAttribute(int id, FixPoint base_value)
        {
            AttributeDefinition definition = AttributeSystem.Instance.GetDefinitionByID(id);
            if (definition == null)
                return;
            List<int> referenced_ids = definition.GetReferencedAttributes();
            for (int i = 0; i < referenced_ids.Count; ++i)
            {
                if (!m_attributes.ContainsKey(referenced_ids[i]))
                {
                    FixPoint referenced_attribute_base_value;
                    if (!m_base_value.TryGetValue(referenced_ids[i], out referenced_attribute_base_value))
                        referenced_attribute_base_value = FixPoint.Zero;
                    CreateAttribute(referenced_ids[i], referenced_attribute_base_value);
                }
            }
            Attribute attribute = RecyclableObject.Create<Attribute>();
            attribute.Construct(this, definition, base_value);
            m_attributes[id] = attribute;
        }

        protected override void OnDestruct()
        {
            var enumerator = m_attributes.GetEnumerator();
            while (enumerator.MoveNext())
                RecyclableObject.Recycle(enumerator.Current.Value);
            m_attributes.Clear();
        }
        #endregion

        public Attribute GetAttributeByID(int attribute_id)
        {
            Attribute attribute;
            m_attributes.TryGetValue(attribute_id, out attribute);
            return attribute;
        }

        public Attribute GetAttributeByName(string attribute_name)
        {
            int attribute_id = AttributeSystem.Instance.AttributeName2ID(attribute_name);
            Attribute attribute;
            m_attributes.TryGetValue(attribute_id, out attribute);
            return attribute;
        }

        public override FixPoint GetVariable(ExpressionVariable variable, int index)
        {
            if (index < variable.MaxIndex)
            {
                int vid = variable[index];
                Attribute attribute = GetAttributeByID(vid);
                if (attribute != null)
                    return attribute.GetVariable(variable, index + 1);
            }
            return base.GetVariable(variable, index);
        }
    }
}