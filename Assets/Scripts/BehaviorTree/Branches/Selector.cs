using lucd.BehaviorTree.Nodes;

namespace lucd.BehaviorTree.Branches
{
    public class Selector : Composite
    {
        public Selector(params Node[] children) : base(children) { }

        public override NodeState Evaluate()
        {
            for (; CurrentIndex < Children.Length; CurrentIndex++)
            {
                var result = EvaluateChild(CurrentIndex);
                if (result == NodeState.Failure) continue;
                return result;
            }

            CurrentIndex = 0;
            return NodeState.Failure;
        }
    }
}