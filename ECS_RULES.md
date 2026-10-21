# ECS 규칙 (Project02)

Unity Entities(DOTS)를 쓰지 않고, NGO GameObject 구조 위에 ECS의 "규칙"만 적용한다.

목적은 시뮬레이션(물리)과 연출의 분리, 그리고 부수효과의 위치를 한 곳으로 모으는 것.

이 문서만 읽어도 규칙을 따를 수 있게 썼다. 

용어 일부는 아래 강연에서 빌려 왔고, 뜻은 0장에 풀어 두었다.

참고: [Overwatch Gameplay Architecture and Netcode (GDC 2017, Tim Ford)](https://gdcvault.com/play/1024001/-Overwatch-Gameplay-Architecture-and)

---

## 0. 왜 이렇게 하나

예전 구조는 `PlayerController` 하나가 입력, 이동, 발사, 연출, 네트워크를 모두 했다.
그래서 서버와 클라가 같은 코드를 다른 조건으로 돌리다 결과가 어긋났고(Reconcile),
"이 값을 누가 바꾸나"를 찾으려면 클래스 전체를 읽어야 했다.
아래 다섯 가지 생각으로 이것을 나눈다.

| 생각 | 뜻 | 이 프로젝트에서 |
|---|---|---|
| 데이터와 로직을 나눈다 | 상태는 필드만 있는 객체(Component)에, 동작은 필드 없는 함수 모음(System)에 둔다. 같은 상태에 같은 함수를 돌리면 같은 결과가 나온다 | 서버와 클라가 같은 `PlayerStepUtil.Step`을 돌려 예측이 맞는다 |
| 하나뿐인 데이터는 World에 둔다 (Singleton Component) | 시계, 입력, 처리 대기 목록처럼 엔티티마다가 아니라 세상에 하나인 값. 전역 변수 대신 World의 필드로 두고 System에 인자로 넘긴다 | `world.Clock`, `world.Hits` |
| 큰 부수효과는 미뤄서 한 곳에서 한다 | 데미지·스폰·전송을 그 자리에서 하지 않고 목록에 적어 두면, 실제 처리는 System 하나가 정해진 순서로 한다. "언제, 어디서 일어나나"가 한 곳에 모인다 | 투사체는 `world.Hits.Add(...)`만 하고, `DamageSystem`이 처리한다 |
| 공용 계산은 Utility로 뺀다 | 여러 System이 같은 계산을 쓰면 인자만 받는 함수로 빼서 공유한다 | `WeaponFireUtil.GetDirection`을 서버 판정과 연출이 같이 쓴다 |
| 큰 기존 시스템은 건드리지 않는다 (Iceberg, 빙산) | 물 위로는 작은 입구만 보이고 아래는 큰 덩어리라는 비유. 그 내부는 ECS 규칙을 적용하지 않고, ECS 쪽에서 부르는 입구만 정한다 | 저장(`SessionService`)은 `SessionSystem` 한 곳에서만 부른다 |

## 1. 용어

| 용어 | 이 프로젝트에서 | 예 |
|---|---|---|
| Iceberg | ECS 규칙을 적용하지 않는 큰 기존 시스템. ECS 쪽에서 부르는 입구(함수 하나 또는 System 하나)만 정한다 (0장) | NGO, `PathGrid`, UI, `SessionService`, `ProgressService`, `InputService` |
| World | 한 쪽(서버/클라)의 모든 상태를 가진 데이터 class | `ServerWorld`, `ClientWorld` |
| Loop | World를 소유하고 System을 정해진 순서로 돌리는 진입점 MonoBehaviour | `ServerLoop`, `ClientLoop` |
| Entity | NetworkObject 하나. 식별자는 `NetworkObjectId` (플레이어는 `OwnerClientId`도 사용) | 플레이어, 적 |
| Component | 데이터 덩어리. struct 또는 필드만 있는 class | `PlayerSimulation`, `EnemySimulation`, `InputQueue`, `RewindTarget`, `HitTarget`·`Mortal`(묶음, 규칙 11) |
| Singleton Component | World에 하나뿐인 데이터 (`*World`의 필드) | 서버: `Clock`, `Projectiles`(`List<Projectile>`), `Hits`(`List<HitInfo>`), `Rewards`(`List<RewardInfo>`), `Despawns`(`List<NetworkObject>`), `Hittables`(`List<HitTarget>`), `Mortals`(`List<Mortal>`), `Outbox` / 클라: `Clock`, `Interp`, `Input`(`InputState`), `Camera`(`CameraState`), `AckTick`(`int`, 서버가 빠짐없이 받은 마지막 입력 틱) |
| System | 필드 없는 static class. Loop가 정해진 순서로 호출 | `ServerPlayerSystem`, `DamageSystem`, `PredictSystem`, `InputSendSystem`, `EnemyViewSystem` |
| Utility (읽기) | 숨은 상태 없이 인자로 받은 것만 다루는 함수. `ref`로 받은 값만 바꾸는 것도 포함. 어디서 불러도 됨 | `CharacterMotor`, `PlayerMoveUtil`, `WeaponFireUtil`, `RewindUtil`, `SnapshotUtil`, `EnemyTargetUtil` |
| Utility (쓰기) | Component 객체를 직접 받아 바꾸는 함수. 호출처 제한 (규칙 5) | `PlayerStepUtil`, `PlayerStatUtil`, `PlayerEquipUtil`, `PlayerSpawnUtil`, `EnemyFsmUtil`, `EnemyNavigationUtil` |
| Authoring | NGO 생명주기에서 엔티티를 World에 등록/해제하고 Component를 초기화하는 MonoBehaviour | `PlayerServerAuthoring`, `PlayerOwnerAuthoring`, `PlayerRemoteAuthoring`, `EnemyServerAuthoring`, `EnemyRemoteAuthoring` |
| Network | 엔티티당 하나. 그 엔티티의 RPC·NetworkVariable과 수신함만 가진다 (규칙 9) | `PlayerNetwork` |
| Proxy | 엔진 기능(Animator, Cinemachine, IK, VFX)에 값을 전달만 하는 MonoBehaviour | `PlayerView`, `PlayerBodyView`, `PlayerAnimatorView`, `PlayerCameraView`, `PlayerWeaponView`, `WeaponView`, `BulletView` |
| Data | 설정 ScriptableObject. 런타임에 바꾸지 않는다 | `SOPlayer`, `SOMovement`, `SOLevelTable`, `SOEnemy`, `SOWeapon`, `SOCatalog` |
| Iceberg | ECS 밖의 큰 하위 시스템. 경계만 정하고 내부는 건드리지 않음 | NGO, `PathGrid`, UI, `SessionService`, `ProgressService`, `InputService` |

- 겸용 클래스는 데이터 필드 + NGO 생명주기 초기화 + NetworkVariable까지만 허용한다. 로직 메서드가 생기면 System으로 뺀다.
  현재 겸용: `Health` (Component + NetworkVariable + 스폰 초기화)

---

## 2. 폴더와 네임스페이스

```
_Scripts/<도메인>/<역할>/파일.cs
```

| 역할 폴더 | 담는 것 |
|---|---|
| `Authoring/` | Authoring, 씬에 놓이는 스포너 (`PlayerSpawner`, `EnemySpawner`) |
| `Components/` | Component, `*State`, 큐 항목 (`HitInfo`, `RewardInfo` 등) |
| `Network/` | `*Network`, 네트워크 메시지 (`*Payload`, `*Bundle`, `*Snapshot`), NetworkVariable 겸용 Component |
| `Systems/` | System |
| `Utilities/` | Utility (`*Util`) |
| `Proxies/` | Proxy (`*View`) |
| `Data/` | ScriptableObject (`SO*`) |

- 도메인: `Character`, `Combat`, `Core`, `Enemy`, `Navigation`, `Player`, `Weapon`, `World`(`Server/`, `Client/`), `Service`, `UI`, `Input`
- **네임스페이스 = `Project02.<최상위 폴더>`.** 역할 폴더와 `Server`/`Client`는 네임스페이스에 넣지 않는다.
  역할은 폴더와 이름 끝(`*Authoring`, `*System`, `*Util`, `*View`)으로 드러난다.
- 예외: `Input/PlayerAction.cs`는 Input System 자동 생성 파일이라 전역 네임스페이스 그대로 둔다.
- 서버/클라 구분은 파일 위치(`World/Server`, `World/Client`)와 이름(`Server*`, `Client*`, `Predict*`, `Remote*`)으로 한다.
- 파일을 옮기거나 이름을 바꿀 때는 `.meta`를 함께 옮긴다 (GUID 유지 → 프리팹·씬 연결 유지). `git mv` 또는 Unity Project 창에서.

---

## 3. 규칙

1. **Component는 데이터만.** struct 또는 필드만 있는 class. 메서드는 접근 도우미만 허용
   (예: `TickBuffer.TryGetValue`, `Health.IsDead`, `PlayerSimulation.Weapon`). 이벤트 금지. 다른 객체의 메서드 호출 금지.
2. **System은 static class.** 필드 없음. 인자는 World와 필요한 값뿐.
   다른 System을 직접 호출하지 않는다. 순서는 Loop가 정한다.
3. **전역 접근 금지.** `XxxService.Instance`, `XxxLoop.Instance`를 System, Component, Utility 안에서 부르지 않는다.
   필요한 값은 인자로 받는다. 그래야 함수만 보고 무엇에 의존하는지 알 수 있다.
   예외는 두 가지:
   - Loop가 서비스(Iceberg)에서 값을 꺼내 System의 인자로 넘긴다
     (예: `ServerLoop`이 `SessionService.Instance`를 꺼내 `SessionSystem.Process(world, session)`에 넘긴다)
   - Authoring이 NGO 생명주기에서 `XxxLoop.Instance.Register/Unregister`를 부른다
4. **큰 부수효과는 목록에 적어 두고 한 곳에서 처리한다** (0장 "미뤄서 한 곳에서").
   대상: 스폰, 디스폰, 데미지, 보상, RPC 전송.
   적는 쪽은 `world.Xxx.Add(...)`만 하고, 목록을 비우며 처리하는 System은 하나뿐이다.
   클라 입력 전송은 `InputSendSystem` 한 곳, 틱 루프 뒤 프레임당 1회. 보낼 입력의 정본이 `InputHistory` + `AckTick`이므로 별도 큐는 두지 않는다.
5. **쓰기 Utility는 호출처를 제한한다.** 같은 결과를 내야 하는 곳만 부른다.
   - `PlayerStepUtil.Step`: `ServerPlayerSystem`, `PredictSystem`, `ReconcileSystem` (같은 틱 → 같은 결과)
   - `PlayerStepUtil.WriteTransform`: `ServerPlayerSystem`(`world.WriteTransforms`일 때만), `ClientLoop` (틱 루프가 끝난 뒤 1회)
   - `PlayerSpawnUtil.Initialize`: `PlayerServerAuthoring`, `PlayerOwnerAuthoring`의 `OnNetworkPostSpawn` (스폰 1회)
   - `EnemyFsmUtil`: 상태 전환은 `Change`/`Reset`으로만 한다. 상태 함수는 `private` (규칙 위반을 컴파일러가 막는다)
   - 그 밖의 쓰기 Utility: 해당 System + 스폰 초기화 1회 (Authoring의 `OnNetworkPostSpawn`, 또는 World 틱이 필요하면 Loop의 `Register`)
6. **MonoBehaviour는 Loop, Authoring, Network, Proxy 네 종류만.** 시뮬레이션 로직을 갖지 않는다.
   (Iceberg 안의 MonoBehaviour는 예외: UI, Service)
7. **`transform`은 시뮬레이션의 출력 전용.** 시뮬레이션은 `transform`을 읽지 않는다.
   위치의 정본은 `MovementState.Position`. transform 쓰기는 틱 루프가 끝난 뒤 한 번
   (`PlayerStepUtil.WriteTransform`, `EnemySystem` 끝, `*ViewSystem`).
   예외: Authoring이 스폰 시 초기 위치를 읽는 것, 에디터 기즈모.
8. **네트워크 콜백(RPC, `NetworkVariable.OnValueChanged`)은 기록만 한다.**
   시뮬레이션 상태를 바꾸지 않는다. 바뀐 값은 System이 틱 안에서 질의한다
   (예: `StatSystem`의 `AppliedLevel != Network.Level`).
   예측 대상 상태가 바뀌어야 하면 서버 상태(`*State`)에 넣어 `StatePayload`로 보내고 재조정에 맡긴다
   (예: `StaminaState.MaxEnergy`). 연출만 바꾸는 콜백(무기 모델 생성 등)은 허용.
9. **Authoring끼리 서로 참조하지 않는다.** 엔티티의 RPC·NetworkVariable은 `*Network` 하나에 모은다
   (`PlayerNetwork`: `EquippedWeapon`, `Nickname`, `Level`, 수신함).
   RPC 처리기는 그 안의 수신함에 기록만 하고, 각 World의 System이 비운다.
   참조는 Authoring → Network/Proxy/Data, Proxy → Network/Data 한 방향만 둔다. Network는 아무것도 참조하지 않는다.
   Authoring은 데이터를 꺼내려고 Proxy를 참조하지 않는다 (설정은 Data에서 꺼낸다).
10. **설정도 상태도 정본은 하나.**
    - 엔티티의 설정 에셋은 하나 (`SOPlayer`). Authoring과 Proxy는 그것만 참조하고, 하위 설정(`Movement`, `LevelTable`, `Catalog`)은 그 안에서 꺼낸다.
      같은 SO를 컴포넌트마다 따로 연결하지 않는다.
    - 무기 설정의 정본은 `PlayerSimulation.Weapon` (`Catalog.Get(State.Weapon.EquippedId)`). Proxy는 판정용 값을 노출하지 않는다.
    - 위치의 정본은 `MovementState.Position` (규칙 7).
11. **소비 시스템은 컴포넌트 묶음 목록만 본다.** 판정·사망·표적 같은 소비 시스템은 "적인지 플레이어인지"를 모른다.
    종류 목록(`Players`, `Enemies`)은 정본을 가진 시스템(이동, 되감기 기록, 스냅샷)만 쓴다.
    묶음은 struct로 두고 담긴 컴포넌트를 첫 줄 주석에 적는다 (예: `HitTarget` = `Team` + `RewindTarget` + `Health`, `Mortal` = `Health` + `NetworkObject` + 보상).
    묶음 등록·해제는 `ServerLoop.Register`/`Unregister`에서 종류 목록과 함께 한다. 같은 편(`Team`)끼리는 맞지 않는다.

---

## 4. 시간

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

## 5. 실행 모드

| 모드 | 용도 | 이 프로세스의 World |
|---|---|---|
| 전용 서버 | 멀티플레이 | `ServerWorld`만 |
| 클라이언트 | 멀티플레이 | `ClientWorld`만 |
| 호스트 | **싱글 플레이 전용** | 둘 다 |

- 실행 순서: `ClientLoop`(-200) → `ServerLoop`(-100) (`[DefaultExecutionOrder]`). 호스트에서 입력이 같은 프레임에 서버에 닿는다.
- 시뮬레이션 사본은 World마다 따로 둔다: 서버 권한 `PlayerServerAuthoring.Simulation`, 예측 `PlayerOwnerAuthoring.Simulation`.
  호스트에서도 둘은 섞지 않는다 (서버 사본으로 예측하지 않는다).
- transform은 한 World만 쓴다: 클라 World가 있으면 클라가 쓴다. `ServerWorld.WriteTransforms`는 `!IsClient`일 때만 true.
- 원격 표시 Authoring은 `enabled = IsClient && !IsOwner`, 적 표시는 `enabled = IsClient`. 서버 전용 Authoring은 `if (!IsServer) return;`.
- 호스트에서도 스냅샷과 StateRPC는 자기 자신에게 간다. 지연 0이므로 리컨사일 0회가 정상.

---

## 6. 이름

| 형태 | 의미 |
|---|---|
| `*World` | World (데이터) |
| `*Loop` | Loop (진입점) |
| `*Authoring` | Authoring |
| `*Network` | 엔티티의 RPC·NetworkVariable 묶음 |
| `*Simulation` | 엔티티의 시뮬레이션 데이터 Component |
| `*System` | System |
| `*Util` | Utility (예외 없음). Iceberg 안의 물리 유틸(`CharacterMotor`, `CharacterPhysics`)만 그대로 |
| `*View` | 연출 Proxy |
| `*State` (struct) | 롤백/스냅샷 대상 시뮬레이션 상태 (로직 클래스에 쓰지 않는다) |
| `*Payload`, `*Bundle`, `*Snapshot`, `*Message` (struct) | 네트워크 메시지 |
| `*Info` (struct) | 큐 항목 (`HitInfo`, `RewardInfo`, `ShotInfo`) |
| `SO*` | ScriptableObject 전부 |
| `*Service` | Iceberg 진입점 |
| `*Presenter` / `*Panel` / `*Widget` | UI (MVP): 로직 / 화면 하나 / 화면 안의 부품 |
| `Step` | 데이터만 바꾸는 1틱 진행 (Unity 객체 부수효과 없음) |
| `Tick` | System이나 Loop가 World를 1틱 돌림 |
| `Restore` | 상태 복원 (재조정) |
| `Process` | 큐를 비우며 처리 |
| `Record` | 버퍼에 기록만 |
| `Present` | LateUpdate 연출 |

- 필드: private `_camelCase`, Component 데이터와 Authoring의 형제 참조 `public PascalCase`, 상수 `UPPER_SNAKE`
- 직렬화 필드(`[SerializeField]`)의 이름은 바꾸지 않는다. 바꾸면 프리팹 연결이 끊긴다 (필요하면 `[FormerlySerializedAs]`)
- 시뮬레이션 dt 인자는 `deltaTime`, 렌더 프레임 시간 인자는 `frameTime`
- RPC 메서드는 `*RPC`. 틱마다 가는 스트림은 Unreliable, 사건(발사)은 Reliable
- 큐 기록은 `world.Xxx.Add(...)`로 직접 (별도 Enqueue 메서드 없음)
- FSM: 상태 하나 = 파일 하나 `EnemyFsmUtil.<상태>.cs` (`static partial`, 함수는 `private Enter<상태>`/`Tick<상태>`/`Exit<상태>`).
  상태 추가 = 파일 하나 + `EnemyFsmUtil.cs`의 switch에 case. 빈 상태 파일은 미리 만들지 않는다
- 디버그 코드는 `#if UNITY_EDITOR || DEVELOPMENT_BUILD`, 에디터 기즈모는 `#if UNITY_EDITOR`
- `UnityEngine.Object`에는 `?.`, `??`, `is null` 금지. `== null`로 비교

---

## 7. 이 코드는 어디에 두나 (판단 순서)

1. 엔진 기능에 값을 넣기만 하나? → **Proxy**
2. NGO 생명주기에서 등록/초기화만 하나? → **Authoring**
3. RPC·NetworkVariable인가? → 엔티티의 **`*Network`**
4. 틱마다 바뀌고 롤백/스냅샷 대상인가? → **`*State`** (`*Simulation` Component 안)
5. World에 하나뿐인가? (시계, 큐, 표) → **Singleton Component** (`*World` 필드)
6. 디자이너가 정하는 고정값인가? → **Data** (`SO*`, 엔티티 설정 SO 안에 연결)
7. 입력 → 출력만 있는 계산인가? → **읽기 Utility**
8. 여러 엔티티의 Component를 읽고 쓰는 행동인가? → **System**
9. 위 어디에도 안 맞고 크기가 큰가? → **Iceberg**. 경계(진입 함수 하나)만 정한다

---

## 8. 바꾸지 않는 것 (Iceberg)

- UI: `UI/Presenters`, `UI/Panels` (MVP 유지), `SettingsService`
- 입력 장치: `InputService`, `PlayerAction`
- 길찾기: `PathGrid`, `PathFinder`
- 물리 유틸: `CharacterPhysics`, `CharacterMotor`
- 접속/부팅/저장: `ConnectionService`, `SessionService`, `ProgressService`, `ServerLauncher`, `Bootstrapper` → `SessionService`는 `SessionSystem` 한 곳에서만 부른다 (Loop가 인자로 넘김)
- 연출: `ProjectileVisualService` → `WeaponView`(Proxy)에서만 부른다
- `*Service`는 World보다 오래 살거나 ECS 밖(UI 등)도 쓰는 기능의 입구다. `Services.prefab`에 두고 씬이 바뀌어도 유지된다.
  둘 다 아니면 Service로 만들지 않는다 (전투 씬 안의 Proxy나 오브젝트로 둔다).
  ECS 쪽에서는 Loop만 `Instance`로 꺼내 System 인자로 넘긴다 (규칙 3). 예외: `WeaponView` → `ProjectileVisualService` (이동 예정)
---

## 9. 저장

- 진행도는 세계 단위 `.sav` 하나에 저장한다 (내용은 JSON). 스키마 `SaveFile { Version, SavedAt, World, Players[] }`.
- 싱글: 슬롯 6개 고정, 슬롯마다 파일 하나. 전용 서버: `server.sav` 하나. 스키마는 같다.
- 저장 파일은 여는 쪽(`ServerLauncher` / 싱글 시작)이 `ProgressService.Open`으로 명시적으로 열고 `Close`로 닫는다.
- 읽기 실패한 파일은 `.corrupt-<시각>`으로 옮기고(격리) 시작하지 않는다. 이 빌드보다 새 버전 파일은 손대지 않고 시작하지 않는다.
  진행도를 잃느니 시작하지 않는다. 복구는 관리자가 파일을 보고 정한다.
- 전용 서버 초기화는 관리자가 파일을 지우는 것으로 한다 (게임 안에 초기화 기능 없음).

---

## 10. 과도기 운영

- 한 번에 한 경로만 바꾼다. 한 단계가 끝나기 전에 다음 단계를 시작하지 않는다.
- 옛 코드는 같은 단계 안에서 지운다. 여러 단계에 걸쳐 남겨야 하면 `[System.Obsolete]`를 붙인다 (컴파일러 경고가 남은 작업 목록).
- 컴포넌트를 지우거나 SO 필드를 옮긴 단계는 프리팹·SO 연결을 다시 확인하고 **File → Save Project**까지 한 뒤 커밋한다.
  (저장 안 된 SO 변경은 다른 프로세스(가상 플레이어, 빌드)에 보이지 않는다)

### 회귀 확인 (단계마다 같은 조건)

- 컴파일 에러 0, Missing Script 0 (Player 프리팹, 몬스터 프리팹, `Services.prefab`, MAIN MENU / WAITING ROOM / TEST WORLD 씬)
- 에디터: Player 선택 시 충돌 캡슐과 턱 높이 기즈모가 보인다 (`PlayerBodyView`)
- 호스트(싱글), 지연 0: 리컨사일 0회, 무기 모델과 손 IK가 붙어 있다, 적과 자기 캐릭터가 정상 표시
- 지연 200ms + 손실 10%: 1분간 리컨사일 횟수를 단계 전후로 비교
- 적을 쏘면 피해가 들어가고, 죽으면 경험치가 오른다. 벽 뒤 적은 안 맞는다
- 지연 200ms에서 옆으로 움직이는 적의 몸 중앙을 겨눠 쏘면 맞는다 (지연 보상)
- 적: 배회(General) → 감지 시 추적(Battle) → 범위 이탈 시 배회
- 클라 2개: 상대 캐릭터와 발사 연출이 부드럽고, 몸보다 총알이 먼저 나가지 않는다
- 클라에서 적 처치 반복 후 `ServerLoop`의 `Enemies` 수가 0 (클라는 서버 World에 등록하지 않음)
- 클라 일시정지(또는 큰 히치) 후 재개: `clock resync` 1회 + 재조정 최대 1회, 이후 입력 정상 반영, buffer가 목표(3) 근처로 복귀
- 저장: 경험치를 얻고 나갔다 들어오면 경험치가 유지된다. 깨진 `.sav`는 `.corrupt-*`로 옮겨지고 서버가 시작하지 않는다

### 보류 (확인 수단이 생기면 위로 올린다)

| 항목 | 필요한 것 |
|---|---|
| 레벨업: `Level` 증가 → 체력·기력 최대치 갱신 (`StatSystem`), 리컨사일 1회 | `PlayerLevelTable`에 2레벨 이상 |
| 적 35마리 이상: 적과 원격 플레이어 보간이 끊기지 않는다 (스냅샷 분할) | 여러 마리를 한 번에 소환하는 수단 |
| 무기 교체(`EquippedWeapon` 변경): 소유자 리컨사일 정확히 1회, 이후 새 연사 간격·줌 적용, 추가 리컨사일 없음 | 무기 2개 이상 + 교체 입력 |