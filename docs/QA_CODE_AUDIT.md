# 코드 기준 QA 감사 (2026-05-18)

> **실기 테스트(10분 플레이, IME, Safe Area 등)는 이 문서만으로 완료 처리하지 않습니다.**  
> 각 GitHub 이슈는 사용자 확인 후 close 합니다.

## #114 장시간 플레이 · 프레임 · 메모리(풀링)

| 항목 | 결과 | 조치 |
|------|------|------|
| `PoolManager` 프리팹별 상한 128 | ✅ | - |
| 재시작 시 `GameplayWorldReset` → `PoolService.Despawn` | ✅ | - |
| 드래그 가이드라인 `RaycastHit2D[16]` 매 프레임 할당 | ⚠️ | `SpawnerVisuals` 정적 버퍼 재사용 (#PR) |
| `guideLine.material` 매 프레임 인스턴스 | ⚠️ | `sharedMaterial` 사용 (#PR) |
| 연쇄 폭발 시 동적 Rigidbody2D 다수 | ⚠️ | 물리 부하 — 실기 프로파일러 확인 필요 |
| 랭킹 리스트 `Instantiate` | ℹ️ | 빈도 낮음, 풀 미적용 |

**남은 완료 조건:** Android 실기 10분+ 플레이, Profiler에서 GC 스파이크·메모리 증가 확인.

## #111 랭킹 로딩 UI

| 항목 | 결과 |
|------|------|
| `loadingMessageObject` 씬 연결 | ❌ 씬에 Loading 오브젝트 없음 |
| 코드 폴백 | ✅ `RankingBoardView.SetLoading` — `contentParent` CanvasGroup (#PR) |

## #109 게임오버 → 랭킹

| 항목 | 결과 |
|------|------|
| `GameOverPanel.rankingButton` | ❌ 인스펙터 미연결 |
| `PopupCard` 자식에 `RankingButton` | ❌ **게임오버 카드에 랭킹 버튼 UI 없음** (Retry만 존재) |
| `ResolveRankingButton()` | 런타임 탐색 실패 가능 |

**권장:** 피그마/씬에 게임오버 랭킹 버튼 추가 후 `rankingButton` 연결, 또는 메인/설정 랭킹만 사용한다고 명시.

## #110 Safe Area

- `SafeAreaPanel` + `SafeArea.cs` 존재 → 실기 2종 해상도 확인 필요.

## #112 닉네임 · #113 키보드

- `NicknameValidator`, `RankingPopup` 키보드 오프셋 로직 존재 → 실기 IME·키보드 확인 필요.

## #119 ServerConfig

- `Assets/Resources/ServerConfig.asset` gitignore, `ServerConfig.asset.example` 제공 ✅
- 운영 URL·키는 로컬 asset / 배포 환경에서 확인 필요.
