using System;
using System.Collections;
using lucd.BehaviorTree.Branches;
using lucd.BehaviorTree.Nodes;
using UnityEngine;

public class NPCBehavior : MonoBehaviour
{
    [SerializeField] private float _updateDelay = 0.25f;

    private PathFindingAgent _agent;
    private NPCStuff _npcTasks;
    private Composite _behaviorTree;

    private void Awake()
    {
        _agent = GetComponent<PathFindingAgent>();
        _npcTasks = GetComponent<NPCStuff>();
        _behaviorTree = CreateBehavior();
    }

    private void OnEnable()
    {
        StartCoroutine(Co_TickBehavior());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator Co_TickBehavior()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(_updateDelay);
            _behaviorTree.Evaluate();
        }
    }

    //tworzy drzewko behawioralne na podstawie prostego systemu który kiedyś stworzyłem.
    private Composite CreateBehavior()
    {
        var randomDestination = new Sequence( //cache do pójścia do losowego punktu
            new ConditionNode(() =>
                _agent.SetDestination(_npcTasks.GetRandomPosition())), //sprawdza czy pozycja nie jest w przeszkodzie.
            new AwaitConditionNode(_agent.IsPathInProgress, true, timeOut: 15f), //czeka, aż znajdzie ścieżkę
            new ActionNode(() => Debug.Log($"{gameObject.name}: Idzie do losowego miejsca...")),
            new AwaitConditionNode(_agent.IsPathInProgress, false) //czeka, aż dojdzie do celu
        );

        return new Selector(
            new RandomNode(
                new Sequence( //teleportacja
                    new ConditionNode(() =>
                    {
                        var pos = _npcTasks.GetRandomPosition();
                        if (!_agent.IsPositionValid(pos)) return false;
                        _npcTasks.Warp(pos);
                        return true;
                    }),
                    new ActionNode(() => Debug.Log($"{gameObject.name}: Teleportuje się w losowe miejsce.")),
                    new DelayNode(5f)
                ), 25f
            ),
            new RandomNode(
                randomDestination, //po prostu idzie do losowego miejsca.
                25f),
            new RandomNode(
                new Sequence(
                    randomDestination,
                    new ActionNode(() => Debug.Log($"{gameObject.name}: Obkręca się...")),
                    new ActionNode(_npcTasks.DoRotationAnimation),
                    new DelayNode(8f)
                ), 25f),
            new RandomNode(
                new Sequence(//idzie na miejsce stworzenia
                    new ConditionNode(() =>
                        _agent.SetDestination(_npcTasks
                            .GetStartingPosition())), //sprawdza czy pozycja nie jest w przeszkodzie.
                    new AwaitConditionNode(_agent.IsPathInProgress, true, timeOut: 15f), //czeka, aż znajdzie ścieżkę
                    new ActionNode(() => Debug.Log($"{gameObject.name}: Idzie do miejsca startowego...")),
                    new AwaitConditionNode(_agent.IsPathInProgress, false) //czeka, aż dojdzie do celu
                ), 25f),
            new RandomNode(
                new Sequence(//zmiana koloru.
                    randomDestination,
                    new ActionNode(() => Debug.Log($"{gameObject.name}: Zmienia kolor...")),
                    new ActionNode(_npcTasks.DoColorAnimation),
                    new DelayNode(8f)
                ), 50f)
        );
    }
}