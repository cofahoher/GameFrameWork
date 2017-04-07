using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ManualConfigProvider : Singleton<ManualConfigProvider>, IConfigProvider
    {
        Dictionary<int, LevelData> m_level_data = new Dictionary<int, LevelData>();
        public Dictionary<int, ObjectTypeData> m_object_type_data = new Dictionary<int, ObjectTypeData>();
        public Dictionary<int, ObjectProtoData> m_object_proto_data = new Dictionary<int, ObjectProtoData>();
        public Dictionary<int, AttributeData> m_attribute_data = new Dictionary<int, AttributeData>();

        private ManualConfigProvider()
        {
            InitLevelData();
            InitObjectTypeData();
            InitObjectProtoData();
            InitAttributeData();
        }

        public override void Destruct()
        {
        }

        #region IConfigProvider
        public LevelData GetLevelData(int id)
        {
            LevelData level_data = null;
            if (!m_level_data.TryGetValue(id, out level_data))
                return null;
            return level_data;
        }

        public ObjectTypeData GetObjectTypeData(int id)
        {
            ObjectTypeData type_data = null;
            if (!m_object_type_data.TryGetValue(id, out type_data))
                return null;
            return type_data;
        }

        public ObjectProtoData GetObjectProtoData(int id)
        {
            ObjectProtoData proto_data = null;
            if (!m_object_proto_data.TryGetValue(id, out proto_data))
                return null;
            return proto_data;
        }

        public AttributeData GetAttributeData(int id)
        {
            AttributeData attribute_data = null;
            if (!m_attribute_data.TryGetValue(id, out attribute_data))
                return null;
            return attribute_data;
        }

        public FixPoint GetLevelBasedNumber(string table_name, int level)
        {
            int table_id = (int)CRC.Calculate(table_name);
            return GetLevelBasedNumber(table_id, level);
        }

        public FixPoint GetLevelBasedNumber(int table_id, int level)
        {
            return FixPoint.Zero;
        }
        #endregion
        
        #region 手工配置
        void InitLevelData()
        {
            LevelData level_data = new LevelData();
            level_data.m_scene_name = "Scenes/zzw_test";
            m_level_data[1] = level_data;
        }

        void InitObjectTypeData()
        {
            //一些Player
            ObjectTypeData type_data = new ObjectTypeData();
            type_data.m_name = "EnvironmentPlayer";
            m_object_type_data[1] = type_data;

            type_data = new ObjectTypeData();
            type_data.m_name = "AIEnemyPlayer";
            m_object_type_data[2] = type_data;

            type_data = new ObjectTypeData();
            type_data.m_name = "LocalPlayer";
            m_object_type_data[3] = type_data;

            //障碍物
            type_data = new ObjectTypeData();
            type_data.m_name = "Obstruct";
            ComponentData cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("PositionComponent");
            cd.m_component_variables["ext_x"] = "0.5";
            cd.m_component_variables["ext_y"] = "0.5";
            cd.m_component_variables["ext_z"] = "0.5";
            cd.m_component_variables["visible"] = "True";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("ModelComponent");
            type_data.m_components_data.Add(cd);

            m_object_type_data[101] = type_data;

            //Legacy英雄
            type_data = new ObjectTypeData();
            type_data.m_name = "LegacyHero";
            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("PositionComponent");
            cd.m_component_variables["ext_x"] = "0.5";
            cd.m_component_variables["ext_y"] = "0.5";
            cd.m_component_variables["ext_z"] = "0.5";
            cd.m_component_variables["visible"] = "True";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("LocomotorComponent");
            cd.m_component_variables["max_speed"] = "5.0";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("SkillManagerComponent");
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("ModelComponent");
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("AnimationComponent");
            cd.m_component_variables["animation_path"] = "bodyctrl";
            type_data.m_components_data.Add(cd);

            m_object_type_data[102] = type_data;

            //Mecanim英雄
            type_data = new ObjectTypeData();
            type_data.m_name = "MecanimHero";
            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("PositionComponent");
            cd.m_component_variables["ext_x"] = "0.5";
            cd.m_component_variables["ext_y"] = "0.5";
            cd.m_component_variables["ext_z"] = "0.5";
            cd.m_component_variables["visible"] = "True";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("LocomotorComponent");
            cd.m_component_variables["max_speed"] = "5.0";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("SkillManagerComponent");
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("ModelComponent");
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("AnimatorComponent");
            cd.m_component_variables["animator_path"] = "bodyctrl";
            type_data.m_components_data.Add(cd);

            m_object_type_data[103] = type_data;
        }

        void InitObjectProtoData()
        {
            ObjectProtoData proto_data = new ObjectProtoData();
            proto_data.m_name = "Cube";
            proto_data.m_component_variables["asset"] = "Objects/3D/zzw_cube";
            m_object_proto_data[101001] = proto_data;

            proto_data = new ObjectProtoData();
            proto_data.m_name = "Sphere";
            proto_data.m_component_variables["asset"] = "Objects/3D/zzw_sphere";
            m_object_proto_data[101002] = proto_data;

            proto_data = new ObjectProtoData();
            proto_data.m_name = "ssx_legacy";
            proto_data.m_component_variables["asset"] = "Objects/3D/zzw_ssx_legacy";
            m_object_proto_data[102001] = proto_data;

            proto_data = new ObjectProtoData();
            proto_data.m_name = "ssx_mecanim";
            proto_data.m_component_variables["asset"] = "Objects/3D/zzw_ssx_mecanim";
            m_object_proto_data[103001] = proto_data;
        }

        void InitAttributeData()
        {
            AttributeData attribute_data = new AttributeData();
            attribute_data.m_attribute_name = "MaxHealth";
            attribute_data.m_attribute_id = (int)CRC.Calculate(attribute_data.m_attribute_name);
            attribute_data.m_formula = "BaseAttributes.MaxHealth";
            attribute_data.m_reflection_property = "CurrentMaxHealth";
            attribute_data.m_clamp_property = "CurrentHealth";
            attribute_data.m_clamp_min_value = FixPoint.Zero;
            AttributeSystem.RegisterAttribute(attribute_data);
            m_attribute_data[attribute_data.m_attribute_id] = attribute_data;

            //attribute_data = new AttributeData();
            //attribute_data.m_attribute_id = 2;
            //attribute_data.m_attribute_name = "";
            //attribute_data.m_formula = "";
            //attribute_data.m_reflection_property = "";
            //attribute_data.m_clamp_property = "";
            //attribute_data.m_clamp_min_value = FixPoint.Zero;
            //AttributeSystem.RegisterAttribute(attribute_data.m_attribute_id);
            //m_attribute_data[attribute_data.m_attribute_id] = attribute_data;
        }
        #endregion
    }
}