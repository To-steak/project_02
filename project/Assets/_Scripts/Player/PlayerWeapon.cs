using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>사격 자세. 자세마다 확산 배율이 다르다.</summary>
public enum FireStance : byte
{
    Idle,
    Walk,
    Run,
    Ads
}

/// <summary>
/// 무기 장착과 사격 규칙(연사, 블룸, 확산).
/// 서버(PlayerServer)와 소유자 클라이언트(PlayerClient)가 같은 입력으로 TryFire를 호출하므로,
/// 사격 판정과 확산 방향은 양쪽에서 같은 결과가 나와야 한다.
/// 반동과 화면 흔들림은 소유자 클라이언트에서만 쓴다.
/// </summary>
public class PlayerWeapon : MonoBehaviour
{
    /// <summary>한 발의 사격 결과. 확산은 조준 방향 기준 회전 각도(도, x: 좌우, y: 상하).</summary>
    public readonly struct Shot
    {
        public readonly FireStance Stance;
        public readonly Vector2 Spread;

        public Shot(FireStance stance, Vector2 spread)
        {
            Stance = stance;
            Spread = spread;
        }
    }

    public SOWeapon Data => _data;
    public Vector3 Muzzle => _view != null ? _view.Muzzle : Origin;
    public Vector3 Origin => _origin.position;

    [SerializeField] private WeaponCatalog _catalog;
    [SerializeField] private Transform _origin;
    [SerializeField] private Transform _socket;

    [SerializeField] private RigBuilder _rigBuilder;
    [SerializeField] private TwoBoneIKConstraint _rightHandIK;
    [SerializeField] private TwoBoneIKConstraint _leftHandIK;

    public bool HasWeapon => _data != null;
    public int RapidFireTick => Mathf.Max(1, Mathf.RoundToInt(_data.RapidFire / Time.fixedDeltaTime));

    private SOWeapon _data;
    private WeaponView _view;

    private int _nextFireTick;
    private int _lastFireTick;
    private float _bloom;

    /// <summary>
    /// 입력으로 이번 틱에 발사하는지 판정하고, 발사하면 블룸과 확산을 계산한다.
    /// 반드시 이동 시뮬레이션(Simulate) 직후에 호출한다. running은 그 결과(PlayerLocomotion.IsRunning)다.
    /// </summary>
    public bool TryFire(in InputPayload input, bool running, ulong ownerId, out Shot shot)
    {
        shot = default;
        if (!HasWeapon)
        {
            return false;
        }

        bool trigger = input.AttackInput || input.AttackPressed;
        if (!trigger || input.Tick < _nextFireTick)
        {
            return false;
        }

        RecoverBloom(input.Tick);

        FireStance stance = GetStance(input, running);
        float angle = GetSpread(stance);
        Vector2 spread = SpreadOffset(ownerId, input.Tick) * angle;

        _bloom = Mathf.Min(_bloom + _data.SpreadPerShot, _data.MaxSpread);
        _lastFireTick = input.Tick;
        _nextFireTick = input.Tick + RapidFireTick;

        shot = new Shot(stance, spread);
        return true;
    }

    /// <summary>조준점을 향하는 방향에 확산 회전을 적용한 발사 방향.</summary>
    public static Vector3 GetDirection(Vector3 from, Vector3 aimPoint, Vector2 spread)
    {
        Quaternion look = Quaternion.LookRotation(aimPoint - from);
        return look * Quaternion.Euler(-spread.y, spread.x, 0.0f) * Vector3.forward;
    }

    /// <summary>소유자 클라이언트 전용. 카메라에 더할 반동(x: yaw, y: 위쪽 pitch, 도).</summary>
    public Vector2 RollRecoil(FireStance stance)
    {
        float multiplier = stance == FireStance.Ads ? _data.AdsRecoilMultiplier : 1.0f;
        return new Vector2(Random.Range(-_data.RecoilHorizontal, _data.RecoilHorizontal), _data.RecoilVertical) * multiplier;
    }

    /// <summary>소유자 클라이언트 전용. 화면 흔들림.</summary>
    public void PlayFeedback(FireStance stance)
    {
        if (_view == null)
        {
            return;   // 프리팹이 없는 무기 데이터
        }

        float multiplier = stance == FireStance.Ads ? _data.AdsRecoilMultiplier : 1.0f;
        _view.Shake(multiplier, _data.ShakeRandom);
    }

    public void Equip(int id, bool createView)
    {
        if (_view != null)
        {
            Destroy(_view.gameObject);
            _view = null;
        }

        _data = _catalog.Get(id);
        _nextFireTick = 0;
        _lastFireTick = 0;
        _bloom = 0.0f;

        bool hasView = createView && _data != null && _data.Prefab != null;
        if (hasView)
        {
            _view = Instantiate(_data.Prefab, _socket);
            _rightHandIK.data.target = _view.RightGrip;
            _leftHandIK.data.target = _view.LeftGrip;
        }

        _rightHandIK.weight = hasView ? 1.0f : 0.0f;
        _leftHandIK.weight = hasView ? 1.0f : 0.0f;

        if (createView)
        {
            _rigBuilder.Build();
        }
    }

    private static FireStance GetStance(in InputPayload input, bool running)
    {
        if (input.AimInput)
        {
            return FireStance.Ads;
        }

        if (input.MoveInput == Vector2.zero)
        {
            return FireStance.Idle;
        }

        return running ? FireStance.Run : FireStance.Walk;
    }

    private float GetSpread(FireStance stance)
    {
        return stance switch
        {
            FireStance.Ads => Mathf.Min(_bloom, _data.MaxSpread) * _data.AdsSpreadMultiplier,
            FireStance.Walk => Mathf.Min(_bloom + _data.HipSpread, _data.MaxSpread) * _data.WalkSpreadMultiplier,
            FireStance.Run => Mathf.Min(_bloom + _data.HipSpread, _data.MaxSpread) * _data.RunSpreadMultiplier,
            _ => Mathf.Min(_bloom + _data.HipSpread, _data.MaxSpread) * _data.IdleSpreadMultiplier,
        };
    }

    /// <summary>
    /// 마지막 발사 이후 흐른 틱으로 블룸 회복을 한 번에 계산한다.
    /// 매 틱 갱신하지 않아도 서버와 클라이언트가 같은 값을 얻는다.
    /// </summary>
    private void RecoverBloom(int tick)
    {
        float elapsed = (tick - _lastFireTick) * Time.fixedDeltaTime;
        float recovering = elapsed - _data.SpreadRecoveryDelay;
        if (recovering > 0.0f)
        {
            _bloom = Mathf.Max(0.0f, _bloom - _data.SpreadRecovery * recovering);
        }
    }

    /// <summary>
    /// (소유자, 틱)으로 정해지는 원 안의 균일한 점.
    /// UnityEngine.Random을 쓰면 서버와 클라이언트의 탄 방향이 달라지므로 쓰지 않는다.
    /// </summary>
    private static Vector2 SpreadOffset(ulong ownerId, int tick)
    {
        uint hash = Hash((uint)ownerId * 0x9E3779B9u ^ (uint)tick);
        float u = (hash & 0xFFFF) / 65535.0f;
        float v = (hash >> 16) / 65535.0f;

        float angle = u * Mathf.PI * 2.0f;
        float radius = Mathf.Sqrt(v);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }

    private static uint Hash(uint x)
    {
        x ^= x >> 16;
        x *= 0x7FEB352Du;
        x ^= x >> 15;
        x *= 0x846CA68Bu;
        x ^= x >> 16;
        return x;
    }
}