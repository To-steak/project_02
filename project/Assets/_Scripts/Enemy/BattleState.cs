using UnityEngine;

public class BattleState : IEnemyState
{
    public void Enter(EnemyController enemy)
    {
        // throw new System.NotImplementedException();
    }

    public void Exit(EnemyController enemy)
    {
        // throw new System.NotImplementedException();
    }

    public void Tick(EnemyController enemy, float dt)
    {
        EnemySettings settings = enemy.Settings;

        if (!enemy.TryGetTarget(out Transform target, out _))
        {
            if (enemy.TryFindClosestPlayer(settings.PerceptionRange, out ulong next))
            {
                enemy.TargetClientId = next;
                return;
            }

            enemy.ChangeState(EnemyStateId.General);
            return;
        }

        Vector3 direction = target.position - enemy.transform.position;
        direction.y = 0.0f;
        float distance = direction.magnitude;
        if (distance > settings.BattleRange)
        {
            enemy.ChangeState(EnemyStateId.General);
            return;
        }

        if (distance > settings.AttackRange)
        {
            enemy.MoveInput = new Vector2(direction.x, direction.z).normalized;
            enemy.MoveSpeed = settings.RunSpeed;
        }

    }
}