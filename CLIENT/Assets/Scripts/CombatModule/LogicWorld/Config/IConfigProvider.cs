using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface IConfigProvider
    {
        FixPoint GetLevelBasedNumber(int table_id, int level);
        FixPoint GetLevelBasedNumber(string table_name, int level);
        LevelTableData GetLevelTableData(int table_id);
        LevelTableData GetLevelTableData(string table_name);
        LevelData GetLevelData(int id);
        AttributeData GetAttributeData(int id);
        ObjectTypeData GetObjectTypeData(int id);
        ObjectProtoData GetObjectProtoData(int id);
        ObjectTypeData GetSkillData(int id);
        EffectGeneratorData GetEffectGeneratorData(int id);
        ObjectTypeData GetEffectData(int id);
        BehaviorTreeData GetBehaviorTreeData(int id);
    }
}