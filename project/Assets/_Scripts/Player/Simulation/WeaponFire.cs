using Project02.Character;
using Project02.Weapon;
using UnityEngine;

namespace Project02.Player
{
    public static class WeaponFire
    {
        private const uint CRITICAL_SALT = 0x68E31DA4u;

        public static bool TryFire(SOWeapon settings, ref WeaponState state, in InputPayload input, bool isRunning, ulong ownerId, float deltaTime, out Shot shot)
        {
            shot = default;
            if (settings == null)
            {
                return false;
            }

            bool trigger = input.AttackInput || input.AttackPressed;
            if (!trigger || input.Tick < state.NextFireTick)
            {
                return false;
            }

            int elapsed = input.Tick - state.LastFireTick;
            int recovered = elapsed - settings.SpreadRecoveryDelayTick;
            if (recovered > 0)
            {
                state.Bloom = Mathf.Max(0.0f, state.Bloom - settings.SpreadRecovery * recovered * deltaTime);
            }

            FireStance stance = GetStance(input, isRunning);
            float angle = GetSpread(stance, settings, state.Bloom);
            Vector2 spread = SpreadOffset(ownerId, input.Tick) * angle;

            state.Bloom = Mathf.Min(state.Bloom + settings.SpreadPerShot, settings.MaxSpread);
            state.LastFireTick = input.Tick;
            state.NextFireTick = input.Tick + settings.RapidFireTick;
            shot = new Shot(stance, spread);
            return true;
        }

        public static Vector3 GetOrigin(in MovementState movement, float yaw, Vector3 offset)
        {
            return movement.Position + Quaternion.Euler(0.0f, yaw, 0.0f) * offset;
        }

        public static bool RollCritical(ulong ownerId, int tick, float chance)
        {
            uint hash = Hash((uint)ownerId * 0x9E3779B9u ^ (uint)tick ^ CRITICAL_SALT);
            return (hash & 0xFFFFFF) / 16777216.0f < chance;   // 하위 24비트로 0~1
        }
        
        private static FireStance GetStance(in InputPayload input, bool value)
        {
            if (input.AimInput)
            {
                return FireStance.Ads;
            }
            if (input.MoveInput == Vector2.zero)
            {
                return FireStance.Idle;
            }

            return value ? FireStance.Run : FireStance.Walk;
        }

        private static float GetSpread(FireStance stance, SOWeapon settings, float bloom)
        {
            return stance switch
            {
                FireStance.Ads => Mathf.Min(bloom, settings.MaxSpread) * settings.AdsSpreadMultiplier,
                FireStance.Walk => Mathf.Min(bloom + settings.HipSpread, settings.MaxSpread) * settings.WalkSpreadMultiplier,
                FireStance.Run => Mathf.Min(bloom + settings.HipSpread, settings.MaxSpread) * settings.RunSpreadMultiplier,
                _ => Mathf.Min(bloom + settings.HipSpread, settings.MaxSpread) * settings.IdleSpreadMultiplier,
            };
        }

        private static Vector2 SpreadOffset(ulong id, int tick)
        {
            uint hash = Hash((uint)id * 0x9E3779B9u ^ (uint)tick);
            float u = (hash & 0xFFFF) / 65535.0f;
            float v = (hash >> 16) / 65535.0f;

            float angle = u * Mathf.PI * 2.0f;
            float radius = Mathf.Sqrt(v);
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        private static uint Hash(uint x)
        {
            x ^= x >> 16;
            x *= 0x7FEB352Du;
            x ^= x >> 15;
            x *= 0x846CA68Bu;
            x ^= x >> 16;
            return x;
        }
    }
}