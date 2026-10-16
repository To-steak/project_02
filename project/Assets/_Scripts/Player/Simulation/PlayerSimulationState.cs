using Project02.Character;

namespace Project02.Player
{
    public struct StaminaState
    {
        public float Energy;
        public float MaxEnergy;
        public bool IsExhausted;
    }

    public struct WeaponState
    {
        public float Bloom;
        public int LastFireTick;
        public int NextFireTick;
    }

    public struct PlayerSimulationState
    {
        public int Tick;
        public MovementState Movement;
        public StaminaState Stamina;
        public WeaponState Weapon;
    }
}