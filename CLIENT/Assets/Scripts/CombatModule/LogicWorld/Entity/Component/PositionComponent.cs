using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class BirthPositionInfo
    {
        /*
         * 如果是地球上的3D游戏，需要一个位置和一个绕Y轴的朝向，只是位置的Y不是任意的，根据地形或轨道或重力算得
         * 如果是XZ平面的2D游戏，-Y轴视角，也需要一个位置和一个绕Y轴的朝向，只是Y永远是0
         * 如果是XY平面的2D游戏，Z轴视角，也需要一个位置和一个绕Y轴的朝向，只是Z永远是0，并且绕Y的朝向只能是0或180
         * 如果是宇宙空间的3D游戏，再说。。。。。。
         */
        public Vector3FP m_birth_position;
        public FixPoint m_birth_angle = FixPoint.Zero; //绕Y轴的旋转角度
        public BirthPositionInfo(FixPoint x, FixPoint y, FixPoint z, FixPoint angle)
        {
            m_birth_position = new Vector3FP(x, y, z);
            m_birth_angle = angle;
        }
        public void CopyFrom(BirthPositionInfo rhs)
        {
            m_birth_position = rhs.m_birth_position;
            m_birth_angle = rhs.m_birth_angle;
        }
    }

    public partial class PositionComponent : EntityComponent
    {
        //需要备份的初始数据
        BirthPositionInfo m_birth_info = new BirthPositionInfo(FixPoint.Zero, FixPoint.Zero, FixPoint.Zero, FixPoint.Zero);
        //运行数据
        Vector3FP m_current_position;
        FixPoint m_current_angle;
        Vector3FP m_extents = new Vector3FP();
        bool m_visible = true;

        #region GETTER
        public Vector3FP CurrentPosition
        {
            get { return m_current_position; }
            set { m_current_position = value; }
        }
        public Vector3FP Extents
        {
            get { return m_extents; }
        }
        #endregion

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            BirthPositionInfo birth_info = ParentObject.GetCreationContext().m_birth_info;
            if (birth_info != null)
            {
                m_birth_info.CopyFrom(birth_info);
                m_current_position = m_birth_info.m_birth_position;
                m_current_angle = m_birth_info.m_birth_angle;
            }
            else
            {
                m_birth_info.m_birth_position = m_current_position;
                m_birth_info.m_birth_angle = m_current_angle;
            }
        }
        #endregion

        #region SETTER
        public void SetPosition(Vector3FP new_position)
        {
            m_current_position = new_position;
        }

        public void SetPositionXZ(FixPoint x, FixPoint z)
        {
            m_current_position.x = x;
            m_current_position.z = z;
        }

        public void SetPositionXY(FixPoint x, FixPoint y)
        {
            m_current_position.x = x;
            m_current_position.y = y;
        }

        public void SetAngle(FixPoint new_angle)
        {
            m_current_angle = new_angle;
            ChangeDirectionRenderMessage msg = RenderMessage.Create<ChangeDirectionRenderMessage>();
            msg.Construct(ParentObject.ID, m_current_angle);
            GetLogicWorld().AddRenderMessage(msg);
        }

        public void SetAngle(Vector3FP direction)
        {
            SetAngle(FixPoint.Radian2Degree(FixPoint.Atan2(-direction.z, direction.x)));
        }
        #endregion
    }
}