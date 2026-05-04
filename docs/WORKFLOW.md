# 개발 워크플로

기본 브랜치는 **`develop`** 입니다. 기능·수정·디자인 등 모든 변경은 **이슈 → 브랜치 → PR → 병합** 순서를 따릅니다.

## 1. 작업 정의

- 무엇을 바꾸는지, 완료 조건이 무엇인지 한 줄로 정리합니다.
- GitHub에서 **이슈**를 생성하고, 템플릿(`작업 이슈`)을 채웁니다.

## 2. 브랜치

- **`develop`** 최신 상태에서 분기합니다.
- 이름 규칙 (이슈 번호가 있으면 포함 권장):

  | 접두사 | 용도 |
  |--------|------|
  | `feature/` | 기능 추가 |
  | `fix/` | 버그 수정 |
  | `design/` | UI·에셋·씬 등 비주얼 |
  | `refactor/` | 동작 유지 리팩터 |
  | `chore/` | 빌드·설정·문서 등 기타 |

- 예: `feature/42-ingame-title`, `fix/ranking-sync`, `design/main-panel`

## 3. 구현·커밋

- 커밋 메시지는 저장소 루트의 **`.gitmessage`** 형식을 따릅니다.
- 로컬에서 한 번만 설정하면 편합니다:

  ```bash
  git config commit.template .gitmessage
  ```

- 이슈와 연결할 때는 본문에 `Closes #이슈번호` 를 넣습니다 (PR에서 넣어도 됩니다).

## 4. Pull Request

- **베이스:** `develop`
- PR 생성 시 저장소의 **PR 템플릿**을 채웁니다.
- 리뷰 후 **`develop`** 로 squash merge 또는 merge commit 정책은 팀 합의에 따릅니다.

## 5. main과의 관계

- 릴리스·출시 타이밍에 **`develop` → `main`** 병합을 따로 정합니다.

## 관련 파일

| 파일 | 설명 |
|------|------|
| `.github/ISSUE_TEMPLATE/task.md` | 이슈 템플릿 |
| `.github/pull_request_template.md` | PR 템플릿 |
| `.gitmessage` | 커밋 메시지 템플릿 |
| `docs/COMMIT_CONVENTION.md` | 타입·예시 보충 |
