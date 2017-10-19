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
        public SceneSpace m_space = null;
        public BirthPositionInfo(FixPoint x, FixPoint y, FixPoint z, FixPoint angle, SceneSpace space = null)
        {
            m_birth_position = new Vector3FP(x, y, z);
            m_birth_angle = angle;
            m_space = space;
        }
        public void CopyFrom(BirthPositionInfo rhs)
        {
            m_birth_position = rhs.m_birth_position;
            m_birth_angle = rhs.m_birth_angle;
            m_space = rhs.m_space;
        }
    }

    public partial class PositionComponent : EntityComponent
    {
        //配置数据
        FixPoint m_radius = FixPoint.Zero;
        FixPoint m_height = FixPoint.One;
        bool m_base_rotatable = true;
        bool m_collision_sender = true;
        bool m_visible = true;

        //运行数据
        Vector3FP m_current_position;
        FixPoint m_base_angle = FixPoint.Zero;  //绕Z轴的转角，注意Unity是左手系
        FixPoint m_head_angle = FixPoint.Zero;  //头部相对于m_current_angle的旋转
        int m_disable_rotate_count = 0;
        SceneSpace m_current_space = null;

        #region GETTER/SETTER
        public SceneSpace GetCurrentSceneSpace()
        {
            return m_current_space;
        }

        public ISpacePartition GetSpacePartition()
        {
            if (m_current_space != null)
                return m_current_space.m_paitition;
            return null;
        }

        public GridGraph GetGridGraph()
        {
            if (m_current_space != null)
                return m_current_space.m_graph;
            return null;
        }

        public Vector3FP CurrentPosition
        {
            get { return m_current_position; }
            set
            {
                if (m_collision_sender && m_current_space != null)
                {
                    SceneSpace space = m_current_space.GetNeighbourSpace(value);
                    if (space != m_current_space)
                    {
                        m_current_space.m_paitition.RemoveEntity(this);
                        m_current_position = value;
                        m_current_space = space;
                        m_current_space.m_paitition.AddEntiy(this);
                    }
                    else
                    {
                        m_current_space.m_paitition.UpdateEntity(this, value);
                        m_current_position = value;
                    }
                }
                else
                {
                    m_current_position = value;
                }
            }
        }

        public FixPoint BaseAngle
        {
            get { return m_base_angle; }
            set
            {
                if (IsRotatingDisabled)
                    return;
                if (m_base_rotatable)
                {
                    m_base_angle = value;
                    SendChangeDirectionRenderMessage();
                }
            }
        }

        public FixPoint HeadAngle
        {
            get { return m_head_angle; }
            set
            {
                if (IsRotatingDisabled)
                    return;
                if (!m_base_rotatable)
                {
                    m_head_angle = value;
                    SendChangeDirectionRenderMessage();
                }
            }
        }

        public FixPoint FacingAngle
        {
            get
            {
                if (m_base_rotatable)
                {
                    return m_base_angle;
                }
                else
                {
                    return m_base_angle + m_head_angle;
                }
            }
            set
            {
                SetFacing(value);
            }
        }

        public Vector2FP Facing2D
        {
            get
            {
                FixPoint radian = FixPoint.Degree2Radian(-FacingAngle);
                return new Vector2FP(FixPoint.Cos(radian), FixPoint.Sin(radian));
            }
        }

        public Vector3FP Facing3D
        {
            get
            {
                FixPoint radian = FixPoint.Degree2Radian(-FacingAngle);
                return new Vector3FP(FixPoint.Cos(radian), FixPoint.Zero, FixPoint.Sin(radian));
            }
        }
        #endregion

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            BirthPositionInfo birth_info = ParentObject.GetCreationContext().m_birth_info;
            if (birth_info != null)
            {
                m_current_position = birth_info.m_birth_position;
                m_base_angle = birth_info.m_birth_angle;
                m_current_space = birth_info.m_space;
            }
            else
            {
                ParentObject.GetCreationContext().m_birth_info = new BirthPositionInfo(m_current_position.x, m_current_position.y, m_current_position.z, m_base_angle);
            }
            if (m_current_space == null)
                m_current_space = GetLogicWorld().GetDefaultSceneSpace();

            ObjectProtoData proto_data = ParentObject.GetCreationContext().m_proto_data;
            if (proto_data != null)
            {
                var dic = proto_data.m_component_variables;
                if (dic != null)
                {
                    string value;
                    if (dic.TryGetValue("radius", out value))
                        m_radius = FixPoint.Parse(value);
                }
            }

            if (m_collision_sender && m_current_space != null)
            {
                m_current_space.m_paitition.AddEntiy(this);
            }
        }

        public override void OnDeletePending()
        {
            if (m_current_space != null)
            {
                m_current_space.OnEntityDestroy(GetOwnerEntity());
                if (m_collision_sender)
                    m_current_space.m_paitition.RemoveEntity(this);
            }
        }

        public void ClearSpace()
        {
            m_current_space = null;
        }

        protected override void OnDestruct()
        {
            m_current_space = null;
        }
        #endregion

        #region SETTER
        public void SetFacing(Vector3FP direction, bool from_command = false)
        {
            if (IsRotatingDisabled)
                return;
            FixPoint angle = FixPoint.XZToUnityRotationDegree(direction.x, direction.z);
            SetFacing(angle, from_command);
        }

        public void SetFacing(FixPoint angle, bool from_command = false)
        {
            if (IsRotatingDisabled)
                return;
            if (m_base_rotatable)
            {
                m_base_angle = angle;
            }
            else
            {
                m_head_angle = angle - m_base_angle;
            }
            if (!from_command || !GetOwnerPlayer().IsLocal)
                SendChangeDirectionRenderMessage();
        }

        void SendChangeDirectionRenderMessage()
        {
#if COMBAT_CLIENT
            ChangeDirectionRenderMessage msg = RenderMessage.Create<ChangeDirectionRenderMessage>();
            msg.Construct(ParentObject.ID, m_base_angle, m_head_angle);
            GetLogicWorld().AddRenderMessage(msg);
#endif
        }

        public void Teleport(SceneSpace space, Vector3FP new_position)
        {
            if (m_collision_sender)
            {
                if (space != m_current_space)
                {
                    if (m_current_space != null)
                        m_current_space.m_paitition.RemoveEntity(this);
                    m_current_position = new_position;
                    m_current_space = space;
                    if (m_current_space != null)
                        m_current_space.m_paitition.AddEntiy(this);
                }
                else
                {
                    if (m_current_space != null)
                        m_current_space.m_paitition.UpdateEntity(this, new_position);
                    m_current_position = new_position;
                }
            }
            else
            {
                m_current_position = new_position;
            }
#if COMBAT_CLIENT
            ChangePositionRenderMessage msg = RenderMessage.Create<ChangePositionRenderMessage>();
            msg.Construct(GetOwnerEntityID(), new_position);
            GetLogicWorld().AddRenderMessage(msg);
#endif
        }

        public void AdjustPosition2Walkable()
        {
            GridGraph graph = null;
            if (m_current_space != null)
                graph = m_current_space.m_graph;
            else
                graph = GetLogicWorld().GetDefaultSceneSpace().m_graph;
            if (graph == null)
                return;
            CurrentPosition = graph.AdjustPosition2Walkable(m_current_position);

#if COMBAT_CLIENT
            ChangePositionRenderMessage msg = RenderMessage.Create<ChangePositionRenderMessage>();
            msg.Construct(GetOwnerEntityID(), m_current_position);
            GetLogicWorld().AddRenderMessage(msg);
#endif
        }

        public Vector3FP AdjustPosition2Walkable(Vector3FP position)
        {
            GridGraph graph = null;
            if (m_current_space != null)
                graph = m_current_space.m_graph;
            else
                graph = GetLogicWorld().GetDefaultSceneSpace().m_graph;
            if (graph == null)
                return position;
            return graph.AdjustPosition2Walkable(position);
        }
        #endregion

        #region 能否旋转控制
        public void EnableRotating()
        {
            --m_disable_rotate_count;
        }

        public void DisableRotating()
        {
            ++m_disable_rotate_count;
        }

        public bool IsRotatingDisabled
        {
            get { return m_disable_rotate_count > 0; }
        }
        #endregion
    }
}