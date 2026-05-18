# OCI Always Free Keep-Alive

랭킹 서버가 돌고 있는 **Oracle Cloud Infrastructure (OCI) Always Free 컴퓨트 인스턴스**가 유휴(idle)로 분류돼 회수되지 않도록 유지하기 위한 운영 메모입니다.

## 1. 회수 정책 요약

Oracle 공식 문서 기준, Always Free 컴퓨트 인스턴스는 **연속 7일** 동안 아래 조건을 **모두** 충족하면 idle로 간주됩니다.

| 항목 | 임계치 | 비고 |
|------|--------|------|
| CPU 95퍼센타일 이용률 | < 20% | 모든 shape |
| 네트워크 이용률 | < 20% | 모든 shape |
| 메모리 이용률 | < 20% | `VM.Standard.A1.Flex` 한정 |

현재 본 프로젝트의 인스턴스는 `VM.Standard.E2.1.Micro`(AMD)이므로 **CPU + 네트워크** 두 조건만 신경 쓰면 됩니다.

흐름: 7일 idle → 경고 메일 → 약 2일 뒤 자동 stop → 누적 비활성이 더 길어지면 영구 삭제 안내.

> 출처: [Always Free Resources — Reclamation of Idle Compute Instances](https://docs.oracle.com/iaas/Content/FreeTier/freetier_topic-Always_Free_Resources.htm#computereclaim)

## 2. 본 저장소가 하는 일

`.github/workflows/keep-oci-alive.yml`

- **트리거**: `cron: "*/10 * * * *"` (10분 간격) + `workflow_dispatch`(수동 실행)
- **동작**: `curl`로 랭킹 서버의 `GET /api/health`를 호출하고 응답이 `200 / "ok"`인지 검증
- **실패 처리**: 호출 자체가 발생했으면 idle 카운터 목적은 달성된 셈이므로 워크플로 종료 코드는 **항상 성공(0)**. 다만 응답이 비정상이면 `::warning::`으로 Actions UI에 노란색 로그를 남김 → 매번 알림이 시끄럽게 오지 않으면서도 이력은 추적 가능.
- **헬스체크 URL**: 저장소 **Variables**의 `OCI_HEALTH_URL`이 있으면 그 값을 사용하고, 없으면 `ServerConfig.cs`의 운영 폴백 주소를 사용. 도메인/HTTPS로 옮긴 뒤에는 Variables만 갱신하면 됨.

### 셋업 (이미 PR 머지된 상태 기준)
1. GitHub 저장소 → Settings → Secrets and variables → Actions → **Variables** 탭
2. `New repository variable`
3. Name: `OCI_HEALTH_URL`, Value: `https://<도메인 또는 IP>/api/health` (HTTPS 권장)
4. `Actions` 탭 → `keep-oci-alive` → `Run workflow`로 한 번 수동 실행해 로그 확인

> 주의: 헬스체크 URL은 응답 본문이 정확히 `ok`인 엔드포인트여야 합니다(현재 Spring Boot 측 구현이 그렇습니다). 만약 형식이 바뀌면 workflow의 검증 조건(`last_body = "ok"`)도 함께 갱신해야 합니다.

## 3. 한계와 보조 안전망

GitHub Actions의 `schedule` cron은 **정확한 시각을 보장하지 않습니다**(러너 부하에 따라 수분~수십분 밀릴 수 있음). 또한 GitHub Actions 자체가 장애 중이면 핑이 멈춥니다. 회수 위험을 0으로 줄이려면 아래 중 하나 이상을 같이 운용하는 것을 권장합니다.

### 3-1. OCI Monitoring 알람 (강력 권장, 5분이면 끝)

조기 경보용입니다. 인스턴스가 실제 idle로 빠지기 전에 메일을 받습니다.

1. OCI Console → Observability & Management → Monitoring → **Alarm Definitions** → `Create Alarm`
2. **Metric description**
   - Compartment: 인스턴스가 있는 컴파트먼트
   - Metric namespace: `oci_computeagent`
   - Metric name: `CpuUtilization`
   - **Interval: `1d`** (= 1 day) — 한 데이터 포인트가 **24시간 윈도우로 집계**되도록
   - Statistic: `Mean` (또는 `P95`)
3. **Trigger rule**
   - Operator: `less than`
   - Value: `15` (%) — Oracle 임계치(20%)보다 5%p 보수적으로
   - **Trigger Delay Minutes: `60`** (1~1440 사이 아무 값이나 OK)

> OCI의 `pendingDuration`(콘솔의 "Trigger Delay Minutes") 허용 범위는 **1~1440분(최대 24시간)** 입니다. 6일 같은 큰 값을 직접 넣을 수 없습니다(`PT144H must be within [PT1M, PT24H]` 오류). 대신 **Interval을 `1d`로 길게 잡으면 한 데이터 포인트 자체가 24시간 평균**이라, 짧은 Trigger Delay만으로도 "최근 하루 평균 사용량이 낮음" 조건이 곧바로 발화합니다. 회수 7일 누적 정책보다 한참 빨라 안전 마진이 충분합니다.

4. **Destinations**: `Notifications` 토픽 만들어 본인 이메일 구독

이러면 keep-alive가 어떤 이유로 끊겨 1일 평균 CPU가 15% 미만으로 떨어지는 즉시 메일이 옵니다 — 회수까지 며칠 여유.

### 3-2. 외부 모니터링 핑 (선택)

GitHub Actions와 독립된 두 번째 경로를 두고 싶을 때 유용합니다.

- 무료 옵션: **UptimeRobot**, **BetterStack(Better Uptime)**, **Cron-job.org** 등
- 5분 간격 HTTPS GET으로 `/api/health` 등록
- 응답 본문 매칭(`Keyword exists: ok`) 켜두면 진짜 서비스 다운도 같이 탐지

### 3-3. 인스턴스 내부 가벼운 워크로드 (선택, RankingServer 레포에서 진행)

호스트 안에서 외부 호출 없이 CPU/메모리 카운터를 들썩이게 하는 방법입니다. 핸들링 트래픽이 거의 없는 새벽 시간대를 메우는 용도로 유효합니다.

- 시스템 `cron`으로 5~10분마다 짧은 `sysbench`/`yes`/`stress-ng` 같은 단순 부하를 1~5초만 돌리기
- 또는 Spring Boot 안에 `@Scheduled`로 가벼운 self-job (DB ping, 캐시 워밍 등) 등록

이 작업은 별도 레포(`romantic02/RankingServer`)에서 진행됩니다 — 본 문서에서는 위치 표시만 하고 실제 구현은 그쪽 PR에서 다룹니다.

## 4. 운영 체크리스트

| 항목 | 빈도 | 비고 |
|------|------|------|
| Actions → `keep-oci-alive` 최근 실행 로그 | 주 1회 | 연속 warning이 보이면 서버/네트워크 점검 |
| OCI Console 로그인 | 월 1~2회 | 비활성 계정 회수 트리거 회피 |
| 결제 수단(카드) 유효성 | 만료 전 | 카드 만료되면 계정이 닫힐 수 있음 |
| OCI Monitoring 알람 발화 메일 | 즉시 대응 | 6일 누적 시 자동 발송 |

## 5. 변경 시 주의

- 헬스체크 URL/응답 포맷을 바꾸면 **워크플로**(`keep-oci-alive.yml`)와 **`ServerConfig.cs`** 양쪽을 함께 점검.
- 워크플로의 cron 간격을 늘리면(예: 30분 이상) idle 임계까지 여유가 줄어듭니다. 안전 마진을 고려해 최대 15분 이내를 권장.
- 워크플로 내부에서 시크릿이 필요한 인증 호출은 하지 않습니다. 본 워크플로는 공개된 헬스 엔드포인트만 호출하도록 설계되어 있습니다.
