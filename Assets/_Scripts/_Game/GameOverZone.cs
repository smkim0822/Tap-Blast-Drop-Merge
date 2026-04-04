using UnityEngine;
using System.Collections.Generic;

public class GameOverZone : MonoBehaviour
{
    [Header("Visuals (시각 효과)")]
    [Tooltip("위험 구역을 표시할 선 (스프라이트 렌더러)")]
    public SpriteRenderer lineRenderer;
    public Color normalColor = new Color(1, 1, 1, 0.2f);
    public Color warningColor = Color.red;

    [Header("Logic Settings (판정 설정)")]
    [Tooltip("스택이 구역에 머물 수 있는 최대 허용 시간")]
    public float limitTime = 3.0f;

    private float timer = 0f;
    private float blinkPhase = 0f;      // 깜빡임 전용 내부 시계
    private bool isGameOver = false;    // 게임 오버 중복 실행 및 재시작 버그 방지

    // 풀링 시스템 대응: 구역 내 객체를 추적하는 리스트
    private List<GameObject> stackList = new List<GameObject>();

    // 옵저버 구독: 오브젝트가 켜질 때 새 게임 시작 이벤트를 듣기 시작
    void OnEnable()
    {
        GameEvents.OnGameStart += ResetStatus;
    }

    // 옵저버 해제: 에러 방지를 위해 꺼질 때 구독 해제
    void OnDisable()
    {
        GameEvents.OnGameStart -= ResetStatus;
    }

    /// <summary>
    /// 게임 시작 시 초기화
    /// </summary>
    private void ResetStatus()
    {
        isGameOver = false;
        timer = 0f;
        blinkPhase = 0f;
        stackList.Clear();

        if (lineRenderer != null)
        {
            lineRenderer.color = normalColor;
        }
    }

    void Update()
    {
        if (isGameOver) return;

        // 파괴된 공 정리
        for (int i = stackList.Count - 1; i >= 0; i--)
        {
            if (stackList[i] == null || !stackList[i].activeInHierarchy)
            {
                stackList.RemoveAt(i);
            }
        }

        // 상태 판별
        bool isDanger = stackList.Count > 0;

        if (isDanger)
        {
            // 위험 시간 누적
            timer += Time.deltaTime;

            // 깜빡임 속도 제어
            float currentHz = Mathf.Lerp(1.5f, 12f, timer / limitTime);

            // 페이즈 누적
            blinkPhase += Time.deltaTime * currentHz;

            // 시간에 주파수를 곱해 알파값 계산
            float alpha = (Mathf.Sin(blinkPhase * Mathf.PI * 2f) + 1f) * 0.5f;
            float finalAlpha = Mathf.Lerp(0.3f, 1f, alpha);

            if (lineRenderer != null)
            {
                Color c = warningColor;
                c.a = finalAlpha;
                lineRenderer.color = c;
            }

            if (timer > limitTime)
            {
                TriggerGameOver();
            }
        }
        else
        {
            // 위험 구역에서 벗어나면 타이머 및 페이즈 초기화
            timer = 0f;
            blinkPhase = 0f;

            if (lineRenderer != null)
            {
                Color c = normalColor;
                lineRenderer.color = Color.Lerp(lineRenderer.color, c, Time.deltaTime * 5f);
            }
        }
    }

    private void TriggerGameOver()
    {
        isGameOver = true;
        timer = 0f;
        blinkPhase = 0f;
        stackList.Clear();
        
        GameEvents.OnGameOver?.Invoke();
    }

    // 닿으면 명단에 추가
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isGameOver) return;

        if (other.CompareTag("Stack"))
        {
            if (!stackList.Contains(other.gameObject))
            {
                stackList.Add(other.gameObject);
            }
        }
    }

    // 나가면 명단에서 삭제
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Stack"))
        {
            stackList.Remove(other.gameObject);
        }
    }
}