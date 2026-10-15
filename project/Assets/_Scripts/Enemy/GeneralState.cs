namespace Project02.Enemy
{
    public class GeneralState : IEnemyState
    {
        public void Enter(EnemyController enemy)
        {
            enemy.TargetClientId = null;
        }

        public void Exit(EnemyController enemy)
        {
            // throw new System.NotImplementedException();
        }

        public void Tick(EnemyController enemy, float dt)
        {
            if (enemy.TryFindClosestPlayer(enemy.Settings.PerceptionRange, out ulong clientId))
            {
                enemy.TargetClientId = clientId;
                enemy.ChangeState(EnemyStateId.Battle);
                return;
            }

            enemy.MoveInput = enemy.WanderInput(dt);
            enemy.MoveSpeed = enemy.Settings.WalkSpeed;
        }
    }
}