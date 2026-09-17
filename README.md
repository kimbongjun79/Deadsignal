# DEAD SIGNAL

탑다운 시점의 좀비 서바이벌 슈터. Unity로 제작.

빛이 닿는 곳만이 유일한 안전지대. 제한된 탄약과 좁은 시야 속에서 10분간 버텨내야 한다.

---

## 게임 개요

- **장르**: 탑다운 서바이벌 슈터
- **목표**: 10분간 생존 (클리어), 생존 중 사망 시 게임오버
- **핵심 시스템**: 손전등형 시야 제한, 히트스캔 사격, 탄약 관리, 시간 기반 난이도 스케일링

## 조작

| 입력 | 동작 |
|---|---|
| W / A / S / D | 이동 |
| Shift (이동 중 홀드) | 달리기 |
| Q / E | 좌 / 우 회전 |
| Space | 사격 |
| R | 재장전 |

## 구현된 시스템

### 플레이어
- WASD 이동 + Q/E 고정 각도 회전 방식
- 걷기/달리기 8방향 Blend Tree 애니메이션 (2D Freeform Directional)
- Fire Layer 분리(Avatar Mask) + Animation Rigging(Aim IK)로 이동 중에도 안정적인 상체 사격 모션
- 히트스캔(Raycast) 기반 사격, 연사 제한(Fire Rate)
- 탄창/예비 탄약 시스템, R키 재장전(4초)
- 사망 시 입력/이동/회전 완전 차단, 총기 드롭(물리 낙하) 연출

### 몬스터 (Enemy)
- `EnemyBase` 추상 클래스: 추격(NavMesh), 체력, 사망 처리 공통화
- `MeleeEnemy`: 근접 사거리 진입 시 히트박스 활성화 공격, 중복 히트 방지, 공격 중 이동/회전 고정
- 시간 기반 지수 곡선 난이도 스케일링 (스폰 간격 감소 + 체력 배율 증가)
- NavMesh 유효 위치 검증을 거쳐 벽/맵 밖 스폰 방지

### 아이템
- 회복/탄약 2종, 맵 내 무작위 NavMesh 유효 위치에 주기적 스폰
- 플레이어 접촉 시 즉시 효과 적용 후 소멸

### 체력 시스템
- 무료 에셋 **Health System For Dummies** 기반

### 게임 상태 / UI
- `GameManager`: 생존 타이머, 처치 수 카운트, 10분 생존 시 클리어 판정
- HUD: 남은 시간, 처치 수, 탄약 표시 (TextMeshPro)
- 씬 흐름: `Title → Gameplay → GameOver / Clear`
- 결과 화면(GameOver/Clear 공용): 생존 시간 및 처치 수 표시

### 이펙트 / 사운드
- War FX 머즐 플래시 파티클 연동 (Instantiate + Destroy 방식)

## 사용 에셋

| 에셋 | 용도 |
|---|---|
| Health System For Dummies | 체력/생존 관리 |
| Cartoon FX / War FX (Jean Moreno) | 총구 이펙트 |
| Animation Rigging (Unity 공식 패키지) | 사격 시 총구 방향 IK 고정 |
| AI Navigation (Unity 공식 패키지) | 몬스터 추격 경로 탐색 |

## 스크립트 구조

```
PlayerController.cs      - 플레이어 이동/조준/사격/탄약/사망 처리
EnemyBase.cs              - 몬스터 공통 로직 (추격/체력/사망)
MeleeEnemy.cs             - 근접 공격 몬스터
AttackHitbox.cs           - 근접 공격 판정
EnemySpawnManager.cs      - 시간 기반 난이도 스케일링 몬스터 스폰
Item.cs                   - 회복/탄약 아이템
ItemSpawnManager.cs       - 아이템 주기적 스폰
GameManager.cs            - 생존 타이머, 처치 수, 클리어/오버 판정
HUD.cs                    - 인게임 UI (시간/킬카운트/탄약)
AmmoUI.cs                 - 탄약 UI
ResultScreen.cs           - 게임오버/클리어 결과 화면
TitleScreen.cs            - 타이틀 화면 (시작/종료)
```

## 남은 작업 (TODO)

- 권총 외 무기 종류 확장
- 원거리(곡사포) 공격 몬스터, 뛰는 좀비 등 타입 추가
- 피격 이펙트(붉은 깜빡임)
- 사운드 (발사/피격/재장전/BGM)
- 지형지물 메쉬 실제 적용, 벽 Collider 이음새 정리
- 레이저 조준선 재설계

## 개발 환경

- Unity (Universal Render Pipeline 여부는 프로젝트 세팅 확인 필요)
- 3D, Humanoid 애니메이션 리깅