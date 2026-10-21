using System.Collections.Generic;
using UnityEngine;
using Project02.Character;
using Project02.Combat;
using Project02.Weapon;
using Project02.World;

namespace Project02.Player
{
    public static class ServerPlayerSystem
    {
        private const int START_BUFFER = 2;
        private const float MAX_LAG_TICKS = 25.0f;

        public static void Tick(ServerWorld world, float deltaTime)
        {
            foreach (PlayerServerAuthoring player in world.Players)
            {
                InputQueue queue = player.Inputs;

                // 1. 수신: RPC가 쌓아 둔 번들을 버퍼로
                Receive(queue, player.Network.ReceivedInputs);

                // 2. 시작 대기: 버퍼가 START_BUFFER만큼 찰 때까지
                if (!queue.Started)
                {
                    if (queue.LatestTick - queue.ConsumedTick < START_BUFFER) continue;
                    queue.Started = true;
                }

                // 3. 소비: 다음 틱 입력 (없으면 복제) + 늦게 온 눌림 합치기
                InputPayload input = Consume(queue);

                // 4. 시뮬레이션 (예측과 같은 함수)
                PlayerSimulation sim = player.Simulation;
                if (PlayerStepUtil.Step(sim, input, player.OwnerClientId, deltaTime, out ShotInfo shot))
                {
                    world.Projectiles.Add(CreateProjectile(player.Settings, sim, player.OwnerClientId, input, shot, world.Clock.Tick));
                    world.Outbox.Fires.Add(new FireMessage(player, world.Clock.Tick, input.AimPoint, shot.Spread));
                }

                // 5. 출력: 클라가 없는 프로세스에서만 (호스트는 ClientLoop가 쓴다)
                if (world.WriteTransforms)
                {
                    PlayerStepUtil.WriteTransform(player.transform, sim);
                }

                // 6. 결과 기록 (송신은 NetSendSystem)
                StatePayload payload = new StatePayload
                {
                    State = sim.State,
                    InputBuffer = (sbyte)Mathf.Clamp(queue.LatestTick - queue.ConsumedTick, sbyte.MinValue, sbyte.MaxValue),
                    AckTick = queue.AckTick,
                };
                world.Outbox.States.Add(new StateMessage(player, payload));
            }
        }

        private static void Receive(InputQueue queue, List<InputPayload> received)
        {
            for (int i = 0; i < received.Count; i++)
            {
                InputPayload payload = received[i];

                // 이미 지나간 틱: 복제로 때운 틱에 진짜 입력이 늦게 왔으면 눌림만 살린다
                if (payload.Tick <= queue.ConsumedTick)
                {
                    bool lateForDuplicatedTick = !queue.Buffer.TryGetValue(payload.Tick, out _) && payload.Tick > queue.ConsumedTick - InputQueue.CAPACITY;
                    if (lateForDuplicatedTick)
                    {
                        queue.CarryJump |= payload.JumpInput;
                        queue.CarryAttack |= payload.AttackPressed;
                        queue.Buffer.SetValue(payload.Tick, payload);
                    }
                    continue;
                }

                // 너무 먼 미래: 클라 시계가 튐 → 재동기화
                if (payload.Tick > queue.ConsumedTick + InputQueue.CAPACITY)
                {
                    Resync(queue, payload.Tick);
                }

                queue.Buffer.SetValue(payload.Tick, payload);
                if (payload.Tick > queue.LatestTick)
                {
                    queue.LatestTick = payload.Tick;
                }
            }

            received.Clear();
            AdvanceAck(queue);
        }

        // ack: 버퍼에 이어서 있는 틱까지 올린다. 영영 오지 않는 틱(복제로 때우고 지나감) 때문에 멈추지 않게,
        // 소비 틱보다 CAPACITY/2 넘게 뒤처지면 소비 틱으로 건너뛴 뒤 다시 이어 올린다
        private static void AdvanceAck(InputQueue queue)
        {
            if (queue.AckTick < queue.ConsumedTick - InputQueue.CAPACITY / 2)
            {
                queue.AckTick = queue.ConsumedTick;
            }

            while (queue.Buffer.TryGetValue(queue.AckTick + 1, out _))
            {
                queue.AckTick++;
            }
        }

        private static InputPayload Consume(InputQueue queue)
        {
            int tick = queue.ConsumedTick + 1;

            InputPayload input;
            if (queue.Buffer.TryGetValue(tick, out InputPayload slot))
            {
                input = slot;
                queue.LastInput = slot;
            }
            else
            {
                input = queue.LastInput;   // 복제 (눌림은 복제하지 않음)
                input.Tick = tick;
                input.JumpInput = false;
                input.AttackPressed = false;
            }

            queue.ConsumedTick = tick;

            input.JumpInput |= queue.CarryJump;
            input.AttackPressed |= queue.CarryAttack;
            queue.CarryJump = queue.CarryAttack = false;
            return input;
        }

        private static void Resync(InputQueue queue, int tick)
        {
            queue.Buffer.Clear();
            queue.CarryJump = queue.CarryAttack = false;
            queue.ConsumedTick = tick - 1;
            queue.LatestTick = tick - 1;
            queue.AckTick = tick - 1;
        }

        private static Projectile CreateProjectile(SOPlayer settings, PlayerSimulation sim, ulong ownerId, in InputPayload input, in ShotInfo shot, int serverTick)
        {
            SOWeapon data = sim.Weapon;
            SOBullet bullet = data.Bullet;

            MovementState movement = sim.State.Movement;
            Vector3 origin = WeaponFireUtil.GetOrigin(movement, input.YawInput, settings.FireOriginOffset);
            Vector3 direction = WeaponFireUtil.GetDirection(origin, input.AimPoint, shot.Spread);
            float damage = bullet.Damage * data.WeaponCoefficient;
            bool isCritical = WeaponFireUtil.RollCritical(ownerId, input.Tick, data.CriticalChance);

            float lag = input.ViewTick < 0.0f ? 1.0f : Mathf.Clamp(serverTick - input.ViewTick, 1.0f, MAX_LAG_TICKS);

            return new Projectile(origin, direction * bullet.Velocity, damage, ownerId, DamageSource.Player, Team.Player, isCritical, data.Range, data.Falloff, lag);
        }
    }
}