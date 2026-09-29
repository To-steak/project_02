// Tests/EditMode/ProjectileSimulatorTests.cs
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class ProjectileSimulatorTests
{
    private const float DT = 0.02f;
    private static readonly LayerMask DEFAULT_LAYER = 1 << 0;

    private readonly List<GameObject> _created = new();

    private class FakeTarget : MonoBehaviour, IDamageable
    {
        public int TotalDamage;
        public ulong? LastAttacker;

        public void ApplyDamage(int amount, ulong? attacker)
        {
            TotalDamage += amount;
            LastAttacker = attacker;
        }
    }

    private FakeTarget CreateWall(Vector3 position, float thickness = 1f, int layer = 0)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = position;
        wall.transform.localScale = new Vector3(5f, 5f, thickness);
        wall.layer = layer;
        _created.Add(wall);

        var target = wall.AddComponent<FakeTarget>();
        Physics.SyncTransforms();   // 에디터에서는 물리 스텝이 없어서 직접 맞춰 줘야 레이가 콜라이더를 본다.
        return target;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var go in _created) Object.DestroyImmediate(go);
        _created.Clear();
    }

    [Test]
    public void 맞으면_피해를_주고_사라진다()
    {
        var target = CreateWall(new Vector3(0, 0, 5));
        var sim = new ProjectileSimulator(DEFAULT_LAYER);
        sim.Spawn(Vector3.zero, new Vector3(0, 0, 100), 1f, damage: 10, owner: 7);

        for (int i = 0; i < 5; i++) sim.Tick(DT);   // 한 틱에 2m, 5m 앞의 벽

        Assert.AreEqual(10, target.TotalDamage);
        Assert.AreEqual(7ul, target.LastAttacker);
        Assert.AreEqual(0, sim.Count);
    }

    [Test]
    public void 빠른_탄도_얇은_벽을_뚫지_않는다()
    {
        var target = CreateWall(new Vector3(0, 0, 5), thickness: 0.05f);
        var sim = new ProjectileSimulator(DEFAULT_LAYER);
        sim.Spawn(Vector3.zero, new Vector3(0, 0, 500), 1f, 10, 1);   // 한 틱에 10m

        sim.Tick(DT);

        Assert.AreEqual(10, target.TotalDamage);
    }

    [Test]
    public void 수명이_다하면_사라진다()
    {
        var sim = new ProjectileSimulator(DEFAULT_LAYER);
        sim.Spawn(Vector3.zero, Vector3.forward, lifespan: 0.1f, 10, 1);

        for (int i = 0; i < 6; i++) sim.Tick(DT);

        Assert.AreEqual(0, sim.Count);
    }

    [Test]
    public void 레이어_마스크에_없는_대상은_통과한다()
    {
        var target = CreateWall(new Vector3(0, 0, 5), layer: 2);   // Ignore Raycast
        var sim = new ProjectileSimulator(DEFAULT_LAYER);
        sim.Spawn(Vector3.zero, new Vector3(0, 0, 100), 1f, 10, 1);

        for (int i = 0; i < 5; i++) sim.Tick(DT);

        Assert.AreEqual(0, target.TotalDamage);
    }

    [Test]
    public void 한_틱에_여러_개가_사라져도_나머지는_모두_움직인다()
    {
        var sim = new ProjectileSimulator(DEFAULT_LAYER);
        sim.Spawn(Vector3.zero, Vector3.forward, lifespan: 0.01f, 1, 1);          // 이번 틱에 사라짐
        sim.Spawn(Vector3.zero, Vector3.forward, lifespan: 0.01f, 1, 1);          // 이번 틱에 사라짐
        sim.Spawn(Vector3.zero, new Vector3(0, 0, 10), lifespan: 1f, 1, 1);       // 살아남아야 함

        sim.Tick(DT);

        Assert.AreEqual(1, sim.Count);
        Assert.AreEqual(0.2f, sim.Projectiles[0].Position.z, 1e-4f);   // 건너뛰지 않고 한 틱만큼 이동
    }
}