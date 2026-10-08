public enum EnemyStateId : byte
{
    General,
    Notice,
    Battle,
    Attack,
    Groggy,
    Defend,
    Die
}

public interface IEnemyState
{
    void Enter(EnemyController enemy);
    void Tick(EnemyController enemy, float dt);
    void Exit(EnemyController enemy);
}