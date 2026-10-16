# ECS 규칙 (Project02)

Unity Entities(DOTS)를 쓰지 않고, NGO GameObject 구조 위에 ECS의 "규칙"만 적용한다.
목적은 시뮬레이션(물리)과 연출의 분리, 그리고 부수효과의 위치를 한 곳으로 모으는 것.
참고: Overwatch Gameplay Architecture and Netcode (GDC 2017, Tim Ford)

---

## 1. 용어

| 용어 | 이 프로젝트에서 | 예 |
|---|---|---|
| World | 한 쪽(서버/클라)의 모든 상태를 가진 데이터 class | `ServerWorld`, `ClientWorld` |
| Loop | World를 소유하고 System을 정해진 순서로 돌리는 진입점 MonoBehaviour | `ServerLoop`, `ClientLoop` |
| Entity | NetworkObject 하나. 식별자는 `NetworkObjectId` (플레이어는 `OwnerClientId`도 사용) | 플레이어, 적 |
| Component | 데이터 덩어리. struct 또는 필드만 있는 class | `PlayerSimulation`, `EnemySimulation`, `InputQueue`, `RewindTarget`, `Health`(Component + 동기화 Proxy + 스폰 초기화 겸용) |
| Singleton Component | World에 하나뿐인 데이터 | 서버: `Clock`, `Hits`, `Projectiles`, `Rewards`, `Despawns`, `Outbox` / 클라: `Clock`, `Interp`, `Input`, `Camera` |
| System | 필드 없는 static class. Loop가 정해진 순서로 호출 | `ServerPlayerSystem`, `DamageSystem`, `PredictSystem`, `EnemyViewSystem` |
| Utility (읽기) | 숨은 상태 없이 인자로 받은 것만 다루는 함수. `ref`로 받은 값만 바꾸는 것도 포함. 어디서 불러도 됨 | `CharacterMotor`, `PlayerMovement`, `WeaponFire`, `RewindUtil`, `SnapshotUtil`, `EnemyTargetUtil` |
| Utility (쓰기) | Component 객체를 직접 받아 바꾸는 함수. 호출처 제한 (규칙 5) | `PlayerStepUtil`, `PlayerStatUtil`, `PlayerEquipUtil`, `EnemyFsm`, `EnemyNavigation` |
| Authoring | NGO 생명주기에서 엔티티를 World에 등록/해제하고 Component를 초기화하는 MonoBehaviour | `PlayerServer`, `PlayerOwner`, `PlayerRemote`, `EnemyController`, `EnemyRemote` |
| Proxy | 엔진 기능(Animator, Cinemachine, IK, VFX, NetworkVariable)에 값을 전달만 하는 MonoBehaviour | `PlayerVisual`, `PlayerAnimation`, `PlayerCamera`, `WeaponView` |
| Iceberg | ECS 밖의 큰 하위 시스템. 경계만 정하고 내부는 건드리지 않음 | NGO, `PathGrid`, UI, `SessionService`, `ProgressService`, `InputService` |

- 겸용 클래스는 데이터 필드 + NGO 생명주기 초기화 + NetworkVariable까지만 허용한다. 로직 메서드가 생기면 System으로 뺀다.

---

## 2. 규칙

1. **Component는 데이터만.** struct 또는 필드만 있는 class. 메서드는 접근 도우미만 허용
   (예: `TickBuffer.TryGetValue`, `Health.IsDead`). 이벤트 금지. 다른 객체의 메서드 호출 금지.
2. **System은 static class.** 필드 없음. 인자는 World와 필요한 값뿐.
   다른 System을 직접 호출하지 않는다. 순서는 Loop가 정한다.
3. **전역 접근 금지.** `XxxService.Instance`, `XxxLoop.Instance`를 System, Component, Utility 안에서 부르지 않는다.
   예외는 두 가지:
   - Loop가 Iceberg에서 값을 꺼내 System 인자로 넘긴다 (`SessionService.Instance`, `InputService.Instance`)
   - Authoring이 NGO 생명주기에서 `XxxLoop.Instance.Register/Unregister`를 부른다
4. **큰 부수효과는 큐에 쌓고 한 곳에서 처리한다.** 스폰, 디스폰, 데미지, 보상, RPC 전송.
   쌓는 쪽은 기록만 하고, 처리하는 System은 하나뿐이다.
   - 서버 송신: `NetSendSystem`(RPC), `SnapshotSystem`(스냅샷) / 디스폰: `DespawnSystem` (`ServerLoop.Tick` 맨 끝)
   - 클라 송신: `PredictSystem`의 입력 전송 한 곳 (예측 틱마다 즉시. 별도 Outbox 없음)
5. **쓰기 Utility는 호출처를 제한한다.** 같은 결과를 내야 하는 곳만 부른다.
   - `PlayerStepUtil.Step`: `ServerPlayerSystem`, `PredictSystem`, `ReconcileSystem` (같은 틱 → 같은 결과)
   - `PlayerStepUtil.WriteTransform`: `ServerPlayerSystem`, `ClientLoop` (틱 루프가 끝난 뒤 1회)
   - 그 밖의 쓰기 Utility: 해당 System + 스폰 초기화 1회 (Authoring의 `OnNetworkPostSpawn`, 또는 World 틱이 필요하면 Loop의 `Register`)
6. **MonoBehaviour는 Loop, Authoring, Proxy 세 종류만.** 시뮬레이션 로직을 갖지 않는다.
7. **`transform`은 시뮬레이션의 출력 전용.** 시뮬레이션은 `transform`을 읽지 않는다.
   위치의 정본은 `MovementState.Position`. transform 쓰기는 틱 루프가 끝난 뒤 한 번
   (`PlayerStepUtil.WriteTransform`, `EnemySystem` 끝, `*ViewSystem`).
   예외: Authoring이 스폰 시 초기 위치를 읽는 것.
8. **네트워크 콜백(RPC, `NetworkVariable.OnValueChanged`)은 기록만 한다.**
   시뮬레이션 상태를 바꾸지 않는다. 바뀐 값은 System이 틱 안에서 질의한다
   (예: `StatSystem`의 `AppliedLevel != Level`).
   예측 대상 상태가 바뀌어야 하면 서버 상태(`*State`)에 넣어 `StatePayload`로 보내고 재조정에 맡긴다
   (예: `StaminaState.MaxEnergy`). 연출만 바꾸는 콜백(무기 모델 생성 등)은 허용.

---

## 3. 시간

| 값 | 단위 | 용도 |
|---|---|---|
| `SimulationClock.DeltaTime` | 초 (0.02, 50Hz) | 1틱의 길이. 시뮬레이션 dt는 항상 이것 |
| `frameTime` (`Time.deltaTime`) | 초 | 렌더 프레임 길이. 시계 누적과 연출에만 |
| `*Tick` (int) | 틱 | "언제": 연사 간격, 쿨다운, 상태 진입 시점, 버퍼 지연 |
| 초당 비율 × dt | | "얼마나 빨리": 속도, 회복량 |

- 초 → 틱: `÷ SimulationClock.DeltaTime`, 틱 → 초: `× SimulationClock.DeltaTime`
- 시계는 World가 가진다. Unity `FixedUpdate`와 `Time.fixedTime`은 쓰지 않는다.
- "언제"를 `float` 초 누적으로 저장하지 않는다. 시작 틱을 저장하고 `현재 틱 − 시작 틱`으로 계산한다.

### 시간축

| 축 | 번호 | 가진 곳 |
|---|---|---|
| 서버 축 | 서버 틱 | `ServerWorld.Clock`, 스냅샷 틱, `RenderTick`, `RemoteFire.Tick`, `RewindTarget.History` |
| 클라 축 | 클라 예측 틱 | `ClientWorld.Clock`, `InputPayload.Tick`, `InputHistory`, `StateHistory` |

두 축을 잇는 다리는 둘이다. 그 밖에서는 섞지 않는다.
- `InputPayload.ViewTick` (서버 축): 클라가 보던 원격 시점을 서버에 알린다 → 지연 보상
- `InputPayload.Tick` (클라 축): 서버가 이 번호를 보존해 `StatePayload.State.Tick`으로 돌려준다 → 재조정

---

## 4. 이름

| 형태 | 의미 |
|---|---|
| `*World` | World (데이터) |
| `*Loop` | Loop (진입점) |
| `*Simulation` | 엔티티의 시뮬레이션 데이터 Component |
| `*System` | System |
| `*Util` | Utility. 단, 도메인 이름이 더 분명한 경우는 예외 (`PlayerMovement`, `WeaponFire`, `EnemyFsm`, `EnemyNavigation`, `CharacterMotor`) |
| `*State` (struct) | 롤백/스냅샷 대상 시뮬레이션 상태 |
| `*Payload`, `*Bundle`, `*Snapshot`, `*Message` (struct) | 네트워크 메시지 |
| `SO*` | ScriptableObject 전부 |
| `*View`, `*Visual` | 연출 Proxy |
| `Step` | 데이터만 바꾸는 1틱 진행 (Unity 객체 부수효과 없음) |
| `Tick` | System이나 Loop가 World를 1틱 돌림 |
| `Restore` | 상태 복원 (재조정) |
| `Process` | 큐를 비우며 처리 |
| `Record` | 버퍼에 기록만 |
| `Present` | LateUpdate 연출 |

- 필드: private `_camelCase`, Component 데이터와 Authoring의 형제 참조 `public PascalCase`, 상수 `UPPER_SNAKE`
- 시뮬레이션 dt 인자는 `deltaTime`, 렌더 프레임 시간 인자는 `frameTime`
- RPC 메서드는 `*RPC`. 틱마다 가는 스트림은 Unreliable, 사건(발사)은 Reliable
- 큐 기록은 `world.Xxx.Add(...)`로 직접 (별도 Enqueue 메서드 없음)
- 디버그 코드는 `#if UNITY_EDITOR || DEVELOPMENT_BUILD`
- `UnityEngine.Object`에는 `?.`, `??`, `is null` 금지. `== null`로 비교

---

## 5. 이 코드는 어디에 두나 (판단 순서)

1. 엔진 기능에 값을 넣기만 하나? → **Proxy**
2. NGO 생명주기에서 등록/초기화만 하나? → **Authoring**
3. 틱마다 바뀌고 롤백/스냅샷 대상인가? → **`*State`** (`*Simulation` Component 안)
4. World에 하나뿐인가? (시계, 큐, 표) → **Singleton Component** (`*World` 필드)
5. 입력 → 출력만 있는 계산인가? → **읽기 Utility**
6. 여러 엔티티의 Component를 읽고 쓰는 행동인가? → **System**
7. 위 어디에도 안 맞고 크기가 큰가? → **Iceberg**. 경계(진입 함수 하나)만 정한다

---

## 6. 바꾸지 않는 것 (Iceberg)

- UI: `Presenter/*`, `View/*` (MVP 유지), `SettingsService`
- 입력 장치: `InputService`, `PlayerAction`
- 길찾기: `PathGrid`, `PathFinder`
- 물리 유틸: `CharacterPhysics`, `CharacterMotor`
- 접속/부팅/저장: `ConnectionService`, `SessionService`, `ProgressService`, `ServerLauncher`, `Bootstrapper` → `SessionService`는 `SessionSystem` 한 곳에서만 부른다 (Loop가 인자로 넘김)
- 연출: `ProjectileVisualService` → `WeaponView`(Proxy)에서만 부른다

---

## 7. 과도기 운영

- 한 번에 한 경로만 바꾼다. 한 단계가 끝나기 전에 다음 단계를 시작하지 않는다.
- 옛 코드는 바로 지우지 않고 `[System.Obsolete]`를 붙인다. 컴파일러 경고가 남은 작업 목록이다. 해당 단계가 끝나면 삭제한다.
- 단계마다 회귀 확인을 같은 조건으로 한다.
   - 지연 0: 리컨사일 0회 (레벨업 순간의 1회는 의도된 것이라 제외)
   - 지연 200ms + 손실 10%: 1분간 리컨사일 횟수를 단계 전후로 비교
   - 적을 쏘면 피해가 들어가고, 죽으면 경험치가 오른다. 벽 뒤 적은 안 맞는다
   - 지연 200ms에서 옆으로 움직이는 적의 몸 중앙을 겨눠 쏘면 맞는다 (지연 보상)
   - 클라 2개: 상대 캐릭터와 발사 연출이 부드럽고, 몸보다 총알이 먼저 나가지 않는다
   - 적 35마리 이상: 적과 원격 플레이어 보간이 끊기지 않는다 (스냅샷 분할)
   - 무기 교체(`EquippedWeapon` 변경): 소유자 리컨사일 정확히 1회, 이후 새 연사 간격·줌 적용, 추가 리컨사일 없음
   - 클라에서 적 처치 반복 후 `ServerLoop`의 `Enemies` 수가 0 (클라는 서버 World에 등록하지 않음)
   - 진행도는 세계 단위 `.sav` 하나에 저장한다. 싱글은 슬롯마다, 전용 서버는 하나. 스키마는 같다.
   - 저장 파일은 여는 쪽(ServerLauncher / StartSinglePlayer)이 명시적으로 연다. 읽기 실패한 파일은 덮어쓰지 않는다.
   - 클라 일시정지(또는 큰 히치) 후 재개: `clock resync` 1회 + 재조정 최대 1회, 이후 입력 정상 반영, buffer가 목표(3) 근처로 복귀