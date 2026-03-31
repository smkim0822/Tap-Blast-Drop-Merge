# 💧 Project Drop (가제)

> **"직관적인 조작과 예측 불가능한 물리 반응이 결합된 모바일 하이퍼캐주얼 낙하형 퍼즐 게임"**

![Project Status](https://img.shields.io/badge/Status-Developing-yellow?style=flat-square) 
![Unity](https://img.shields.io/badge/Unity-6_(6000.0.38f1)-black?style=flat-square&logo=unity)

## 🎮 프로젝트 소개
**Project Drop**은 물리 엔진을 기반으로 한 낙하형 병합 퍼즐 게임입니다. 
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
* **라이브 랭킹 시스템:** 오라클 클라우드 서버와 HTTPS(REST API) 기반 실시간 통신을 통해 데이터 즉시 동기화 및 무결성 확보. API 도배 방지 쿨타임 적용.
* **오프라인 대응 및 지연 동기화:** 네트워크 단절 시 로컬 백업(PlayerPrefs)을 통한 데이터 보존 및 온라인 복귀 시 자동 백그라운드 동기화 지원.

## 🛠️ 기술 스택 (Tech Stack)
* **Client (Game Engine):** Unity 6 (6000.3.8f1), C#
* **Architecture & Pattern:** Singleton Pattern
* **Communication & Security:** REST API (UnityWebRequest), HTTPS (SSL/TLS)

## 🔗 관련 프로젝트 (Related Project)
* [[랭킹 서버 레포지토리 (RankingServer)]](https://github.com/romantic02/RankingServer)

## 📅 개발 히스토리 (Change Log)
* **v0.8.1:** 오프라인 랭킹 예외 처리, 데이터 유실 방지(로컬 백업) 및 지연 동기화 파이프라인 구축
* **v0.8.0:** 라이브 랭킹 서버 정식 통합 (HTTPS 통신), 데이터 즉시 동기화 아키텍처 구축 및 모바일 키보드 UX/UI 폴리싱
* **v0.7.0:** 랭킹 시스템 연동 (REST API 통신 기반 구축, UUID 식별)
* **v0.6.1:** Unity 6 엔진 업그레이드, 모바일 해상도 대응
* **v0.6.0:** 핵심 게임 루프 완성 및 UI 시스템 고도화
