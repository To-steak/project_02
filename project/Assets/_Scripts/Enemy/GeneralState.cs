public class GeneralState : IEnemyState
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
        enemy.MoveInput = enemy.WanderInput(dt);
        enemy.MoveSpeed = enemy.Settings.WalkSpeed;
    }
}