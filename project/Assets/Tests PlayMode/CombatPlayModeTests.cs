using System.Collections;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;

public class CombatPlayModeTests
{
    private GameObject _target;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        NetworkManager nm = NetworkManager.Singleton;
        Assert.IsNotNull(nm, "Bootstrapper가 Services 프리팹을 만들지 않았다.");
        Assert.IsTrue(nm.StartServer());
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_target != null) Object.Destroy(_target);
        NetworkManager.Singleton.Shutdown();
        yield return null;
    }

    private Health SpawnTarget(Vector3 position, int layer)
    {
        _target = GameObject.CreatePrimitive(PrimitiveType.Cube);   // Default 레이어, BoxCollider 포함
        _target.layer = layer;
        _target.transform.position = position;
        _target.AddComponent<NetworkObject>();
        Health health = _target.AddComponent<Health>();              // 기본 최대 체력 100
        _target.GetComponent<NetworkObject>().Spawn();
        Physics.SyncTransforms();
        return health;
    }

    [UnityTest]
    public IEnumerator 발사한_총알이_Health를_깎는다()
    {
        int layer = LayerMask.NameToLayer("Obstacle");   // 게임에서 적이 쓰는 레이어
        Health health = SpawnTarget(new Vector3(0, 0, 5), layer);
        int before = health.Current.Value;

        GameServices.Projectiles.Spawn(Vector3.zero, new Vector3(0, 0, 100), 1f, damage: 3, owner: 7);

        for (int i = 0; i < 10; i++) yield return new WaitForFixedUpdate();

        Assert.AreEqual(before - 3, health.Current.Value);
    }

    [UnityTest]
    public IEnumerator 죽으면_피해를_준_모든_플레이어가_기여자로_나온다()
    {
        int layer = LayerMask.NameToLayer("Obstacle");   // 게임에서 적이 쓰는 레이어
        Health health = SpawnTarget(new Vector3(0, 0, 5), layer);
        ulong[] contributors = null;
        health.Died += (_, c) => contributors = c;

        health.ApplyDamage(60, 1);
        health.ApplyDamage(30, 2);
        health.ApplyDamage(30, 1);   // 같은 사람이 또 때려도 한 번만 세야 한다
        yield return null;

        Assert.IsTrue(health.IsDead);
        CollectionAssert.AreEquivalent(new ulong[] { 1, 2 }, contributors);
    }
}