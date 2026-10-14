using UnityEngine;

[CreateAssetMenu(fileName = "WeaponCatalog", menuName = "Scriptable Objects/WeaponCatalog")]
public class WeaponCatalog : ScriptableObject
{
    [SerializeField] private SOWeapon[] _weapons;

    public SOWeapon Get(int id) => id >= 0 && id < _weapons.Length ? _weapons[id] : null;
}