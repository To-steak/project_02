using UnityEngine;

namespace Project02.Weapon
{
    [CreateAssetMenu(fileName = "SOBullet", menuName = "Scriptable Objects/SOBullet")]
    public class SOBullet : ScriptableObject
    {
        public float Damage = 1.0f;
        public float Velocity = 1.0f;

        [Tooltip("투사체 벽 레이캐스트 대상: 지형·장애물만. Enemy·Player 레이어는 빼야 함 (대상은 되감은 캡슐로 판정)")]
        public LayerMask HitLayer;

        [Tooltip("피격 이펙트 (클라 예측 연출). 비어 있으면 아무것도 하지 않는다")]
        public GameObject ImpactPrefab;
    }
}