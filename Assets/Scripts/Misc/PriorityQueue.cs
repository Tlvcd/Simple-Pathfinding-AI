using System.Collections.Generic;
using System.Linq;

public class PriorityQueue<T>
{
    private readonly List<(T obj, float priority)> _elements = new();

    public int Count => _elements.Count;

    /// <summary>
    /// Add given item to Queue and assign item the given priority value.
    /// </summary>
    /// <param name="item">Item to be added.</param>
    /// <param name="priority">Item priority.</param>
    public void Enqueue(T item, float priority)
    {
        _elements.Add((item, priority));
    }


    /// <summary>
    /// Return lowest priority value item and remove item from Queue.
    /// </summary>
    /// <returns>Queue item with lowest priority value.</returns>
    public T Dequeue()
    {
        if (Count == 0) return default;
        var bestPriorityIndex = 0;

        for (var i = 1; i < Count; i++)
        {
            if (_elements[i].priority < _elements[bestPriorityIndex].priority)
            {
                bestPriorityIndex = i;
            }
        }
        var obj = _elements[bestPriorityIndex].obj;
        _elements.RemoveAt(bestPriorityIndex);
        return obj;
    }

    public bool Contains(T item)
    {
        return _elements.Any(element => element.obj.Equals(item));
    }

}