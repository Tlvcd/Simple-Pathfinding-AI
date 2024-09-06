using lucd.BehaviorTree.Nodes;

namespace lucd.BehaviorTree.Branches
{
    public abstract class Composite : Node
    {
        protected readonly Node[] Children;
    
        protected int CurrentIndex;

        protected Composite(params Node[] children)
        {
            Children = children;
        }

        protected NodeState EvaluateChild(int index)
        {
            return index < Children.Length ? Children[index].Evaluate() : NodeState.Failure;
        }

        public override NodeState Evaluate() //override this
        {
            return NodeState.Failure;
        }

        public override void ResetState()
        {
            CurrentIndex = 0;
            foreach (var node in Children)
            {
                node.ResetState();
            }
        }
    }
}
