using System;
using UnityEngine;

namespace lucd.BehaviorTree.Nodes
{
    public class ActionNode : Node
    {
        private Action _condition;

        public ActionNode(Action condition)
        {
            _condition = condition;
        }

        public override NodeState Evaluate()
        {
            _condition?.Invoke();
            return NodeState.Success;
        }
    }
}
