using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class DamagableComponent : EntityComponent
    {
        //配置数据
        int m_current_max_health = 0;
        //运行数据
        int m_current_health = 0;

        #region GETTER
        public int MaxHealth
        {
            get { return m_current_max_health; }
        }
        public int CurrentHealth
        {
            get { return m_current_health; }
        }
        #endregion

        #region 初始化
        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("max_health", out value))
                m_current_max_health = int.Parse(value);
        }

        protected override void PostInitializeComponent()
        {
            m_current_health = m_current_max_health;
        }
        #endregion
    }
}