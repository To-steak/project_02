using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string DisplayName = "Default Name";
    public float WeaponCoefficient = 1.0f;
    [Tooltip("발사 간격(초). 고정 틱 단위로 반올림되며 최소 1틱이다.")]
    [Min(0.01f)] public float RapidFire = 1.0f;
}
