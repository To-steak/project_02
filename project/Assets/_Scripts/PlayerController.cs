using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerInputs inputs;

    void Awake()
    {
        // GetComponent
        inputs = GetComponent<PlayerInputs>();

        // Initialzied
        inputs.Initialzied();
    }
}
