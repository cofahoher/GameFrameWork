using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ProjectileParameters : IRecyclable, IDestruct
    {
        int m_source_entity_id;
        int m_target_entity_id;
        FixPoint m_speed;
        Vector3FP m_facing;

        public void Reset()
        {
        }

        public void Destruct()
        {
        }
    }

    public partial class ProjectileComponent : EntityComponent
    {
    }
}