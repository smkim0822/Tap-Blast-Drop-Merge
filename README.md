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
* **반응형 UX/UI 및 연출:** 다양한 모바일 해상도(Safe Area) 대응, AnimationCurve 기반 팝업 연출, 동적 사운드 피치(Pitch) 적용.
* **글로벌 랭킹 시스템:** 전용 서버와의 REST API 통신을 통해 실시간 Top 50 랭킹 및 내 순위 제공.

## 🛠️ 기술 스택 (Tech Stack)
* **Client (Game Engine):** Unity 6 (6000.3.8f1), C#
* **Architecture & Pattern:** Singleton Pattern
* **Communication:** REST API (UnityWebRequest)

## 🔗 관련 프로젝트 (Related Project)
* [[랭킹 서버 레포지토리 (RankingServer)]](https://github.com/romantic02/RankingServer)

## 📅 개발 히스토리 (Change Log)
* **v0.7.0:** 랭킹 시스템 연동 완료 (REST API 통신, UUID 기반 식별, 랭킹 UI 연동)
* **v0.6.1:** Unity 6 엔진 업그레이드, 모바일 해상도 대응 및 UI/UX 폴리싱
* **v0.6.0:** 핵심 게임 루프 완성 및 UI 시스템 고도화