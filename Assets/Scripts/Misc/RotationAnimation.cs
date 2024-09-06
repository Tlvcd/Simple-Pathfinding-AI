using UnityEngine;
using UnityEngine.Serialization;

public class RotationAnimation : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 10f;
    private Vector3 _randomDir;

    void Start()
    {
        _randomDir = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;
    }

    void Update()
    {
        transform.Rotate(_randomDir * (_rotationSpeed * Time.deltaTime));
    }
}
