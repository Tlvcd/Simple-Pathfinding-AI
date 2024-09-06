using System;

namespace lucd.BehaviorTree.Nodes
{
    public class AwaitConditionNode : AwaitableNode
    {
        private readonly Func<bool> _condition;
        private readonly bool _state; //je?eli warunek jest równy tej warto?ci
        
        public AwaitConditionNode(Func<bool> condition, bool state = true, float timeOut = 20f) : base(timeOut)
        {
            _condition = condition;
            _state = state;
        }

        public override NodeState Evaluate()
        {
            StartTimer();

            if (_condition.Invoke() == _state)
            {
                StopTimer();
                return NodeState.Success;
            }

            if(IsTimerOver())
            {
                StopTimer();
                return NodeState.Failure;
            }

            return NodeState.Running;
        }
    }
}
