using UnityEngine;
// ReSharper disable CheckNamespace
namespace lucd.BehaviorTree.Nodes
{
	public class RandomNode : Node
	{
		private readonly Node _child;
		private readonly float _chance = 0f;

		private bool _isRunning;
		
		public RandomNode(Node child, float percentChance)
		{
			_child = child;
			_chance = percentChance;
		}

		public override NodeState Evaluate()
		{
			if (!_isRunning && Random.Range(0f, 100f) > _chance)
			{
				return NodeState.Failure;
			}
			
			var eval = _child.Evaluate();
			
			_isRunning = eval == NodeState.Running;
			return eval;
		}
	}
}
