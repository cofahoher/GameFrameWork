﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class AIComponent : EntityComponent
    {
        public static readonly int BTEntry_AIMain = (int)CRC.Calculate("AIMain");

        //配置数据
        int m_bahavior_tree_id = 0;

        //运行数据
        BehaviorTree m_behavior_tree = null;

        #region 初始化/销毁
        public override void InitializeComponent()
        {
            ObjectProtoData proto_data = ParentObject.GetCreationContext().m_proto_data;
            if (proto_data != null)
            {
                var variables = proto_data.m_component_variables;
                if (variables != null)
                {
                    string value;
                    if (variables.TryGetValue("ai_tree_id", out value))
                        m_bahavior_tree_id = int.Parse(value);
                }
            }

            m_behavior_tree = BehaviorTreeFactory.Instance.CreateBehaviorTree(m_bahavior_tree_id);
            if (m_behavior_tree != null)
            {
                m_behavior_tree.Construct(GetLogicWorld());
                BTContext context = m_behavior_tree.Context;
                context.SetData<IExpressionVariableProvider>(BTContextKey.ExpressionVariableProvider, this);
                context.SetData<Entity>(BTContextKey.OwnerEntity, GetOwnerEntity());
                context.SetData<AIComponent>(BTContextKey.OwnerAIComponent, this);
                m_behavior_tree.Active();
            }
        }

        protected override void OnDestruct()
        {
            if (m_behavior_tree != null)
                BehaviorTreeFactory.Instance.RecycleBehaviorTree(m_behavior_tree);
            m_behavior_tree = null;
        }
        #endregion

        protected override void OnEnable()
        {
            if (m_behavior_tree == null)
                return;
            m_behavior_tree.Run(BTEntry_AIMain);
        }

        protected override void OnDisable()
        {
            if (m_behavior_tree == null)
                return;
            m_behavior_tree.ClearRunningTrace();
        }
    }
}