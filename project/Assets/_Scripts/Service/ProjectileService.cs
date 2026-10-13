using Unity.Netcode;
using UnityEngine;

public class ProjectileService : MonoBehaviour
{
    public static ProjectileService Instance { get; private set; }

    [SerializeField] private LayerMask _hitLayer;

    private ProjectileSimulator _simulator;

    private void Awake()
    {
        _simulator = new ProjectileSimulator(_hitLayer);
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void FixedUpdate()
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager == null || !networkManager.IsServer)
        {
            return;
        }

        _simulator.Tick(Time.fixedDeltaTime);
    }

    public void Spawn(in Projectile projectile)
    {
        _simulator.Spawn(projectile);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_simulator == null)
        {
            return;
        }

        Gizmos.color = Color.magenta;
        foreach (var projectile in _simulator.Projectiles)
        {
            Gizmos.DrawRay(projectile.Position, projectile.Velocity * Time.fixedDeltaTime);
        }
    }
#endif
}
