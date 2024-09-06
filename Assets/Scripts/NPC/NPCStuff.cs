using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class NPCStuff : MonoBehaviour
{
    [SerializeField] private float _rotSpeed = 90f;
    [SerializeField] private float _colLerpDuration = 2f;
    [SerializeField] private Bounds _randomPointBounds;
    
    [SerializeField] private Color _defaultColor = Color.red;
    
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    
    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;

    private Quaternion _defaultRotation;
    private Vector2 _defaultPos;

    private Color _currentColor;
    
    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(_propertyBlock);
        _currentColor = _defaultColor;
        
        _propertyBlock.SetColor(ColorID, _defaultColor);
        _renderer.SetPropertyBlock(_propertyBlock);
        
        var trans = transform;
        _defaultRotation = trans.rotation;
        _defaultPos = trans.position;
    }

    public Vector2 GetStartingPosition() => _defaultPos;

    public Vector2 GetRandomPosition()
    {
        return new Vector2(Random.Range(_randomPointBounds.min.x, _randomPointBounds.max.x),
            Random.Range(_randomPointBounds.min.y, _randomPointBounds.max.y));
    }

    public void DoColorAnimation()
    {
        StopAllCoroutines();
        ResetState();
        StartCoroutine(Co_FadeColorInOut());
    }

    public void DoRotationAnimation()
    {
        StopAllCoroutines();
        ResetState();
        StartCoroutine(Co_Rotate());
    }

    public void Warp(Vector2 pos)
    {
        StopAllCoroutines();
        ResetState();
        StartCoroutine(Co_Warp(pos));
    }
    
    private void ResetState()
    {
        transform.rotation = _defaultRotation;
        _propertyBlock.SetColor(ColorID, _currentColor);
        _renderer.SetPropertyBlock(_propertyBlock);
    }

    private IEnumerator Co_FadeColorInOut()
    {
        var randomColor = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
        yield return Co_LerpColor(_currentColor, randomColor, _colLerpDuration);
        _currentColor = randomColor;
    }

    private IEnumerator Co_Warp(Vector2 pos)
    {
        var alphaZero = _currentColor;
        alphaZero.a = 0;
        yield return Co_LerpColor(_currentColor, alphaZero, _colLerpDuration);
        var trans = transform;
        trans.position = new Vector3(pos.x, pos.y, trans.position.z);
        yield return new WaitForSeconds(1f);
        yield return Co_LerpColor(alphaZero, _currentColor, _colLerpDuration);
    }

    private IEnumerator Co_LerpColor(Color from, Color to, float duration)
    {
        var elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            var t = elapsedTime / duration;
            var lerpedColor = Color.Lerp(from, to, t);

            _propertyBlock.SetColor(ColorID, lerpedColor);
            _renderer.SetPropertyBlock(_propertyBlock);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        _propertyBlock.SetColor(ColorID, to);
        _renderer.SetPropertyBlock(_propertyBlock);
    }
    
    private IEnumerator Co_Rotate()
    {
        var totalRot = 0f;
        const float rotAmount = 360f;
        
        while (totalRot < rotAmount)
        {
            var rotationStep = _rotSpeed * Time.deltaTime;
            transform.Rotate(0f, 0f, rotationStep);
            totalRot += rotationStep;
            
            if (totalRot > rotAmount)
            {
                totalRot = rotAmount;
            }

            yield return null;
        }

        transform.rotation = _defaultRotation;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_randomPointBounds.center, _randomPointBounds.size);
    }
}
