using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string DisplayName = "Default Name";
    public float WeaponCoefficient = 1.0f;
    [Tooltip("발사 간격(초). 고정 틱 단위로 반올림되며 최소 1틱이다.")]
    [Min(0.01f)] public float RapidFire = 1.0f;
    public BulletData Bullet;
    public WeaponView Prefab;

    [Header("Critical")]
    [Tooltip("치명타 확률. 피해량을 늘리지 않고 경직을 준다.")]
    [Range(0.0f, 1.0f)]public float CriticalChance;

    [Header("Noise")]
    [Tooltip("사격 소음이 몬스터에게 들리는 유효거리")]
    [Min(0f)] public float NoiseRange = 30.0f;

    [Tooltip("소음기 장착 여부. 장착시 몬스터 어그로 횟수 감소")]
    public bool IsSuppressed;
}
