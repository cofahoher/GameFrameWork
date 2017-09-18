using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class ApplyGeneratorEffectComponent : EffectComponent
    {
        //配置数据
        int m_generator_cfgid = 0;

        //运行数据
        EffectGenerator m_generator;

        public override void Apply()
        {
            Entity owner = GetOwnerEntity();
            m_generator = GetLogicWorld().GetEffectManager().CreateGenerator(m_generator_cfgid, owner);
            if (m_generator == null)
                return;
            EffectDefinitionComponent definition_component = ((Effect)ParentObject).GetDefinitionComponent();
            EffectApplicationData app_data = RecyclableObject.Create<EffectApplicationData>();
            app_data.m_original_entity_id = definition_component.OriginalEntityID;
            app_data.m_source_entity_id = owner.ID;
            m_generator.Activate(app_data, owner);
            RecyclableObject.Recycle(app_data);
        }

        public override void Unapply()
        {
            if (m_generator == null)
                return;
            m_generator.Deactivate();
            GetLogicWorld().GetEffectManager().DestroyGenerator(m_generator.ID, GetOwnerEntityID());
        }
    }
}