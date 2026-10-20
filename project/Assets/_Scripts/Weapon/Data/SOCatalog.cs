using UnityEngine;

namespace Project02.Weapon
{
    [CreateAssetMenu(fileName = "SOCatalog", menuName = "Scriptable Objects/SOCatalog")]
    public class SOCatalog : ScriptableObject
    {
        [SerializeField] private SOWeapon[] _weapons;

        public SOWeapon Get(int id) => id >= 0 && id < _weapons.Length ? _weapons[id] : null;
    }
}