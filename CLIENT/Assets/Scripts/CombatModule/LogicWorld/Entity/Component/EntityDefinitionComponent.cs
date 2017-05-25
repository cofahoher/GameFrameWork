using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class EntityDefinitionComponent : EntityComponent
    {
        #region Crawl m_category_1 分类
        public static readonly int DonotCareCategory = 0;
        public static readonly int HumanCategory = (int)CRC.Calculate("human");
        public static readonly int MonsterCategory = (int)CRC.Calculate("monster");
        public static readonly int SoulCategory = (int)CRC.Calculate("soul");
        #endregion

        //配置数据
        int m_category_1 = 0;
        int m_category_2 = 0;
        int m_category_3 = 0;

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            ObjectProtoData proto_data = ParentObject.GetCreationContext().m_proto_data;
            if (proto_data == null)
                return;
            var dic = proto_data.m_component_variables;
            if (dic == null)
                return;
            string value;
            if (dic.TryGetValue("category1", out value))
                m_category_1 = (int)CRC.Calculate(value);
            if (dic.TryGetValue("category2", out value))
                m_category_2 = (int)CRC.Calculate(value);
            if (dic.TryGetValue("category3", out value))
                m_category_3 = (int)CRC.Calculate(value);
        }
        #endregion
    }
}