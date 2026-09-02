using UnityEngine;

public class WeaponSocket : MonoBehaviour
{
    [SerializeField] private Transform _leftHandSocket;
    [SerializeField] private Transform _rightHandSocket;

    public Transform LeftHandSocket => _leftHandSocket;
    public Transform RightHandSocket => _rightHandSocket;
}