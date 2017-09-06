using System;
using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BehaviorTreeNodeTypeRegistry
    {
        static public void RegisterDefaultNodes()
        {
            if (ms_default_btnodes_registered)
                return;
            ms_default_btnodes_registered = true;

            Register<BTAction_Test>();
            Register<BTAction_Test2>();
        }
    }

    public partial class BTAction_Test
    {
        public const int ID = -582665673;

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("config_int", out value))
                m_int = int.Parse(value);
            if (variables.TryGetValue("config_fp", out value))
                m_fp = FixPoint.Parse(value);
            if (variables.TryGetValue("config_string", out value))
                m_string = value;
            if (variables.TryGetValue("config_bool", out value))
                m_bool = bool.Parse(value);
            if (variables.TryGetValue("config_crc", out value))
                m_crcint = (int)CRC.Calculate(value);
            if (variables.TryGetValue("config_formula", out value))
                m_formula.Compile(value);
        }
    }

    public partial class BTAction_Test2
    {
        public const int ID = -89988531;
    }
}