using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ManualConfigProvider : Singleton<ManualConfigProvider>, IConfigProvider
    {
        Dictionary<int, LevelTableData> m_leveltable_data = new Dictionary<int, LevelTableData>();
        Dictionary<int, LevelData> m_level_data = new Dictionary<int, LevelData>();
        Dictionary<int, ObjectTypeData> m_object_type_data = new Dictionary<int, ObjectTypeData>();
        Dictionary<int, ObjectProtoData> m_object_proto_data = new Dictionary<int, ObjectProtoData>();
        Dictionary<int, AttributeData> m_attribute_data = new Dictionary<int, AttributeData>();
        Dictionary<int, SkillData> m_skill_data = new Dictionary<int, SkillData>();

        private ManualConfigProvider()
        {
            InitLevelTableData();
            InitLevelData();
            InitObjectTypeData();
            InitObjectProtoData();
            InitAttributeData();
            InitSkillData();
        }

        public override void Destruct()
        {
        }

        #region IConfigProvider
        public FixPoint GetLevelBasedNumber(int table_id, int level)
        {
            LevelTableData level_table_data = null;
            if (!m_leveltable_data.TryGetValue(table_id, out level_table_data))
                return FixPoint.Zero;
            return level_table_data[level];
        }

        public FixPoint GetLevelBasedNumber(string table_name, int level)
        {
            int table_id = (int)CRC.Calculate(table_name);
            return GetLevelBasedNumber(table_id, level);
        }

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

        public SkillData GetSkillData(int id)
        {
            SkillData skill_data = null;
            if (!m_skill_data.TryGetValue(id, out skill_data))
                return null;
            return skill_data;
        }
        #endregion
        
        #region 手工配置
        void InitLevelTableData()
        {
            LevelTableData level_table_data = new LevelTableData();
            int id = (int)CRC.Calculate("name1");
            level_table_data.m_max_level = 10;
            level_table_data.m_table = new FixPoint[level_table_data.m_max_level + 1];
            level_table_data.m_table[0] = FixPoint.Parse("0");
            level_table_data.m_table[1] = FixPoint.Parse("0.5");
            level_table_data.m_table[2] = FixPoint.Parse("1");
            level_table_data.m_table[3] = FixPoint.Parse("1.5");
            level_table_data.m_table[4] = FixPoint.Parse("2");
            level_table_data.m_table[5] = FixPoint.Parse("2.5");
            level_table_data.m_table[6] = FixPoint.Parse("3");
            level_table_data.m_table[7] = FixPoint.Parse("3.5");
            level_table_data.m_table[8] = FixPoint.Parse("4");
            level_table_data.m_table[9] = FixPoint.Parse("4.5");
            level_table_data.m_table[10] = FixPoint.Parse("5");
            m_leveltable_data[id] = level_table_data;

            level_table_data = new LevelTableData();
            id = (int)CRC.Calculate("name2");
            level_table_data.m_max_level = 5;
            level_table_data.m_table = new FixPoint[level_table_data.m_max_level + 1];
            level_table_data.m_table[0] = FixPoint.Parse("0");
            level_table_data.m_table[1] = FixPoint.Parse("2.1");
            level_table_data.m_table[2] = FixPoint.Parse("2.9");
            level_table_data.m_table[3] = FixPoint.Parse("4.6");
            level_table_data.m_table[4] = FixPoint.Parse("6.4");
            level_table_data.m_table[5] = FixPoint.Parse("11.3");
            m_leveltable_data[id] = level_table_data;

            level_table_data = new LevelTableData();
            id = (int)CRC.Calculate("name3");
            level_table_data.m_max_level = 3;
            level_table_data.m_table = new FixPoint[level_table_data.m_max_level + 1];
            level_table_data.m_table[0] = FixPoint.Parse("0");
            level_table_data.m_table[1] = FixPoint.Parse("1");
            level_table_data.m_table[2] = FixPoint.Parse("2");
            level_table_data.m_table[3] = FixPoint.Parse("3");
            m_leveltable_data[id] = level_table_data;
        }

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
            cd.m_component_variables = new Dictionary<string, string>();
            cd.m_component_variables["ext_x"] = "0.5";
            cd.m_component_variables["ext_y"] = "0.5";
            cd.m_component_variables["ext_z"] = "0.5";
            cd.m_component_variables["visible"] = "True";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("DamagableComponent");
            cd.m_component_variables = new Dictionary<string, string>();
            cd.m_component_variables["max_health"] = "1000";
            cd.m_component_variables["current_health"] = "900";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("DeathComponent");
            cd.m_component_variables = new Dictionary<string, string>();
            cd.m_component_variables["hide_delay"] = "1";
            cd.m_component_variables["delete_delay"] = "2";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("ModelComponent");
            cd.m_component_variables = new Dictionary<string, string>();
            cd.m_component_variables["bodyctrl_path"] = "bodyctrl";
            type_data.m_components_data.Add(cd);

            m_object_type_data[101] = type_data;

            //Legacy英雄
            type_data = new ObjectTypeData();
            type_data.m_name = "LegacyHero";

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("AttributeManagerComponent");
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("PositionComponent");
            cd.m_component_variables = new Dictionary<string, string>();
            cd.m_component_variables["ext_x"] = "0.5";
            cd.m_component_variables["ext_y"] = "0.5";
            cd.m_component_variables["ext_z"] = "0.5";
            cd.m_component_variables["visible"] = "True";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("LocomotorComponent");
            cd.m_component_variables = new Dictionary<string, string>();
            cd.m_component_variables["max_speed"] = "5.0";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("DamagableComponent");
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("DeathComponent");
            cd.m_component_variables = new Dictionary<string, string>();
            cd.m_component_variables["hide_delay"] = "1";
            cd.m_component_variables["delete_delay"] = "2";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("SkillManagerComponent");
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("ModelComponent");
            cd.m_component_variables = new Dictionary<string, string>();
            cd.m_component_variables["bodyctrl_path"] = "bodyctrl";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("AnimationComponent");
            cd.m_component_variables = new Dictionary<string, string>();
            cd.m_component_variables["animation_path"] = "bodyctrl/animationctrl";
            type_data.m_components_data.Add(cd);

            m_object_type_data[102] = type_data;

            //Mecanim英雄
            type_data = new ObjectTypeData();
            type_data.m_name = "MecanimHero";

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("LevelComponent");
            cd.m_component_variables = new Dictionary<string, string>();
            cd.m_component_variables["level"] = "1";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("AttributeManagerComponent");
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("PositionComponent");
            cd.m_component_variables = new Dictionary<string, string>();
            cd.m_component_variables["ext_x"] = "0.5";
            cd.m_component_variables["ext_y"] = "0.5";
            cd.m_component_variables["ext_z"] = "0.5";
            cd.m_component_variables["visible"] = "True";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("LocomotorComponent");
            cd.m_component_variables = new Dictionary<string, string>();
            cd.m_component_variables["max_speed"] = "5.0";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("DamagableComponent");
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("DeathComponent");
            cd.m_component_variables = new Dictionary<string, string>();
            cd.m_component_variables["hide_delay"] = "1";
            cd.m_component_variables["delete_delay"] = "2";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("SkillManagerComponent");
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("ModelComponent");
            cd.m_component_variables = new Dictionary<string, string>();
            cd.m_component_variables["bodyctrl_path"] = "bodyctrl";
            type_data.m_components_data.Add(cd);

            cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("AnimatorComponent");
            cd.m_component_variables = new Dictionary<string, string>();
            cd.m_component_variables["animator_path"] = "bodyctrl/animationctrl";
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
            proto_data.m_attributes[(int)CRC.Calculate("TestAttribute1")] = new FixPoint(1);
            proto_data.m_attributes[(int)CRC.Calculate("TestAttribute2")] = new FixPoint(2);
            proto_data.m_attributes[(int)CRC.Calculate("TestAttribute3")] = new FixPoint(3);
            proto_data.m_attributes[(int)CRC.Calculate("TestAttribute4")] = new FixPoint(4);
            proto_data.m_attributes[(int)CRC.Calculate("TestAttribute5")] = new FixPoint(5);
            proto_data.m_attributes[(int)CRC.Calculate("TestAttribute6")] = new FixPoint(6);
            proto_data.m_attributes[(int)CRC.Calculate("MaxHealth")] = new FixPoint(1000);
            m_object_proto_data[103001] = proto_data;
        }

        void InitAttributeData()
        {
            AttributeData attribute_data = new AttributeData();
            attribute_data.m_attribute_name = "MaxHealth";
            attribute_data.m_attribute_id = (int)CRC.Calculate(attribute_data.m_attribute_name);
            attribute_data.m_formula = "BaseValue";
            attribute_data.m_reflection_property = (int)CRC.Calculate("max_health");
            attribute_data.m_clamp_property = (int)CRC.Calculate("current_health");
            attribute_data.m_clamp_min_value = FixPoint.Zero;
            AttributeSystem.RegisterAttribute(attribute_data);
            m_attribute_data[attribute_data.m_attribute_id] = attribute_data;

            attribute_data = new AttributeData();
            attribute_data.m_attribute_name = "TestAttribute1";
            attribute_data.m_attribute_id = (int)CRC.Calculate(attribute_data.m_attribute_name);
            attribute_data.m_formula = "BaseValue";
            AttributeSystem.RegisterAttribute(attribute_data);
            m_attribute_data[attribute_data.m_attribute_id] = attribute_data;

            attribute_data = new AttributeData();
            attribute_data.m_attribute_name = "TestAttribute2";
            attribute_data.m_attribute_id = (int)CRC.Calculate(attribute_data.m_attribute_name);
            attribute_data.m_formula = "BaseValue + LevelTable.name3";
            AttributeSystem.RegisterAttribute(attribute_data);
            m_attribute_data[attribute_data.m_attribute_id] = attribute_data;

            attribute_data = new AttributeData();
            attribute_data.m_attribute_name = "TestAttribute3";
            attribute_data.m_attribute_id = (int)CRC.Calculate(attribute_data.m_attribute_name);
            attribute_data.m_formula = "BaseValue + TestAttribute1.Value";
            AttributeSystem.RegisterAttribute(attribute_data);
            m_attribute_data[attribute_data.m_attribute_id] = attribute_data;

            attribute_data = new AttributeData();
            attribute_data.m_attribute_name = "TestAttribute14";
            attribute_data.m_attribute_id = (int)CRC.Calculate(attribute_data.m_attribute_name);
            attribute_data.m_formula = "(BaseValue + TestAttribute2.Value) * LevelTable.name1";
            AttributeSystem.RegisterAttribute(attribute_data);
            m_attribute_data[attribute_data.m_attribute_id] = attribute_data;

            attribute_data = new AttributeData();
            attribute_data.m_attribute_name = "TestAttribute5";
            attribute_data.m_attribute_id = (int)CRC.Calculate(attribute_data.m_attribute_name);
            attribute_data.m_formula = "BaseValue + Max(TestAttribute3.Value, Entity.Attribute.TestAttribute4.Value)";
            AttributeSystem.RegisterAttribute(attribute_data);
            m_attribute_data[attribute_data.m_attribute_id] = attribute_data;

            attribute_data = new AttributeData();
            attribute_data.m_attribute_name = "TestAttribute6";
            attribute_data.m_attribute_id = (int)CRC.Calculate(attribute_data.m_attribute_name);
            attribute_data.m_formula = "BaseValue * TestAttribute2.Value * TestAttribute5.Value";
            AttributeSystem.RegisterAttribute(attribute_data);
            m_attribute_data[attribute_data.m_attribute_id] = attribute_data;
        }

        void InitSkillData()
        {
            //普攻技能
            SkillData skill_data = new SkillData();
            skill_data.m_name = "近战普通攻击";

            ComponentData cd = new ComponentData();
            cd.m_component_type_id = (int)CRC.Calculate("SkillDefinitionComponent");
            cd.m_component_variables = new Dictionary<string, string>();
            cd.m_component_variables["cooldown_time"] = "1";
            cd.m_component_variables["inflict_time"] = "0.5";
            cd.m_component_variables["target_gathering_type"] = "DefaultTarget";
            cd.m_component_variables["main_animation"] = "attack";
            skill_data.m_components_data.Add(cd);
            //cd = new ComponentData();
            //cd.m_component_type_id = (int)CRC.Calculate("DirectDamageSkillComponent");
            //cd.m_component_variables = new Dictionary<string, string>();
            //skill_data.m_components_data.Add(cd);
            m_skill_data[1001] = skill_data;
        }
        #endregion
    }
}