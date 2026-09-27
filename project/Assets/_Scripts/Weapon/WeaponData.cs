using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string DisplayName = "Default Name";
    public float WeaponCoefficient = 1.0f;
    [Min(0.01f)] public float RapidFire = 1.0f;
}
