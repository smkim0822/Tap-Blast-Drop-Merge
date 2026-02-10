using UnityEngine;
using System.Collections.Generic;

public class GameOverZone : MonoBehaviour
{
    [Header("Visuals")]
    public SpriteRenderer lineRenderer;
    public Color normalColor = new Color(1, 1, 1, 0.2f);
    public Color warningColor = Color.red;

    [Header("Logic Settings")]
    public float limitTime = 1.0f;

    private float timer = 0f;
    private float blinkPhase = 0f;  //  깜빡임 전용 내부 시계

    private List<GameObject> stackList = new List<GameObject>();

    void Update()
    {
        // 파괴된 공 정리
        stackList.RemoveAll(item => item == null);

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
            float alpha = (Mathf.Sin(blinkPhase * Mathf.PI * 2) + 1f) * 0.5f;
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
        timer = 0f;
        blinkPhase = 0f;
        stackList.Clear();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }

    // 닿으면 명단에 추가
    void OnTriggerEnter2D(Collider2D other)
    {
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