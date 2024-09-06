using UnityEngine;

public class PathFindingObstacle : MonoBehaviour
{
    private MeshRenderer _renderer;
    
    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
    }

    public Vector2[] GetObstaclePoints()
    {
        if (!_renderer) return null;

        var bounds = _renderer.bounds;
        var min = bounds.min;
        var max = bounds.max;

        return new Vector2[]
        {
            min,
            max,
            new(min.x, max.y),
            new(max.x, min.y)
        };
    }
}
