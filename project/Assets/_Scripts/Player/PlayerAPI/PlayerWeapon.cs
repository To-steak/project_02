using System.Collections.Generic;
using UnityEngine;

namespace PlayerAPI
{
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] public Transform WeaponSocket;
        [SerializeField] private WeaponHandSync _handSync;

        public List<GameObject> WeaponList;

        public void Initialize()
        {
            
        }
    }
}