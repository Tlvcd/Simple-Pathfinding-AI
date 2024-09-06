using System;
using UnityEngine;

namespace lucd.BehaviorTree.Nodes
{
    public class ConditionNode : Node
    {
        private readonly Func<bool> _condition;
        private readonly bool _state; //jeżeli warunek jest równy tej wartości

        public ConditionNode(Func<bool> condition, bool state = true)
        {
            _condition = condition;
            _state = state;
        }

        public override NodeState Evaluate()
        {
            if (_condition.Invoke() == _state) return NodeState.Success;
            return NodeState.Failure;
        }
    }
}
