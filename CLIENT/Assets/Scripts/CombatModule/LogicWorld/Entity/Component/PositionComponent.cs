using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class BirthPositionInfo
    {
        public Vector3I m_birth_position;
        public int m_birth_angle = 0; //绕Y轴的旋转角度
        public BirthPositionInfo(int x = 0, int y = 0, int z = 0, int angle = 0)
        {
            m_birth_position = new Vector3I(x, y, z);
            m_birth_angle = angle;
        }
        public void CopyFrom(BirthPositionInfo rhs)
        {
            m_birth_position = rhs.m_birth_position;
            m_birth_angle = rhs.m_birth_angle;
        }
    }

    public class PositionComponent : EntityComponent
    {
        //配置数据
        BirthPositionInfo m_birth_info = new BirthPositionInfo();
        Vector3I m_extents = new Vector3I();
        bool m_visible = true;
        //运行数据
        Vector3I m_current_position;
        int m_current_angle;

        #region GETTER
        public Vector3I CurrentPosition
        {
            get { return m_current_position; }
            set { m_current_position = value; }
        }
        public Vector3I Extents
        {
            get { return m_extents; }
        }
        public int CurrentAngle
        {
            get { return m_current_angle; }
            set { m_current_angle = value; }
        }
        public bool Visible
        {
            get { return m_visible; }
        }
        #endregion

        #region 初始化
        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;
            if (variables.TryGetValue("x", out value))
            {
                m_birth_info.m_birth_position.x = int.Parse(value);
                if (variables.TryGetValue("y", out value))
                    m_birth_info.m_birth_position.y = int.Parse(value);
                if (variables.TryGetValue("z", out value))
                    m_birth_info.m_birth_position.z = int.Parse(value);
                if (variables.TryGetValue("angle", out value))
                    m_birth_info.m_birth_angle = int.Parse(value);
            }

            if (variables.TryGetValue("ext_x", out value))
            {
                m_extents.x = int.Parse(value);
                if (variables.TryGetValue("ext_y", out value))
                    m_extents.y = int.Parse(value);
                if (variables.TryGetValue("ext_z", out value))
                    m_extents.z = int.Parse(value);
            }


            if (variables.TryGetValue("visible", out value))
                m_visible = bool.Parse(value);
        }

        public override void InitializeComponent()
        {
            BirthPositionInfo birth_info = ParentObject.GetCreationContext().m_birth_info;
            if (birth_info != null)
                m_birth_info.CopyFrom(birth_info);
            m_current_position = m_birth_info.m_birth_position;
            m_current_angle = m_birth_info.m_birth_angle;
        }
        #endregion

        #region SETTER
        public void SetPosition(Vector3I new_position)
        {
            m_current_position = new_position;
        }

        public void SetPositionXZ(int x, int z)
        {
            m_current_position.x = x;
            m_current_position.z = z;
        }

        public void SetPositionXY(int x, int y)
        {
            m_current_position.x = x;
            m_current_position.y = y;
        }

        public void SetAngle(int new_angle)
        {
            m_current_angle = new_angle;
        }
        #endregion
    }
}