# 💧 (가제)Project Drop (개발 중)

> **"공을 떨어뜨리고 분열시키고 폭발시켜 점수를 획득하는 안드로이드 하이퍼캐주얼 퍼즐 게임"**

![Project Status](https://img.shields.io/badge/Status-Developing-yellow?style=flat-square) 
![Unity](https://img.shields.io/badge/Unity-2022.3.x-black?style=flat-square&logo=unity)
![Spring Boot](https://img.shields.io/badge/Backend-SpringBoot-green?style=flat-square&logo=springboot)

## 🎮 프로젝트 소개
Project Drop은 물리 엔진을 활용한 낙하형 퍼즐 게임입니다.
플레이어는 폭탄을 투하하여 공으로 분열시키고, 같은 색상의 공을 합쳐 점수를 획득합니다.
단일 씬(Single Scene) 구조를 통해 끊김 없는 게임 플레이 경험을 제공하는 것을 목표로 합니다.

## 🛠️ 기술 스택 (Tech Stack)
- **Client:** Unity (C#)
- **Backend:** Spring Boot, MySQL (글로벌 랭킹 시스템 연동 예정)
- **Design Pattern:** - **Object Pooling:** 메모리 최적화를 위해 폭탄 및 공 객체 재사용
  - **Singleton:** 게임 매니저 및 점수 시스템 관리
  - **Observer Pattern:** (추후 적용 예정) UI 이벤트 처리

## ✨ 핵심 기능 (Key Features)
- **물리 기반 분열 시스템:** 충돌 시 자연스러운 산개 효과 구현
- **BatchID 로직:** 갓 생성된 객체 간의 무한 연쇄 폭발 방지 알고리즘 적용
- **최적화된 씬 관리:** `ResetData` 메서드를 통한 로딩 없는 즉시 재시작 구현
- **반응형 UI:** 모바일 Safe Area 자동 대응

## 📅 개발 히스토리 (Change Log)
- **v0.6.0:** 핵심 게임 루프 완성, UI 시스템 고도화
- **v0.7.0 (예정):** Spring Boot 기반 랭킹 시스템 연동
