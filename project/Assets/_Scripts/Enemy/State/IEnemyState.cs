namespace Project02.Enemy
{
    public interface IEnemyState
    {
        void Enter(EnemyController enemy);
        void Tick(EnemyController enemy, float dt);
        void Exit(EnemyController enemy);
    }
}