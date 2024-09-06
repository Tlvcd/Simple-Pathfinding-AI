using UnityEngine;

namespace lucd.BehaviorTree.Nodes
{
    public abstract class Node
    {
        public abstract NodeState Evaluate();

        public virtual void ResetState(){ }
    }

    public abstract class AwaitableNode : Node
    {
        protected readonly float Delay;
        private float _timer;

        private bool _timerSet;

        protected AwaitableNode(float delay)
        {
            Delay = delay;
        }

        protected void StartTimer()
        {
            if (_timerSet) return;
            _timerSet = true;
            _timer = Time.time + Delay;
        }

        protected bool IsTimerOver()
        {
            return Time.time >= _timer;
        }

        protected void StopTimer()
        {
            _timerSet = false;
        }

        public override void ResetState()
        {
            StopTimer();
            _timer = 0;
        }
    }

    public enum NodeState
    {
        Running,
        Success,
        Failure
    }
}