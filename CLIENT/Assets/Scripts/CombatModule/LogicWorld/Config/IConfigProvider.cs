using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface IConfigProvider
    {
        FixPoint GetLevelBasedNumber(int table_id, int level);
        FixPoint GetLevelBasedNumber(string table_name, int level);
        LevelData GetLevelData(int id);
        ObjectTypeData GetObjectTypeData(int id);
        ObjectProtoData GetObjectProtoData(int id);
        AttributeData GetAttributeData(int id);
    }
}