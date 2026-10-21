# ECS 전환 작업 규칙 (작업 1~12)

이번 브랜치의 목표는 ECS로의 전환이다. 아래는 12개 작업 각각에서 코드를 어떻게 써야 하는지 정한 규칙이다.

## 정해진 사항

- **게임:** PvE. 플레이어도 나중에 적에게 맞고 죽는다. 그러므로 플레이어도 "맞을 수 있는 대상" 구조에 들어간다.
- **이번 브랜치의 범위:**
  - 하지 않는 것: 적의 공격, 플레이어 사망 처리(리스폰 등). 플레이어가 맞을 수 있는 **구조**까지만 만든다.
  - 플레이어 총알은 플레이어를 판정하지 않는다(같은 편).
- **저장:** 10초 주기 + 접속 종료·서버 종료 시 즉시. 크래시 시 최대 10초분을 잃는다.
- **되감기 상한:** 12틱(240ms). 넘는 지연의 클라는 적을 외삽해서 그린다.
- **물리 씬:** 이동 판정은 Unity 물리를 계속 쓴다. "Ground/Obstacle 레이어에는 정지 콜라이더만"을 규칙으로 고정하고, 위반을 검사한다.

---

## 모든 작업에 적용되는 규칙

`ECS_RULES.md`에서 이번 작업들에 걸리는 것을 옮겼다.

- **Component**: 필드만 있는 struct 또는 class. 메서드는 접근 도우미만 둔다. 이벤트 금지, 다른 객체의 메서드 호출 금지.
- **System**: 필드 없는 `static class`. 인자는 World와 필요한 값뿐이다. 다른 System을 부르지 않으며, 순서는 `ServerLoop.Tick` / `ClientLoop.Update`가 정한다.
- **World 하나뿐인 데이터**(시계, 큐, 목록)는 `ServerWorld`/`ClientWorld`의 필드(Singleton Component)로 둔다.
- **전역 접근 금지**: System, Component, Utility 안에서 `XxxService.Instance`, `XxxLoop.Instance`를 쓰지 않는다. 예외는 두 가지다.
  - Loop가 꺼내 인자로 넘기는 것.
  - Authoring이 `Register/Unregister`를 부르는 것.
- **큰 부수효과는 큐에 적고 한 System이 비운다.** 대상은 데미지, 보상, 디스폰, RPC 전송이다. 적는 쪽은 `world.Xxx.Add(...)`만 한다.
- **쓰기 Utility**는 같은 결과를 내야 하는 곳에서만 부른다. 호출처를 파일 첫 줄 주석에 적는다.
- **MonoBehaviour**는 Loop, Authoring, Network, Proxy 네 종류뿐이다. 시뮬레이션 로직을 갖지 않는다.
- **`transform`은 출력 전용**이다. 위치의 정본은 `MovementState.Position`이다. 예외는 Authoring이 스폰 시 초기 위치를 읽는 것뿐이다.
- **네트워크 콜백**(RPC 처리기, `OnValueChanged`)은 수신함에 기록만 한다.
- **정본은 하나다.** 같은 설정을 두 곳에 연결하지 않고, 같은 상태를 두 곳에 두지 않는다.
- **시간:**
  - 시뮬레이션 dt는 `SimulationClock.DeltaTime`(0.02초)이다.
  - "언제"는 `int` 틱으로 저장한다.
  - 서버 축 틱과 클라 축 틱을 직접 빼지 않는다.
- **이름:** `*System`, `*Util`, `*View`, `*Authoring`, 큐 항목 `*Info`, 네트워크 메시지 `*Payload`/`*Bundle`. private 필드는 `_camelCase`, 상수는 `UPPER_SNAKE`. 시뮬레이션 dt 인자는 `deltaTime`, 렌더 프레임 시간은 `frameTime`.
- **`[SerializeField]` 필드 이름은 바꾸지 않는다.** 바꿔야 하면 `[FormerlySerializedAs]`를 붙인다.
- `UnityEngine.Object`에는 `?.`, `??`, `is null`을 쓰지 않는다. `== null`로 비교한다.
- **디버그 코드**는 `#if UNITY_EDITOR || DEVELOPMENT_BUILD`, 기즈모는 `#if UNITY_EDITOR`로 감싼다.
- **작업이 바꾸는 규칙은 같은 작업에서 `ECS_RULES.md`도 함께 고친다.** 각 작업의 "문서" 항목에 적었다.

---

## 작업 1. 시스템이 컴포넌트 조합이 아니라 "엔티티 종류 목록"을 순회한다

**목적:** 판정·사망 시스템이 "적인지 플레이어인지"를 모르고, 필요한 컴포넌트 묶음만 보게 한다. 강연: 되감기 대상과 피해 대상은 컴포넌트 유무로 정해지고, 서로 독립이다.

**규칙**
1. **편(Team):** `Combat/Components/Team.cs`에 `enum Team : byte { Player, Enemy }`를 둔다. 같은 편끼리는 맞지 않는다. 강연의 적대 판정(FilterBits)에 해당한다.
2. **피격 묶음:** `Combat/Components/HitTarget.cs`에 `readonly struct HitTarget { Team Team; RewindTarget Rewind; Health Health; }`를 둔다. 첫 줄 주석은 `// 튜플: Team + RewindTarget + Health`.
3. **사망 묶음:** `Combat/Components/Mortal.cs`에 `readonly struct Mortal { Health Health; NetworkObject Object; int Exp; int Gold; }`를 둔다.
4. `ServerWorld`에 `List<HitTarget> Hittables`, `List<Mortal> Mortals`를 추가한다. `Despawns`는 `List<NetworkObject>`로 바꾼다.
5. **등록과 해제**는 `ServerLoop.Register`/`Unregister` 안에서, 종류 목록(`Players`, `Enemies`)과 함께 한다. 해제는 같은 `Health` 참조로 찾는다.
   - 적: `Hittables`(Team.Enemy)와 `Mortals`(`Exp`/`Gold`는 `SOEnemy`에서 복사)에 등록한다.
   - 플레이어: `Hittables`(Team.Player)에만 등록한다. `Mortals`에는 넣지 않는다. 플레이어 사망 처리는 이번 브랜치 범위 밖이므로, 그 이유를 등록 코드에 주석으로 남긴다.
6. **플레이어 되감기:**
   - `PlayerServerAuthoring`에 `public readonly RewindTarget Rewind = new();`를 추가한다.
   - `OnNetworkPostSpawn`에서 `History.Clear()`, 그리고 `Radius`/`Height`를 `SOPlayer.Movement.Profile`에서 채운다.
   - `RewindSystem.Record`는 적과 플레이어 모두를 기록한다. 위치 정본을 읽어야 하므로 종류 목록을 순회한다. "기록은 정본을 가진 쪽, 판정은 묶음 목록" 주석을 단다.
7. **투사체:**
   - `Projectile`에 `Team Team`(쏜 쪽 편)을 추가한다. `ServerPlayerSystem.CreateProjectile`은 `Team.Player`를 넣는다.
   - `ProjectileSystem`은 `world.Hittables`만 순회하고, `target.Team == projectile.Team`이면 건너뛴다.
   - 결과는 `out Health`로 받는다. 판정 순서(벽보다 가까운 첫 대상), 거리 감쇠, 크리티컬은 그대로다.
8. **`DeathSystem`**은 `world.Mortals`만 순회하고, `Despawns`에 `NetworkObject`를 넣는다.
   - "디스폰은 틱 끝에 일어나므로 같은 대상이 한 틱에 두 번 들어가지 않는다"를 주석으로 남긴다.
   - `DespawnSystem`은 `obj == null || !obj.IsSpawned`이면 건너뛴다.
9. `ProjectileSystem`, `DeathSystem`, `DespawnSystem`에는 `using Project02.Enemy`와 `using Project02.Player`가 없어야 한다.

**하지 않는 것:** 적의 공격, 플레이어 사망 처리, 피해량·보상 분배 변경.

**문서**
- 1장 Singleton Component에 `Hittables`, `Mortals`를 추가하고 `Despawns`의 타입을 고친다.
- 3장에 규칙 11을 추가한다: "판정·사망·표적 같은 소비 시스템은 컴포넌트 묶음 목록만 본다. 종류 목록(`Players`, `Enemies`)은 정본을 가진 시스템(이동, 되감기 기록, 스냅샷)만 쓴다. 묶음은 struct로 두고 담긴 컴포넌트를 주석에 적는다."

**확인**
- `Grep "Enemies\|Players" Combat/Systems` → `RewindSystem`만 나온다.
- 적 피격, 벽 차단, 지연 보상, 처치 경험치가 그대로 동작한다.
- 플레이어 총알이 다른 플레이어를 맞히지 않는다.

---

## 작업 2. 예측 일치 판정에 허용 오차가 크다 (위치 1m)

**목적:** 서버와 클라의 예측 결과가 실제로 같은지 검증할 수 있게 허용치를 줄이고, 어긋나는 원인을 고친다. 강연: 히트 오예측의 대부분은 위치 불일치였다.

**규칙**
1. `ReconcileSystem`의 허용치를 바꾼다: `POSITION_THRESHOLD = 0.001f`, `ENERGY_THRESHOLD = 0.001f`, `BLOOM_THRESHOLD = 0.001f`. 정수·bool 비교는 그대로 둔다.
   - 서버 상태는 float 그대로 전송되고 클라가 그대로 복원하므로, 결정적이라면 이 허용치에서도 호스트·지연 0의 리컨사일은 0이어야 한다.
2. 리컨사일 로그(디버그 블록)는 위치를 x/y/z 각각의 차이로 출력하게 보강한다.
3. 어긋남이 나오면 **허용치를 다시 키우지 않는다.** 원인을 고칠 때는 서버와 클라가 **같은 함수, 같은 입력, 같은 설정 에셋**을 쓰도록 맞춘다. 한쪽에만 보정을 넣지 않는다. 점검 순서:
   1. `PlayerStepUtil.Step`에 들어가는 값: `InputPayload` 전 필드, `deltaTime`, `PlayerSimulation.Movement`/`Catalog`.
   2. 입력의 틱 번호.
   3. 물리 씬 (작업 12의 검사).
   4. 발밑 판정 (`CharacterPhysics.MoveVertical`, `TryStep`).

**하지 않는 것:** 위치 양자화, 보정을 숨기는 시각적 스무딩.

**확인:** 허용치를 바꾸기 전과 후에 리컨사일 횟수를 비교한다. 조건은 호스트·지연 0 / 지연 200ms + 손실 10% / 지연 200ms에서 경사로·턱 이동이고, 각 1분이다. 호스트·지연 0은 0회여야 한다.

---

## 작업 3. 입력 슬라이딩 윈도우가 "마지막 확인 이후 전부"가 아니라 고정 3개

**목적:** 서버가 받았다고 확인한 다음 틱부터 현재까지의 입력을 매번 보낸다. 강연: 손실 뒤 도착한 첫 패킷이 빈 구간을 전부 메운다.

**규칙**
1. **ack의 뜻은 "이 틱까지의 진짜 입력을 빠짐없이 받았다"이다.** `StatePayload.State.Tick`은 ack로 쓰지 않는다. 서버가 입력을 복제한 틱도 그 번호를 갖기 때문이다.
2. **서버 (`InputQueue.AckTick`, 초기값 -1):**
   - `ServerPlayerSystem.Receive`가 버퍼에 넣은 뒤 `while (Buffer.TryGetValue(AckTick + 1, out _)) AckTick++;`.
   - `AckTick < ConsumedTick - InputQueue.CAPACITY / 2`이면 `AckTick = ConsumedTick`. 영영 오지 않는 틱 때문에 ack가 멈추지 않게 한다.
   - 서버 `Resync`에서 `AckTick = tick - 1`.
3. `StatePayload`에 `int AckTick`를 추가하고 직렬화한다. 값은 `ServerPlayerSystem`이 결과를 기록할 때 채운다.
4. **클라 (`ClientWorld.AckTick`, Singleton Component, 초기값 -1):**
   - `ReconcileSystem.Process`가 payload를 꺼낸 직후, 다른 분기보다 먼저 `Mathf.Max`로 갱신한다.
   - 클라 `Resync`와 `ClientLoop.Register(PlayerOwnerAuthoring)`에서 -1로 되돌린다.
5. **윈도우:** `max(AckTick + 1, 마지막 틱 - MAX_WINDOW + 1)` ~ 마지막 틱, `MAX_WINDOW = 16`(320ms).
   - `InputPayload` 1개 ≈ 41바이트 × 16 ≈ 660바이트로, `SnapshotSystem.MAX_MESSAGE`(1100) 아래다. 이 계산을 상수 옆 주석에 적는다.
6. **`InputBundle`**은 `Count` + payload `Count`개를 직렬화하는 가변 길이로 바꾸고, `CAPACITY = MAX_WINDOW`로 한다.
   - 저장 방식은 `Packages/manifest.json`의 NGO 버전이 RPC 인자로 지원하는 것 중 수신마다 GC 할당이 없는 것을 우선한다.
   - 어떤 방식을 골랐는지 `InputBundle` 첫 줄 주석에 적는다.
7. `ServerPlayerSystem.Receive`의 기존 동작(늦게 온 눌림 살리기, 너무 먼 미래면 재동기화)은 바꾸지 않는다.

**하지 않는 것:** 입력 압축, 전송 시점 변경(작업 4).

**확인:**
- 호스트·지연 0의 리컨사일이 0회다.
- 지연 200ms + 손실 10%에서 리컨사일이 전과 같거나 적다.
- 손실 30%에서 점프가 씹히지 않는다.

---

## 작업 4. 클라이언트 쪽에는 부수효과 큐가 없다 (RPC를 틱 안에서 바로 전송)

**목적:** 입력 RPC를 예측 틱마다 보내지 않고, 틱 루프가 끝난 뒤 한 곳에서 프레임당 한 번 보낸다. 작업 3 다음에 한다.

**규칙**
1. 새 `World/Client/Systems/InputSendSystem.cs`: `static`, 필드 없음, `public static void Flush(ClientWorld world)`.
2. `PredictSystem`에서 전송 코드(`SendInputs`)를 지운다. 입력 기록(`InputHistory`, `SendTimes`)은 `PredictSystem`에 남긴다.
3. `ClientLoop.Update`: 예측 루프 바로 뒤, `WriteTransform` 앞에서 `if (ticks > 0) InputSendSystem.Flush(world);`.
4. 윈도우는 작업 3의 규칙을 따른다. 마지막 틱은 `world.Clock.Tick - 1`이다(루프 뒤에는 이미 1이 올라가 있음).
5. 별도의 전송 큐를 만들지 않는다. "보낼 입력"의 정본은 `InputHistory` + `AckTick`이다(정본은 하나).
6. 실행 순서 `ClientLoop`(-200) → `ServerLoop`(-100)는 바꾸지 않는다. 호스트에서 같은 프레임에 입력이 서버에 닿아야 한다.

**문서**
- 1장 클라 Singleton Component에 `AckTick`을 추가하고, System 예에 `InputSendSystem`을 추가한다.
- 규칙 4에 "클라 입력 전송은 `InputSendSystem` 한 곳, 틱 루프 뒤 프레임당 1회"를 추가한다.

**확인:**
- `Grep "InputRPC"` → `PlayerNetwork`(정의), `InputSendSystem` 두 곳뿐이다.
- 30fps로 제한해도 리컨사일이 늘지 않는다.

---

## 작업 5. Iceberg 경계 너머의 무거운 작업이 틱 안에서 동기로 돈다

**목적:** 적 처치마다 틱 안에서 파일을 쓰던 것을, 메모리 반영 + 10초 주기·종료 시 저장으로 바꾼다. 강연: 빙산의 무거운 일은 ECS 바깥에서 한다.

**규칙**
1. 고치는 곳은 Iceberg 내부(`ProgressService`, `SessionService`)뿐이다. `SessionSystem`과 `ServerLoop`의 호출은 그대로 둔다.
2. **`ProgressService`:**
   - `private bool _dirty;`, `private float _nextSaveTime;`, `private const float SAVE_INTERVAL = 10.0f;`를 둔다.
   - `AddReward`는 값을 더하고 `_dirty = true`만 한다(`Save()` 삭제).
   - `public void Flush()`: `IsOpen && _dirty`일 때 저장한다. 저장에 성공했을 때만 `_dirty = false`로 내린다(`JsonFile.TrySave`의 결과 사용). 실패하면 다음 주기에 다시 시도한다.
   - `Update()`: `Time.unscaledTime >= _nextSaveTime`이면 `Flush()`를 부르고 다음 시각을 정한다.
   - `Close()` 첫 줄과 `OnApplicationQuit()`에서 `Flush()`.
3. `SessionService.OnClientDisconnected`에서 `ProgressService.Instance.Flush()`. Iceberg 안이므로 `Instance` 사용은 허용된다.
4. `Register`와 새 파일 생성의 `Save()`는 그대로 둔다. 드물고, 즉시 남아야 하는 사건이다.

**하지 않는 것:** 비동기(스레드) 저장, `.sav` 스키마 변경.

**문서:** 9장에 "보상은 메모리에 반영하고 10초마다, 그리고 접속 종료·`Close`·종료 시 저장한다. 크래시 시 최대 10초분을 잃는다."를 추가한다.

**확인:**
- 클라 접속 종료, 서버 정상 종료, 싱글에서 메뉴로 나가기 후 경험치가 유지된다.
- 연속 처치 중 `.sav` 수정 시각이 10초 간격으로만 바뀐다.

---

## 작업 6. 유틸리티가 World 전체를 인자로 받는다

**목적:** 적 유틸이 `ServerWorld` 전체가 아니라 필요한 데이터만 받게 한다. 강연: 여러 곳에서 쓰는 유틸은 읽는 것이 적어야 한다. 작업 1 다음에 한다.

**규칙**
1. `World/Server/Components/TargetInfo.cs`에 `readonly struct TargetInfo { ulong ClientId; Vector3 Position; bool IsDead; }`를 둔다.
2. `ServerWorld.Targets`(`List<TargetInfo>`)는 Singleton Component이고, 매 틱 비우고 다시 채운다.
3. 새 `World/Server/Systems/TargetSystem.cs`의 `Collect(ServerWorld world)`가 `Players`에서 채운다. `ServerLoop.Tick`에서 `ServerPlayerSystem.Tick` 다음, `ProjectileSystem.Tick` 앞에 둔다. 적이 이번 틱에 이동한 플레이어 위치를 보는 지금 동작과 같아야 한다.
4. 시그니처를 바꾼다.
   - `EnemyTargetUtil.TryGetTarget(List<TargetInfo> targets, EnemySimulation sim, out Vector3)`
   - `EnemyTargetUtil.TryFindClosest(List<TargetInfo> targets, Vector3 origin, float range, out ulong)`
   - `EnemyFsmUtil.Tick(EnemySimulation sim, int tick, List<TargetInfo> targets)`. private 상태 함수(`TickGeneral`, `TickBattle`)도 `(sim, tick, targets)`로 맞추고, `world.Clock.Tick`은 `tick`으로 바꾼다.
5. `EnemySystem.Tick`이 `world.Clock.Tick`과 `world.Targets`를 넘긴다.
6. 상태 함수는 계속 `private`이고, 전환은 `Change`/`Reset`으로만 한다.

**하지 않는 것:** 표적 선택 규칙(가장 가까운 대상, 범위) 변경.

**문서:** 1장 Singleton Component에 `Targets`를 추가하고, System 예에 `TargetSystem`을 추가한다.

**확인:**
- `Grep "ServerWorld" Enemy/Utilities` → 0건.
- 적이 배회 → 추적 → 범위 이탈 시 배회한다. 플레이어가 둘이면 가까운 쪽을 쫓는다.

---

## 작업 7. 발사 RPC가 Unreliable

**목적:** "사건(발사)은 Reliable, 틱마다 가는 스트림은 Unreliable" 규칙을 지킨다.

**규칙**
1. `PlayerNetwork.FireRPC`를 `[Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]`로 바꾼다.
2. `MAX_FIRES` 상한과 "가장 오래된 것 버림"은 유지한다. 스냅샷이 끊겨 `RenderTick`이 멈춰도 무한히 쌓이지 않게 하는 장치다.
3. 늦게 도착한 발사는 `RemotePlayerViewSystem.PresentFires`에서 즉시 재생된다. 이 동작을 `FireRPC` 주석에 적는다.

**확인:** 클라 2개, 지연 200ms + 손실 10%에서 A가 쏜 발 수와 B 화면의 탄도 수가 같다.

---

## 작업 8. 스폰은 규칙 4의 큐 대상인데 즉시 처리된다

**목적:** 강연대로 스폰은 즉시 한다(생성 지연은 한 틱 어긋남을 만든다). 코드가 아니라 규칙을 고친다. 대신 위험한 경우인 "틱 도중 등록"을 막는다.

**규칙**
1. `ServerLoop`:
   - `private bool _ticking;`을 두고, `Update`의 루프에서 `Tick(...)` 호출 전후로 켜고 끈다.
   - 두 `Register` 첫 줄에 `Debug.Assert(!_ticking, "틱 도중 등록 금지: 순회 중인 목록이 바뀐다 (ECS_RULES 규칙 4)");`.
2. `Unregister`에는 넣지 않는다. 틱 마지막 단계인 `DespawnSystem`의 `Despawn()`이 `OnNetworkDespawn` → `Unregister`를 부르는 것이 정상 흐름이므로, 이 이유를 주석으로 남긴다.
3. `ClientLoop`는 Register가 틱 루프 밖(NGO 콜백)에서만 일어나므로 가드를 두지 않는다.

**하지 않는 것:** 스폰을 큐로 바꾸는 것.

**문서:** 규칙 4의 대상에서 "스폰"을 빼고 아래 문단을 넣는다.
> 스폰은 즉시 한다 (강연: 생성 지연은 한 틱 어긋남을 만든다). 단 `ServerLoop` 틱 도중에는 스폰하지 않는다. `Register`가 순회 중인 목록을 바꾸기 때문이다. 틱 안에서 스폰이 필요해지면 `world.Spawns` 큐를 만들어 `DespawnSystem` 앞에서 처리한다.

**확인:** 호스트·클라 플레이에서 Assert가 뜨지 않는다.

---

## 작업 9. 체력의 쓰기 지점이 스폰 시점에 두 곳이다

**목적:** 스폰 시 `Health.Max`/`Current`를 쓰는 곳을 엔티티마다 한 곳으로 줄이고, 적 최대 체력의 정본을 SO로 옮긴다.

**규칙**
1. `SOEnemy`에 `public int MaxHp;`를 추가한다. 기존 값은 Turtle Shell 프리팹의 `Health._defaultMax: 10`이고, 이 값을 해당 `SOEnemy` 에셋에 옮긴다.
2. **`Health`:**
   - `OnNetworkSpawn`에서 `Max.Value`/`Current.Value` 쓰기를 지운다.
   - 로컬 초기화(`DamageReduction`, `Contributors.Clear()`)는 남긴다.
   - `[SerializeField] _defaultMax`를 지운다.
3. **적:** `EnemyServerAuthoring.OnNetworkSpawn`(서버만)에서 `Max`와 `Current`를 `_settings.MaxHp`로 채운다. "`OnNetworkSpawn`에서 쓴 값은 스폰 메시지에 실린다"를 주석으로 남긴다. 시뮬레이션 초기화는 `OnNetworkPostSpawn`에 둔다.
4. **플레이어:** `PlayerStatUtil.ApplyHealth`와 `AppliedLevel = level`을 `PlayerServerAuthoring.OnNetworkSpawn`의 `ServerLoop.Instance.Register(this)` 바로 뒤로 옮긴다. `Register` 안의 `SessionSystem.Join`이 `Level`을 정하므로 그 뒤여야 한다. `PlayerSpawnUtil.Initialize`는 `OnNetworkPostSpawn`에 둔다.
5. 끝난 뒤 `Max.Value`/`Current.Value`를 쓰는 곳은 아래뿐이다.
   - 두 Authoring의 `OnNetworkSpawn`
   - `DamageSystem`
   - `PlayerStatUtil.ApplyHealth`

**문서**
- 규칙 5의 스폰 초기화 위치를 "Authoring의 `OnNetworkSpawn` 또는 `OnNetworkPostSpawn`"으로 고친다.
- 1장 겸용 설명의 `Health`를 "Component + NetworkVariable. 값 초기화는 Authoring"으로 고친다.

**확인:**
- `Grep "Max.Value\|Current.Value"`의 쓰기 지점이 위 목록과 같다.
- 적 스폰 직후 클라에서 체력이 0으로 보이는 프레임이 없다.
- 플레이어 체력이 레벨 테이블 값이다.

---

## 작업 10. 히트 이펙트를 클라에서 예측하지 않는다

**목적:** 강연처럼 **피격 이펙트만** 클라에서 예측한다. 피해, 체력, 경험치는 서버 결과를 기다린다. 오예측(이펙트는 났는데 피해 없음)은 되돌리지 않는다. 작업 1, 11 다음에 한다.

**규칙**
1. **판정은 서버와 같은 함수로 한다.**
   - `ProjectileSystem`의 1틱 처리(벽 레이캐스트 → 벽보다 가까운 첫 캡슐 → 전진)를 읽기 Utility `Combat/Utilities/ProjectileStepUtil.cs`로 뺀다. 선분–캡슐 교차도 여기로 옮긴다.
   - 대상 위치는 호출하는 쪽이 샘플해서 `List<CapsuleSample>`(`Vector3 Position; float Radius; float Height; int Index`)로 넘긴다. 서버는 `RewindUtil`로, 클라는 `SnapshotUtil`로 샘플한다.
   - 이 추출은 서버 동작을 바꾸지 않아야 한다. 샘플 목록은 World의 재사용 버퍼(Singleton Component)로 두어 매 틱 할당하지 않는다.
2. **벽 레이어의 정본은 하나:**
   - `ServerLoop._projectileHitLayer`를 `SOBullet.HitLayer`로 옮긴다.
   - `Projectile`이 생성 시 `HitLayer`를 복사하고, `ProjectileSystem`의 `hitLayer` 인자를 지운다.
   - 기존 값(지형·장애물)은 `SOBullet` 에셋에 다시 넣어야 한다.
3. **클라 피격 묶음:**
   - `ClientWorld.HitTargets`(`List<ClientHitTarget>`, 튜플: `TickBuffer<EnemySnapshot>` + Radius + Height)를 둔다.
   - `EnemyRemoteAuthoring`이 `SOEnemy.Profile`에서 반경과 높이를 읽어 `ClientLoop.Register`로 등록한다. 이를 위해 `EnemyRemoteAuthoring`에 `[SerializeField] SOEnemy _settings`를 추가한다(Authoring → Data 참조는 허용).
   - 플레이어 총알은 플레이어를 맞히지 않으므로 원격 플레이어는 넣지 않는다.
4. **예측 탄의 상태는 World에 둔다:**
   - `ClientWorld.PredictedShots`(`List<Projectile>`).
   - `PredictSystem`이 **처음 예측할 때** 발사에 성공하면 추가한다. 원점·방향 계산은 서버와 같은 `WeaponFireUtil.GetOrigin`/`GetDirection`이다.
   - `ReconcileSystem`의 재시뮬레이션에서는 추가하지 않는다.
5. **예측 탄의 진행:** 새 `World/Client/Systems/ShotPredictSystem.cs`가 그 프레임의 예측 틱 수만큼 `ProjectileStepUtil`로 전진·판정한다.
   - 적 위치는 그 시점의 `world.Interp.RenderTick`(작업 11의 외삽 중이면 그린 시점)에 `SnapshotUtil`로 샘플한다. 쏜 사람이 본 화면과 서버의 되감기 시점이 같아지게 하려는 것이다.
   - `ClientLoop`에서 예측 루프와 `InputSendSystem` 사이에 둔다.
6. **연출은 미뤄서 한 곳에서:**
   - `ShotPredictSystem`은 맞은 지점과 종류(벽/적)를 `ClientWorld.PredictedImpacts`(`List<ImpactInfo>`)에 적기만 한다.
   - 실제 생성은 `LateUpdate` 단계의 View System이 `WeaponView` 경유로 `ProjectileVisualService`에 넘긴다. `ProjectileVisualService`는 `WeaponView`에서만 부른다는 기존 경계를 지킨다.
   - 이펙트 프리팹은 `SOBullet.ImpactPrefab`이고, 비어 있으면 아무것도 하지 않는다.
7. 작업 11의 외삽 중에는 **적 피격 이펙트를 예측하지 않는다.** 벽 피격은 예측한다(정지 물체라 오예측이 없음).
8. 클라에서 `Health`, 경험치, 히트 표시를 바꾸지 않는다.

**하지 않는 것:** 원격 플레이어 발사(`RemoteFire`)의 피격 예측, 서버 판정 규칙 변경.

**문서:** 1장 클라 Singleton Component에 `HitTargets`, `PredictedShots`, `PredictedImpacts`를 추가하고, System 예에 `ShotPredictSystem`을, 읽기 Utility에 `ProjectileStepUtil`을 추가한다.

**확인:**
- 서버 판정 결과(적 피격, 벽 차단, 지연 보상)가 추출 전과 같다.
- 지연 150ms에서 이펙트는 즉시 나고 체력 감소는 왕복 지연 뒤에 일어난다.
- 벽 뒤 적에게는 이펙트가 나지 않는다.

---

## 작업 11. 되감기 상한이 500ms(25틱)이고 외삽이 없다

**목적:** 되감기 상한을 12틱으로 두고, 상한을 넘는 지연의 클라는 적을 외삽해서 그린다. 그래서 겨눈 곳과 서버 판정 시점이 맞게 한다. 강연: RTT 약 220ms부터 되감기를 제한하고 외삽한다.

**규칙**
1. **상한 상수는 한 곳에만 둔다:** `Combat/Components/RewindTarget.cs`에 `public const int MAX_LAG_TICKS = 12;`.
   - `ServerPlayerSystem.MAX_LAG_TICKS`를 지우고 이 상수를 쓴다.
   - `CAPACITY`(64)가 상한의 2배 이상임을 주석에 적는다.
2. **클라의 예상 되감기 양**은 읽기 Utility `World/Client/Utilities/LagUtil.cs`의 함수 하나에 둔다.
   - `예상 lag = (Interp.LatestTick − Interp.RenderTick) + Clock.RttTicks + Clock.SmoothedBuffer`
   - 근거: 서버가 입력을 소비하는 틱 ≈ 지금 받은 최신 스냅샷 틱 + 왕복 시간 + 서버 입력 버퍼.
   - 위의 값들은 모두 "틱 수"이므로 서버 축과 클라 축을 직접 빼지 않는다는 규칙을 지킨다. `RttTicks < 0`(측정 전)이면 외삽하지 않는다.
3. `초과 = clamp(예상 lag − MAX_LAG_TICKS, 0, MAX_EXTRAPOLATION_TICKS)`, `MAX_EXTRAPOLATION_TICKS = 10`.
4. **외삽:**
   - `SnapshotUtil` 옆에 읽기 Utility를 둔다. 마지막 두 스냅샷의 위치 차이로 속도를 구하고, `RenderTick + 초과` 시점의 위치를 추정한다.
   - 적용 대상은 적(`EnemyViewSystem`)뿐이다. 원격 플레이어는 판정 대상이 아니므로 외삽하지 않는다.
5. **`ClientWorld`에 그린 시점을 둔다:** `float ViewTick` = `RenderTick + 초과`. 매 프레임 한 System이 계산한다.
   - `PredictSystem`은 `InputPayload.ViewTick`에 이 값을 넣는다.
   - `EnemyViewSystem`과 `ShotPredictSystem`(작업 10)도 이 값을 쓴다. 같은 값을 여러 곳에서 다시 계산하지 않는다(정본은 하나).
6. 서버는 기존대로 `ViewTick`을 `[1, MAX_LAG_TICKS]`로 클램프한다. 클라 값을 그대로 믿지 않는다.
7. `초과 > 0`인 동안 작업 10의 적 피격 예측을 끈다. 이 판단도 같은 Utility의 결과를 쓴다.
8. 디버그 표시(`ClientLoop.OnGUI`)에 예상 lag와 초과 틱을 추가한다.

**하지 않는 것:** 상한 이하 지연에서의 동작 변경.

**문서:** 4장 시간축 표 아래에 "되감기 상한과 외삽" 단락을 추가한다. 내용은 상수 이름·값, 예상 lag 식과 근거, 외삽 시 `ViewTick`의 의미다.

**확인:**
- 지연 100ms: 초과 0, 기존 지연 보상 확인을 통과한다.
- 지연 400ms: 초과 > 0, 일정하게 옆으로 걷는 적의 몸 중앙을 겨누면 맞는다. 방향을 자주 바꾸는 적은 빗나갈 수 있다(의도된 동작).

---

## 작업 12. 이동 판정이 Unity 물리 씬을 읽는다

**목적:** 이동 판정(`CharacterMotor` → `Physics.CapsuleCast`)이 서버와 클라에서 같은 결과를 내려면, 대상 레이어(Ground, Obstacle)에 움직이지 않는 콜라이더만 있어야 한다. 이 전제를 규칙과 검사 코드로 고정한다. 이동 코드는 바꾸지 않는다.

**규칙**
1. `CharacterProfile`에 `#if UNITY_EDITOR`의 `OnValidate`를 추가한다. `GroundLayer | ObstacleLayer`에 `Player` 또는 `Enemy` 레이어가 있으면 `Debug.LogWarning(..., this)`. 레이어 번호는 `LayerMask.NameToLayer`로 얻는다(숫자 하드코딩 금지).
2. 새 읽기 Utility `Character/PhysicsSceneCheckUtil.cs`(`#if UNITY_EDITOR || DEVELOPMENT_BUILD`):
   - 주어진 레이어 마스크의 콜라이더 중 kinematic이 아닌 `Rigidbody`가 붙은 것을 찾아 경고한다.
3. 검사는 씬마다 1회, 시뮬레이션이 시작될 때 Loop가 부른다.
   - 서버: `ServerLoop`에서 첫 `Register(PlayerServerAuthoring)` 때.
   - 클라: `ClientLoop.Register(PlayerOwnerAuthoring)` 때.
   - 레이어 마스크는 등록되는 플레이어의 `SOPlayer.Movement.Profile`에서 얻는다. 매 프레임 돌리지 않는다.
4. `CharacterMotor`, `CharacterPhysics`는 Iceberg(물리 유틸)이므로 고치지 않는다.

**하지 않는 것:** 움직이는 물체 지원, 자체 충돌 구현.

**문서**
- 규칙 7 아래에 "Ground/Obstacle 레이어에는 움직이지 않는 콜라이더만 둔다. 문·발판처럼 움직이는 물체를 넣으려면 그 상태를 시뮬레이션 상태에 넣고 되감기 대상으로 만들어야 한다."를 추가한다.

**확인:**
- 현재 씬에서 경고가 없다.
- Profile의 ObstacleLayer에 Enemy를 켜면 경고가 난다.
- Obstacle 레이어 큐브에 일반 Rigidbody를 붙이면 경고가 난다(확인 후 원래대로 되돌린다).

---

## 진행 순서

| 순서 | 작업 | 이유 |
|---|---|---|
| 1 | 8 | 이후 리팩터링에서 틱 도중 등록 실수를 잡는 가드 |
| 2 | 12 | 작업 2의 측정 전에 물리 차이 요인을 막음 |
| 3 | 7 | 작고 독립적 |
| 4 | 9 | 독립적 |
| 5 | 5 | 독립적 |
| 6 | 3 | 작업 4의 전제 |
| 7 | 4 | 윈도우가 늘기 전에 전송을 줄이면 입력이 빠짐 |
| 8 | 2 | 손실·물리 요인을 없앤 뒤 재야 원인을 가를 수 있음 |
| 9 | 1 | 가장 큰 구조 변경 |
| 10 | 6 | 작업 1의 구조 위에서 |
| 11 | 11 | 위치 일치(작업 2)가 확보된 뒤 |
| 12 | 10 | 작업 1의 판정 구조와 작업 11의 `ViewTick`을 씀 |
