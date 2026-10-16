# ECS 규칙 (Project02)

Unity Entities(DOTS)를 쓰지 않고, NGO GameObject 구조 위에 ECS의 "규칙"만 적용한다.
목적은 시뮬레이션(물리)과 연출의 분리, 그리고 부수효과의 위치를 한 곳으로 모으는 것.
참고: Overwatch Gameplay Architecture and Netcode (GDC 2017, Tim Ford)

---

## 1. 용어

| 용어 | 이 프로젝트에서 | 예 |
|---|---|---|
| World | 시스템 실행 순서와 시계를 가진 진입점. 서버/클라에 하나씩 | `ServerSimulation`, `ClientSimulation` |
| Entity | NetworkObject 하나. 식별자는 `NetworkObjectId` | 플레이어, 적 |
| Component | 데이터 덩어리. struct 또는 필드만 있는 class | `MovementState`, `WeaponState`, `TickBuffer<T>` |
| Singleton Component | World에 하나뿐인 데이터 | `ClientClock`, `InterpolationClock`, `_hits`, `_deaths` |
| System | 필드 없는 static class. World가 정해진 순서로 호출 | `PredictSystem`, `DamageSystem` |
| Utility | 순수 함수. 시스템이 부름 | `CharacterMotor`, `PlayerMovement`, `WeaponFire` |
| Proxy | 엔진 기능(NGO, Animator, Cinemachine, IK)에 값을 전달만 하는 MonoBehaviour | `PlayerVisual`, `WeaponView` |
| Iceberg | ECS 밖의 큰 하위 시스템. 경계만 정하고 내부는 건드리지 않음 | NGO, `PathGrid`, UI, `ProgressService` |

---

## 2. 규칙

1. **Component는 데이터만.** struct 또는 필드만 있는 class. 메서드는 접근 도우미만 허용
   (예: `TickBuffer.TryGet`). 이벤트 금지. 다른 객체의 메서드 호출 금지.
2. **System은 static class.** 필드 없음. 인자는 World(또는 Singleton)와 필요한 Component뿐.
   다른 System을 직접 호출하지 않는다. 순서는 World가 정한다.
3. **전역 접근 금지.** `XxxService.Instance`, `XxxSimulation.Instance`를 System과 Component 안에서 부르지 않는다.
   필요한 값은 World가 인자로 넘긴다. (예외: 등록용 Proxy의 `Register/Unregister`)
4. **큰 부수효과는 큐에 쌓고 한 곳에서 처리한다.** 스폰, 디스폰, 데미지, 사망, 보상, RPC 전송.
   쌓는 쪽은 기록만 하고, 처리하는 System은 하나뿐이다.
5. **여러 곳에서 부르는 Utility는 읽기 위주.** 쓰기가 많은 Utility(플레이어 1틱 진행)는
   호출처를 서버 Tick과 클라 예측/재실행 두 곳으로 제한한다.
6. **MonoBehaviour는 두 종류만.**
   - 등록(Authoring): NGO 생명주기(`OnNetworkSpawn/Despawn`)에서 World에 엔티티를 등록/해제
   - Proxy(View): Animator, Cinemachine, IK, 이펙트, NetworkVariable 동기화
   시뮬레이션 로직을 갖지 않는다.
7. **`transform`은 시뮬레이션의 출력 전용.** 시뮬레이션은 `transform`을 읽지 않는다.
   위치의 정본은 `MovementState.Position`이다.

---

## 3. 시간

| 값 | 단위 | 용도 |
|---|---|---|
| `SimulationClock.DeltaTime` | 초 (0.02, 50Hz) | 1틱의 길이. 시뮬레이션 dt는 항상 이것 |
| `frameTime` (`Time.deltaTime`) | 초 | 렌더 프레임 길이. 시계 누적과 연출에만 |
| `*Tick` (int) | 틱 | "언제": 연사 간격, 쿨다운, 버퍼 지연 |
| 초당 비율 × dt | | "얼마나 빨리": 속도, 회복량 |

- 초 → 틱: `÷ SimulationClock.DeltaTime`, 틱 → 초: `× SimulationClock.DeltaTime`
- 시계는 World가 가진다. Unity `FixedUpdate`와 `Time.fixedTime`은 쓰지 않는다.
- 클라에는 시간축이 둘이다. 섞지 않는다.
  - 예측 시계 `ClientClock.Tick`: 클라 번호, 서버보다 미래
  - 보간 시계 `InterpolationClock.RenderTick`: 서버 번호, 서버보다 과거
  - 둘을 잇는 다리는 입력의 `ViewTick` 하나뿐

---

## 4. 이름

| 형태 | 의미 |
|---|---|
| `*Simulation` | World |
| `*System` | System |
| `*State` (struct) | 시뮬레이션 상태. 스냅샷/롤백 대상 |
| `*Payload`, `*Bundle`, `*Snapshot` (struct) | 네트워크 메시지 |
| `SO*` | ScriptableObject 전부 |
| `*View`, `*Visual` | 연출 Proxy |
| `Step` | 순수 함수로 1틱 진행 |
| `Tick` | World가 System/엔티티를 1틱 돌림 |
| `Capture` / `Restore` | 상태 저장 / 복원 |
| `Process` | 큐를 비우며 처리 |
| `Record` / `Enqueue*` | 큐나 버퍼에 기록만 |
| `Present` | LateUpdate 연출 |

- 필드: private `_camelCase`, 시스템에 공개하는 컴포넌트 데이터 `internal PascalCase`, 상수 `UPPER_SNAKE`
- 시뮬레이션 dt 인자는 `deltaTime`, 렌더 프레임 시간 인자는 `frameTime`
- RPC 메서드는 `*RPC`. 틱마다 가는 스트림은 Unreliable
- 디버그 코드는 `#if UNITY_EDITOR || DEVELOPMENT_BUILD`

---

## 5. 이 코드는 어디에 두나 (판단 순서)

1. 엔진 기능에 값을 넣기만 하나? → **Proxy**
2. 틱마다 바뀌고 롤백/스냅샷 대상인가? → **`*State` Component**
3. World에 하나뿐인가? (시계, 큐, 표) → **Singleton Component**
4. 입력 → 출력만 있는 계산인가? → **Utility**
5. 여러 엔티티의 Component를 읽고 쓰는 행동인가? → **System**
6. 위 어디에도 안 맞고 크기가 큰가? → **Iceberg**. 경계(진입 함수 하나)만 정한다

---

## 6. 바꾸지 않는 것

- UI: `Presenter/*`, `View/*` (MVP 유지), `SettingsService`
- 입력 장치: `InputService`, `PlayerAction`
- 길찾기: `PathGrid`, `PathFinder`
- 물리 유틸: `CharacterPhysics`, `CharacterMotor`
- 접속/부팅/저장: `ConnectionService`, `SessionService`, `ProgressService`, `ServerLauncher`, `Bootstrapper`
  → World에서는 정해진 System 한 곳에서만 부른다

---

## 7. 과도기 운영

- 한 번에 한 경로만 바꾼다. 한 단계가 끝나기 전에 다음 단계를 시작하지 않는다.
- 옛 코드는 바로 지우지 않고 `[System.Obsolete]`를 붙인다. 컴파일러 경고가 남은 작업 목록이다.
  해당 단계가 끝나면 삭제한다.
- 단계마다 회귀 확인을 같은 조건으로 한다.
  - 지연 0: 리컨사일 0회
  - 지연 200ms + 손실 5%: 1분간 리컨사일 횟수를 단계 전후로 비교
  - 적을 쏘면 피해가 들어가고, 죽으면 경험치가 오른다