using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

public class PathFinder : MonoBehaviour
{
    [SerializeField] private float _cellSize = 1.0f;
    [SerializeField] private Vector2Int _gridSize = new Vector2Int(30, 30);
    [SerializeField] private float _obstacleCheckEpsilon = 0.01f; //granica tolerancji przy sprawdzaniu czy linie się przecinają.

    private readonly List<Vector2[]> _obstacles = new();
    private readonly Dictionary<Vector2, List<Vector2>> _neighborCache = new();
    private readonly HashSet<Vector2> _unavailableNodes = new(); //tylko do wyświetlania gdzie są przeszkody na siatce gizmos.

    void Start()
    {
        var obstacles = FindObjectsOfType<PathFindingObstacle>();
        foreach (var obstacle in obstacles)
        {
            _obstacles.Add(obstacle.GetObstaclePoints());
        }

        InitializeNeighborCache(); //wypalanie siatki, żeby szukanie było szybsze.
    }

    #region Wypalanie siatki
    private void InitializeNeighborCache()
    {
        var centerOffset = (Vector2)transform.position - _gridSize / 2;
        for (float x = 0; x < _gridSize.x; x += _cellSize)
        {
            for (float y = 0; y < _gridSize.y; y += _cellSize)
            {
                var node = new Vector2(
                    Mathf.Floor(x / _cellSize) * _cellSize + _cellSize / 2 + centerOffset.x,
                    Mathf.Floor(y / _cellSize) * _cellSize + _cellSize / 2 + centerOffset.y
                );

                if (!IsPointInObstacle(node))
                {
                    _neighborCache[node] = GetNeighborsBounded(node);
                }
                else
                {
                    _unavailableNodes.Add(node);
                }
            }
        }
    }

    private bool IsPointInObstacle(Vector2 point)
    {
        foreach (var obstacle in _obstacles)
        {
            if (IsPointInSquare(point, obstacle))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsPositionValid(Vector2 point)
    {
        return _neighborCache.ContainsKey(SnapToGrid(point));
    }

    //sprawdza czy punkt jest w granicach wierzchołków kwadratu
    private bool IsPointInSquare(Vector2 point, Vector2[] square)
    {
        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;

        foreach (var vertex in square)
        {
            minX = Mathf.Min(vertex.x, minX);
            minY = Mathf.Min(vertex.y, minY);
            maxX = Mathf.Max(vertex.x, maxX);
            maxY = Mathf.Max(vertex.y, maxY);
        }
        
        return point.x.InBetween(minX, maxX) && point.y.InBetween(minY, maxY);
    }

    //znajduje sąsiadów w granicy siatki.
    private List<Vector2> GetNeighborsBounded(Vector2 node)
    {
        var potentialNeighbors = GetPotentialNeighbors(node);

        var centerOffset = (Vector2)transform.position - _gridSize / 2;
        return potentialNeighbors.Where(neighbor =>
            neighbor.x >= centerOffset.x && neighbor.x < centerOffset.x + _gridSize.x &&
            neighbor.y >= centerOffset.y && neighbor.y < centerOffset.y + _gridSize.y && //sprawdza czy mieści się w wypalonej siatce
            !IntersectsObstacle(node, neighbor)).ToList();//sprawdza czy któraś z linii nie przechodzi przez przeszkodę.
    }
    
    //znajduje sąsiadów poza granicami
    private List<Vector2> GetNeighbors(Vector2 node)
    {
        var potentialNeighbors = GetPotentialNeighbors(node);

        return potentialNeighbors.Where(neighbor => !IntersectsObstacle(node, neighbor)).ToList();//sprawdza czy któraś z linii nie przechodzi przez przeszkodę.
    }

    //8 kierunków
    private Vector2[] GetPotentialNeighbors(Vector2 node)
    {
        return new[]
        {
            node + Vector2.up * _cellSize,
            node + Vector2.down * _cellSize,
            node + Vector2.left * _cellSize,
            node + Vector2.right * _cellSize,
            node + (Vector2.one * _cellSize),
            node + (-Vector2.one * _cellSize),
            node + new Vector2(-_cellSize, _cellSize),
            node + new Vector2(_cellSize, -_cellSize),
        }; 
    }
    
    private bool IntersectsObstacle(Vector2 start, Vector2 end)
    {
        foreach (var obstacle in _obstacles)
        {
            if (LineIntersectsRectangle(start, end, obstacle))
            {
                return true;
            }
        }

        return false;
    }

    private bool LineIntersectsRectangle(Vector2 lineStart, Vector2 lineEnd, Vector2[] obstacle)
    {
        var bottomLeft = Vector2.one * float.MaxValue;
        var topRight = Vector2.one * float.MinValue;
        
        foreach (var vertex in obstacle)
        {
            bottomLeft = Vector2.Min(vertex, bottomLeft);
            topRight = Vector2.Max(vertex, topRight);
        }
        var bottomRight = new Vector2(topRight.x, bottomLeft.y);
        var topLeft = new Vector2(bottomLeft.x, topRight.y);
        
        return LinesIntersect(lineStart, lineEnd, bottomLeft, bottomRight) ||  // Bottom edge
               LinesIntersect(lineStart, lineEnd, bottomRight, topRight) ||    // Right edge
               LinesIntersect(lineStart, lineEnd, topRight, topLeft) ||        // Top edge
               LinesIntersect(lineStart, lineEnd, topLeft, bottomLeft);        // Left edge
    }

    private bool LinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        var aDir = a2 - a1; //kierunek linii a
        var bDir = b2 - b1; //kierunek linii b
        var bDotDPerp = aDir.x * bDir.y - aDir.y * bDir.x; //czy linie są równoległe - jak wynosi 0 to tak

        if (Mathf.Abs(bDotDPerp) < _obstacleCheckEpsilon) //jeżeli linie są równoległe, to sprawdza czy jakiś punkt znajduje się na linii.
        {
            return IsPointOnLine(a1, b1, b2) || IsPointOnLine(a2, b1, b2) ||
                   IsPointOnLine(b1, a1, a2) || IsPointOnLine(b2, a1, a2);
        }

        var abDir = b1 - a1; //kierunek od a do b
        
        var t = (abDir.x * bDir.y - abDir.y * bDir.x) / bDotDPerp; 
        var u = (abDir.x * aDir.y - abDir.y * aDir.x) / bDotDPerp;//kalkulacja w której częsci linie się spotykają.

        return t >= -_obstacleCheckEpsilon && t <= 1 + _obstacleCheckEpsilon &&
               u >= -_obstacleCheckEpsilon && u <= 1 + _obstacleCheckEpsilon; //sprawdza czy t i u mieszczą się w zakresie od 0 do 1
    }

    private bool IsPointOnLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        var d1 = Vector2.Distance(point, lineStart);
        var d2 = Vector2.Distance(point, lineEnd);
        var lineLength = Vector2.Distance(lineStart, lineEnd);
        return Mathf.Abs(d1 + d2 - lineLength) < _obstacleCheckEpsilon;
        //jeżeli suma dystansów a - b i a - c jest równa długości lini to punkt jest w linii
    }
    #endregion

    #region Tworzenie ścieżki
    public IEnumerator Co_FindPath(Vector2 start, Vector2 target, Action<List<Vector2>> callback)
    {
        PriorityQueue<Vector2> openSet = new(); //komórki do sprawdzenia
        HashSet<Vector2> closedSet = new(); //komórki sprawdzone
        Dictionary<Vector2, Vector2> cameFrom = new(); //do późniejszej składania ścieżki
        Dictionary<Vector2, float> gScore = new(); //wyniki komórek.

        start = SnapToGrid(start);
        target = SnapToGrid(target);

        openSet.Enqueue(start, 0);
        gScore[start] = 0;

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue(); //bierze komórkę z kolejki o najniżym priorytecie.

            if (IsCloseToTarget(current, target)) yield break;

            closedSet.Add(current); //dodaje do zamkniętych żeby nie była kilka razy obliczana

            foreach (var neighbor in GetCachedNeighbors(current))
            {
                if (closedSet.Contains(neighbor)) continue;

                var currentGScore = gScore[current] + Vector2.Distance(current, neighbor); //kalkulacja kosztu od obecnej komórki do obecnej, 
                var hScore = Vector2.Distance(neighbor, target); //szacowany koszt od obecnej komórki do celu.
                var fScore = currentGScore + hScore; //suma kosztów.

                Debug.DrawLine(current, neighbor, Color.cyan, 1);

                if (!gScore.ContainsKey(neighbor) || currentGScore < gScore[neighbor]) //jak nie ma w tabeli gScore, lub gScore jest mniejszy od tego co już był
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = currentGScore;
                    openSet.Enqueue(neighbor, fScore); //dodaje do potencjalnych do sprawdzenia, z szacowanym wynikiem
                }

                if (IsCloseToTarget(neighbor, target)) yield break;
            }

            yield return null;
        }

        yield break;

        //kończy szukanie gdy wykryje wystarczająco blisko cel.
        bool IsCloseToTarget(Vector2 current, Vector2 target) 
        {
            if (Vector2.Distance(current, target) > _cellSize) return false;
            cameFrom[target] = current;
            callback(RetracePath(cameFrom, start, target));
            return true;
        }
    }

    private Vector2 SnapToGrid(Vector2 position)
    {
        var centerOffset = (Vector2)transform.position - _gridSize / 2;
        return new Vector2(
            Mathf.Floor((position.x - centerOffset.x) / _cellSize) * _cellSize + _cellSize / 2 + centerOffset.x,
            Mathf.Floor((position.y - centerOffset.y) / _cellSize) * _cellSize + _cellSize / 2 + centerOffset.y
        );
        //najpierw zaokrągla do najbliższej wartości, a później przesuwa
    }

    private List<Vector2> GetCachedNeighbors(Vector2 node)
    {
        var gridNode = SnapToGrid(node);

        if (_neighborCache.TryGetValue(gridNode, out var neighbors))
        {
            return neighbors;
        }

        //na wszelki wypadek, gdyby nie znalazł w wypalonej siatce, lub gdyby się znalazł poza siatką. Jest wolniejsze od cached.
        return GetNeighbors(gridNode);
    }

    private List<Vector2> RetracePath(Dictionary<Vector2, Vector2> cameFrom, Vector2 start, Vector2 end)
    {
        if (!cameFrom.ContainsKey(end))
        {
            Debug.LogError("Cel nie znalazł się na ścieżce, pathfinding zakończony niepowodzeniem.");
            return new List<Vector2>();
        }

        var path = new List<Vector2>();
        var current = end;

        while (current != start) //tworzy ścieżke od końca, aż nie napotka komórki która nawiązuje do startu.
        {
            if (!cameFrom.ContainsKey(current))
            {
                Debug.LogError("Ścieżka nie została dokończona, pathfinding zakończony niepowodzeniem.");
                return new List<Vector2>();
            }

            path.Add(current);
            current = cameFrom[current];
        }

        path.Add(start);//dodaje start bo nie było
        path.Reverse(); //odwraca żeby ścieżka była od początku do końca.
        return path;
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        var centerPos = transform.position;
        var centerOffset = (Vector2)centerPos - _gridSize / 2;

        //granica siatki
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(centerPos, new Vector3(_gridSize.x, _gridSize.y, 0));

        if (_cellSize < 0.2f) return; //dolny limit, bo zacina edytor.
        // Draw grid
        Gizmos.color = Color.gray;
        
        //poziome linie
        for (float x = 0; x <= _gridSize.x; x += _cellSize)
        {
            var startPos = new Vector3(x, 0, 0) + new Vector3(centerOffset.x, centerOffset.y, centerPos.z);
            var endPos = new Vector3(x, _gridSize.y, 0) + new Vector3(centerOffset.x, centerOffset.y, centerPos.z);
            Gizmos.DrawLine(startPos, endPos);
        }

        //pionowe linie
        for (float y = 0; y <= _gridSize.y; y += _cellSize)
        {
            var startPos = new Vector3(0, y, 0) + new Vector3(centerOffset.x, centerOffset.y, centerPos.z);
            var endPos = new Vector3(_gridSize.x, y, 0) + new Vector3(centerOffset.x, centerOffset.y, centerPos.z);
            Gizmos.DrawLine(startPos, endPos);
        }
        
        if (_cellSize < 0.5f) return; //nie rysuje kulek pokazujących dostępność komórki, bo zacina edytor.
        
        for (float x = 0; x < _gridSize.x; x += _cellSize)
        {
            for (float y = 0; y < _gridSize.y; y += _cellSize)
            {
                var cellCenter = new Vector3(
                    x + _cellSize / 2 + centerOffset.x,
                    y + _cellSize / 2 + centerOffset.y,
                    centerPos.z
                );

                if (!_unavailableNodes.Contains(cellCenter))
                {
                    Gizmos.color = Color.yellow; //wolna
                }
                else
                {
                    Gizmos.color = Color.red; //przeszkoda
                }

                Gizmos.DrawSphere(cellCenter, 0.1f);
            }
        }
    }
}