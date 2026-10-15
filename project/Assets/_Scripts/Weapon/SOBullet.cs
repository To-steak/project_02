using UnityEngine;

namespace Project02.Weapon
{
    [CreateAssetMenu(fileName = "SOBullet", menuName = "Scriptable Objects/SOBullet")]
    public class SOBullet : ScriptableObject
    {
        public float Damage = 1.0f;
        public float Velocity = 1.0f;
    }
}