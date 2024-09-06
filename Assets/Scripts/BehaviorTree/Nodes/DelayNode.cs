
// ReSharper disable once CheckNamespace

using UnityEngine;

namespace lucd.BehaviorTree.Nodes
{
    public class DelayNode : AwaitableNode
    {
        public DelayNode(float delay = 0.1f) : base(delay) { }

        public override NodeState Evaluate()
        {
            StartTimer();

            if (!IsTimerOver()) return NodeState.Running;
            
            StopTimer();
            return NodeState.Success;

        }
    }
}
