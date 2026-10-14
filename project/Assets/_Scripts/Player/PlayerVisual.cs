using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private Transform _visual;

    private readonly VisualInterpolator<Vector3> _position = new(Vector3.Lerp);

    private const float SNAP_DISTANCE = 1f;
    private const float MIN_FACE_SQR = 0.01f;

    private PlayerSettings _settings;

    public void Initialize(Vector3 position, PlayerSettings settings)
    {
        _settings = settings;
        _position.Reset(position);
        _visual.rotation = transform.rotation;
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

    /// <summary>
    /// 소유자 전용, 매 프레임. 총구에서 조준점을 향하도록 모델을 수평으로 돌린다.
    /// 카메라가 어깨 옆에 있어서 생기는 시차를 보정한다. 판정용 루트 회전에는 영향이 없다.
    /// </summary>
    public void Face(Vector3 aimPoint, Vector3 muzzle, float deltaTime)
    {
        Vector3 direction = aimPoint - muzzle;
        direction.y = 0.0f;
        if (direction.sqrMagnitude < MIN_FACE_SQR)
        {
            return;
        }

        Quaternion look = Quaternion.LookRotation(direction);
        _visual.rotation = Quaternion.RotateTowards(_visual.rotation, look, _settings.TurnSpeed * deltaTime);
    }

    /// <summary>
    /// 월드 좌표를 루트 기준 로컬 좌표로. 보간으로 생긴 위치 차이와 모델의 추가 회전을 빼서,
    /// 서버가 자기 루트에 TransformPoint로 되돌릴 수 있는 값을 만든다.
    /// </summary>
    public Vector3 ToLocal(Vector3 world)
    {
        return Quaternion.Inverse(transform.rotation) * (world - _visual.position);
    }
}