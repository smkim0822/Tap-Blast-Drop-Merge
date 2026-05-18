# Tap Blast: Drop & Merge

> **"직관적인 조작과 예측 불가능한 물리 반응이 결합된 모바일 하이퍼캐주얼 낙하형 퍼즐 게임"**

![Project Status](https://img.shields.io/badge/Status-Release_v1.0.0-brightgreen?style=flat-square)
![Unity](https://img.shields.io/badge/Unity-6_(6000.3.8f1)-black?style=flat-square&logo=unity)
![Platform](https://img.shields.io/badge/Platform-Android-green?style=flat-square)

## 🎮 프로젝트 소개
**Tap Blast: Drop & Merge**는 물리 엔진을 기반으로 한 낙하형 병합 퍼즐 게임입니다. 
플레이어는 최적의 타이밍과 위치에 폭탄을 투하하여 연쇄 분열과 병합을 유도하고, 최고 점수를 달성해 글로벌 유저들과 경쟁합니다. 
복잡한 튜토리얼 없이 누구나 쉽게 시작할 수 있으면서도, 다이나믹한 물리 충돌이 만들어내는 예측 불가능한 상황들로 몰입감을 제공합니다.

## 🕹️ 핵심 플레이 (Core Gameplay)
1. 발사대를 움직여 원하는 위치에 오브젝트를 낙하시킵니다.
2. 낙하한 오브젝트는 여러 개의 공으로 분열되며, 같은 색상의 공끼리 충돌하면 폭발하여 사라지고 점수를 획득합니다.
3. 공이 제한 구역을 넘지 않도록 관리하며 최고 점수를 경신하고 랭킹 보드에 이름을 새깁니다.

## ✨ 주요 특징 (Key Features)
* **다이나믹 물리 퍼즐 시스템:** 물리 충돌, 중력, 산개 효과를 활용한 역동적인 화면 연출 및 연쇄 폭발 알고리즘.
* **끊김 없는 플레이 경험:** 게임 오버 시 씬 전환이나 로딩 없는 즉각적인 재시작 로직 구현.
* **모바일 최적화 UX/UI:** 가상 키보드 가림 현상 방지(반응형 팝업 UI), 다양한 모바일 해상도(Safe Area) 대응, AnimationCurve 기반 팝업 연출.
* **한글 지원:** 조합형 언어(IME) 예외 처리가 적용된 닉네임 입력 시스템. 한글 11,172자 정적(Static) 폰트 아틀라스를 통한 런타임 렌더링 최적화.
* **라이브 랭킹 시스템:** 오라클 클라우드 서버와 HTTPS(REST API) 기반 실시간 통신을 통해 데이터 즉시 동기화 및 무결성 확보. API 도배 방지 쿨타임 적용.
* **오프라인 대응 및 지연 동기화:** 네트워크 단절 시 로컬 백업(PlayerPrefs)을 통한 데이터 보존 및 온라인 복귀 시 자동 백그라운드 동기화 지원. 에뮬레이터 및 실기기 환경의 예외 상황 통제.

## 🛠️ 기술 스택 (Tech Stack)
* **Client (Game Engine):** Unity 6 (6000.3.8f1), C#
* **Architecture & Pattern:** 단일 Entry(`GameManager`) + 세션 플로우 분리 + 이벤트 기반(세션/점수/오디오) + 서비스/스토어 분리
* **Communication & Security:** REST API (UnityWebRequest), HTTPS (SSL/TLS)

## 🧱 아키텍처 (Architecture)
프로젝트 초기에는 여러 책임이 `GameManager`, `ScoreManager`, `RankingPopup/Manager` 등에 집중되어 결합도가 높았습니다. 현재는 **역할(Responsibility) 단위로 컴포넌트/서비스를 분리**해, 유지보수와 디버깅 비용을 낮추는 방향으로 개선했습니다.

### 게임 세션 플로우
- **`GameManager`**: 씬 진입점(Entry) + 레퍼런스 조립 최소화
- **`GameSessionFlow`**: 상태 전이(메인/플레이/게임오버), 재시작/메인복귀, 서버 점수 동기화 플로우
- **`GameUIRoots`**: 메인/게임오버/플레이 UI 루트 활성화 및 플레이어 배치
- **`GameplayWorldReset`**: 풀 오브젝트/스택/이펙트 등 월드 리셋

### 점수(단일 소스) + HUD 뷰 분리
- **`BestScoreStore`**: 최고점수 저장소(PlayerPrefs 캡슐화)
- **`ScoreState`**: 현재점수/최고점수의 순수 상태 모델
- **`ScoreHudView`**: 점수 텍스트/펀치 애니메이션 등 시각 요소 담당

### 랭킹 UI/동기화 분리
- **`RankingBoardView`**: 리스트 렌더링/오프라인·로딩 표시/내 랭크 표시
- **`RankingSyncFlow`**: 닉네임/점수 동기화 및 랭킹 로딩 플로우(중복 요청 방지 포함)
- **`RankingPlayerIdentityStore` / `RankingRequestSigner` / `RankingWebRequestFactory`**: UUID/해시/요청 생성 책임 분리

### 풀링 안정화
- **`PoolService`**: `PoolManager` 의존을 완화한 파사드(없으면 Instantiate/Destroy 폴백)
- **`PoolManager`**: 풀 상한(`maxPooledPerPrefab`) 및 null 엔트리 정리로 안정성 강화

## 🧰 마이그레이션 (Scene Reference Migration)
리팩토링 과정에서 기존 컴포넌트에 들어있던 인스펙터 레퍼런스를 새 컴포넌트로 옮겨야 하는 경우가 있습니다.

- **메뉴**: `Dev/Migrate Refactor References (Active Scene)`
- **역할**: 활성 씬에서 `GameManager/ScoreManager/Spawner/RankingPopup` 등 레퍼런스를 새 컴포넌트(`GameUIRoots`, `ScoreHudView`, `SpawnerVisuals`, `RankingBoardView`)로 복사

## 🔗 관련 프로젝트 (Related Project)
* [[랭킹 서버 레포지토리 (RankingServer)]](https://github.com/romantic02/RankingServer)

## 📅 개발 히스토리 (Change Log)
* **v1.0.0:** Android Release 빌드·앱 아이콘, QA·씬 레퍼런스 정리, 세로 화면 orientation 수정, `ServerConfig` example·gitignore 정리
* **v0.9.0:** 피그마 기반 UI 디자인 반영(메인·인게임·랭킹·설정·결과) — **주요 작업**, 공식 타이틀 **Tap Blast: Drop & Merge** 확정 및 저장소명 정리
* **v0.8.4:** 결합도 높은 매니저/로직 리팩토링(세션 플로우 분리, 점수 단일 소스, 랭킹 동기화/뷰 분리, 풀 안정화), 운영 서버 Base URL/포트 처리 보강
* **v0.8.3:** 닉네임 한글 입력(IME) 시스템 구축, 정적 폰트 아틀라스 렌더링 최적화 (OOM 및 Tofu 현상 방지)
* **v0.8.2:** 에뮬레이터 및 실기기 환경 오프라인 UI 동기화 결함 수정, 비동기 레이스 컨디션(Race Condition) 최적화
* **v0.8.1:** 오프라인 랭킹 예외 처리, 데이터 유실 방지(로컬 백업) 및 지연 동기화 파이프라인 구축
* **v0.8.0:** 라이브 랭킹 서버 정식 통합 (HTTPS 통신), 데이터 즉시 동기화 아키텍처 구축 및 모바일 키보드 UX/UI 폴리싱
* **v0.7.0:** 랭킹 시스템 연동 (REST API 통신 기반 구축, UUID 식별)
* **v0.6.1:** Unity 6 엔진 업그레이드, 모바일 해상도 대응
* **v0.6.0:** 핵심 게임 루프 완성 및 UI 시스템 고도화
