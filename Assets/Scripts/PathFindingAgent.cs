using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFindingAgent : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 2f;
    
    private bool _isPathInProgress = false;
    
    private PathFinder _pathFinder;

    private void Start()
    {
        _pathFinder = FindObjectOfType<PathFinder>();

        if (!_pathFinder)
        {
            Debug.Log("Path finder nie znaleziony.");
        }
    }

    private void OnDisable()
    {
        _isPathInProgress = false;
    }
    
    public bool SetDestination(Vector2 pos)
    {
        if (!_pathFinder || !_pathFinder.IsPositionValid(pos)) return false;
        StopAllCoroutines();
        StartCoroutine(_pathFinder.Co_FindPath(transform.position, pos, PathFound)); //sam startuje coroutine, żeby było łatwiej zarządzać.
        return true;
    }
    
    public bool IsPositionValid(Vector2 pos) => _pathFinder.IsPositionValid(pos);

    public bool IsPathInProgress() => _isPathInProgress;

    private void PathFound(List<Vector2> path)
    {
        if (!path.Any())
        {
            _isPathInProgress = false;
            return;
        }
        
        //tutaj rysowanie ścieżki
        var initialPos = transform.position;
        var previousPos = initialPos;
        foreach (var pos in path)
        {
            var newPos = new Vector3(pos.x, pos.y, initialPos.z);
            Debug.DrawLine(previousPos, newPos, Color.magenta, 10f);
            previousPos = newPos;
        }

        _isPathInProgress = true;
        StartCoroutine(Co_FollowPath(path));
    }

    private IEnumerator Co_FollowPath(List<Vector2> path)
    {
        var initialPos = transform.position; //zczytuje z
        var previousPos = initialPos;

        foreach (var pos in path)
        {
            var targetPos = new Vector3(pos.x, pos.y, initialPos.z);
            var distance = Vector3.Distance(previousPos, targetPos);
            var journeyLength = distance;
            var journeyTime = journeyLength / _movementSpeed;
            var elapsedTime = 0f;

            while (elapsedTime < journeyTime)
            {
                var t = Mathf.Clamp01(elapsedTime / journeyTime);
                transform.position = Vector3.Lerp(previousPos, targetPos, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            transform.position = targetPos; //przyciąga do pozycji 
            previousPos = targetPos;
        }

        _isPathInProgress = false;
    }
    
}
