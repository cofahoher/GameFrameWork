using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class SkillManager : ObjectManager<Skill>
    {
        public SkillManager(LogicWorld logic_world)
            : base(logic_world, IDGenerator.SKILL_FIRST_ID)
        {
        }

        protected override Skill CreateObjectInstance(ObjectCreationContext context)
        {
            return new Skill();
        }
    }
}