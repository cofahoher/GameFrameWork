using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class ObstacleComponent : EntityComponent
    {
        static readonly FixPoint DEGREE90 = new FixPoint(90);
        static readonly FixPoint DEGREE270 = new FixPoint(270);

        //ZZWTODO 不可入区域的定义，现在先只有一个
        Vector3FP m_extents = new Vector3FP();

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            ObjectProtoData proto_data = ParentObject.GetCreationContext().m_proto_data;
            if (proto_data == null)
                return;
            var dic = proto_data.m_component_variables;
            if (dic == null)
                return;
            string value;
            if (dic.TryGetValue("ext_x", out value))
                m_extents.x = FixPoint.Parse(value);
            if (dic.TryGetValue("ext_y", out value))
                m_extents.y = FixPoint.Parse(value);
            if (dic.TryGetValue("ext_z", out value))
                m_extents.z = FixPoint.Parse(value);
        }

        protected override void PostInitializeComponent()
        {
            CoverArea();
        }

        public override void OnDeletePending()
        {
            UncoverArea();
        }

        public override void OnResurrect()
        {
            CoverArea();
        }
        #endregion

        void CoverArea()
        {
            PositionComponent position_component = ParentObject.GetComponent(PositionComponent.ID) as PositionComponent;
            if (position_component == null)
                return;
            GridGraph grid_graph = position_component.GetGridGraph();
            if (grid_graph == null)
                return;
            Vector3FP extents = m_extents;
            if (position_component.BaseAngle == DEGREE90 || position_component.BaseAngle == DEGREE270)
            {
                FixPoint temp = extents.x;
                extents.x = extents.z;
                extents.z = temp;
            }
            grid_graph.CoverArea(position_component.CurrentPosition, extents);
        }

        void UncoverArea()
        {
            PositionComponent position_component = ParentObject.GetComponent(PositionComponent.ID) as PositionComponent;
            if (position_component == null)
                return;
            GridGraph grid_graph = position_component.GetGridGraph();
            if (grid_graph == null)
                return;
            Vector3FP extents = m_extents;
            if (position_component.BaseAngle == DEGREE90 || position_component.BaseAngle == DEGREE270)
            {
                FixPoint temp = extents.x;
                extents.x = extents.z;
                extents.z = temp;
            }
            grid_graph.UncoverArea(position_component.CurrentPosition, extents);
        }
    }
}