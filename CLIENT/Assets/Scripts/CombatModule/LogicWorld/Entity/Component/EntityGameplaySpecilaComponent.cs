using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class EntityGameplaySpecilaComponent : EntityComponent
    {
        //总有些逻辑，完全是由Gameplay自定义的
        #region 初始化/销毁
        protected override void PostInitializeComponent()
        {
            System.Object custom_data = ParentObject.GetCreationContext().m_custom_data;
            if (custom_data == null)
                return;
        }
        #endregion
    }
}