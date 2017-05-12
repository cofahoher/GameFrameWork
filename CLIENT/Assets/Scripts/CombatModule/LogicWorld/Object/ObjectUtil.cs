using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class ObjectUtil
    {
        public static bool IsDead(Object obj)
        {
            if (obj.DeletePending)
                return true;
            return false;
        }

        public static FixPoint GetVariable(Object obj, int vid)
        {
            int component_type_id = ComponentTypeRegistry.GetVariableOwnerComponentID(vid);
            if (component_type_id == 0)
                return FixPoint.Zero;
            while (obj != null)
            {
                Component component = obj.GetComponent(component_type_id);
                if (component != null)
                {
                    FixPoint value;
                    if (component.GetVariable(vid, out value))
                        return value;
                }
                obj = obj.GetOwnerObject();
            }
            return FixPoint.Zero;
        }

        public static int GetLevel(Object obj)
        {
            while (obj != null)
            {
                LevelComponent level_component = obj.GetComponent(LevelComponent.ID) as LevelComponent;
                if (level_component != null)
                    return level_component.CurrentLevel;
                else
                    obj = obj.GetOwnerObject();
            }
            return 0;
        }
    }
}