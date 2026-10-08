using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private Transform _visual;


    private readonly VisualInterpolator<Vector3> _position = new(Vector3.Lerp);

    private const float SNAP_DISTANCE = 1f;

    public void Initialize(Vector3 position)
    {
        _position.Reset(position);
    }

    // FixedUpdate - Simulate() 직후
    public void Record()
    {
        Vector3 position = transform.position;
        if (Vector3.Distance(_position.Current, position) > SNAP_DISTANCE)
        {
            _position.Reset(position);
        }
        else
        {
            _position.Record(position);
        }
    }

    // LateUpdate
    public void Interpolate()
    {
        _visual.position = _position.Interpolate();
    }
}