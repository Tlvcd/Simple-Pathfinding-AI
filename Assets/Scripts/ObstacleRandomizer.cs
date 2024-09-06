using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(-5)]
public class ObstacleRandomizer : MonoBehaviour
{
    [SerializeField] private Bounds _spawnBounds;
    [SerializeField] private PathFindingObstacle _obstaclePrefab;
    [SerializeField] private NPCStuff _npcPrefab;
    [SerializeField] private int _npcsToSpawn = 2;
    
    [SerializeField] private int _minObstacles = 20, _maxObstacles = 60;
    [SerializeField] private float _gridSize = 1.0f;
    
    private void Awake()
    {
        var gridWidth = Mathf.CeilToInt(_spawnBounds.size.x / _gridSize);
        var gridHeight = Mathf.CeilToInt(_spawnBounds.size.y / _gridSize);
    
        //wszystkie możliwe pozycje na siatce.
        var gridPositions = new List<Vector3>();
        for (var x = 0; x < gridWidth; x++)
        {
            for (var y = 0; y < gridHeight; y++)
            {
                var gridPos = new Vector3(
                    _spawnBounds.min.x + x * _gridSize + _gridSize / 2,
                    _spawnBounds.min.y + y * _gridSize + _gridSize / 2,
                    0);
                gridPositions.Add(gridPos);
            }
        }
        
        // Spawn npc w losowych pozycjach, po czym usuwa pozycje, żeby przeszkoda nie została wygenerowana na nim.
        for (var i = 0; i < _npcsToSpawn; i++)
        {
            var randomIndex = Random.Range(0, gridPositions.Count);
            var spawnPosition = gridPositions[randomIndex];
            gridPositions.RemoveAt(randomIndex);
            
            var created = Instantiate(_npcPrefab, spawnPosition, Quaternion.identity);
            created.gameObject.name += " " + (i + 1); //dodaje liczbe do nazwy, żeby łatwiej było rozpoznać w konsoli.
        }
        
        
        var obstaclesToCreate = Random.Range(_minObstacles, _maxObstacles + 1);
        obstaclesToCreate = Mathf.Min(obstaclesToCreate, gridPositions.Count); //żeby nie próbowało stworzyć przeszkód więcej niż kratek.
        
        for (var i = 0; i < obstaclesToCreate; i++)
        {
            var randomIndex = Random.Range(0, gridPositions.Count);
            var pos = gridPositions[randomIndex];
            gridPositions.RemoveAt(randomIndex);
            
            Instantiate(_obstaclePrefab, pos, Quaternion.identity, transform);
        }
    }

    //rysuje _spawnBounds
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_spawnBounds.center, _spawnBounds.size);
    }
}
