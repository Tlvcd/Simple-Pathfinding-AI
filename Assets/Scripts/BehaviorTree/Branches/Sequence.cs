using lucd.BehaviorTree.Nodes;

namespace lucd.BehaviorTree.Branches
{
    public class Sequence : Composite
    {
        public Sequence(params Node[] children) : base(children) { }

        public override NodeState Evaluate()
        {
            for (; CurrentIndex < Children.Length; CurrentIndex++)
            {
                var result = EvaluateChild(CurrentIndex);
                if (result == NodeState.Success) continue;
                return result;
            }

            CurrentIndex = 0;
            return NodeState.Success;
        }
    }
}