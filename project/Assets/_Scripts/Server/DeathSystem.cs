using System.Collections.Generic;
using Project02.Enemy;

namespace Project02.Server
{
    public static class DeathSystem
    {
        public static void Process(List<EnemyController> deaths)
        {
            foreach (EnemyController enemy in deaths)
            {
                enemy.Die();
            }
            deaths.Clear();
        }
    }
}