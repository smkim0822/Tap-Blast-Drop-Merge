using UnityEngine;
using System.Collections;

public class GrainLogic : MonoBehaviour
{
    // 출신 번호 (BombLogic에서 할당)
    public int batchId = -1;
    // 오디오 파일 변수
    public AudioClip popSound;
    // 파티클 프리팹 변수
    public ParticleSystem explosionEffect;

    [Header("Score Settings")]
    [Tooltip("점수 계산 배율 (크기 * 배율 = 기본 점수)")]
    public float scoreMultiplier = 1f;

    [Tooltip("배치 ID당 점수 증가율 (0.1 = 10%씩 증가, 0.01 = 1%씩 증가)")]
    public float scoreGrowthRate = 0.05f;

    [Header("UI Effects")]
    public GameObject floatingTextPrefab;

    private SpriteRenderer myRenderer;
    private bool isDestroying = false;  // 중복 파괴 방지 플래그

    void Start(){
        myRenderer = GetComponent<SpriteRenderer>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 이미 파괴 중이면 로직 무시
        if (isDestroying) return;

        // 다른 알갱이와 닿았을 때
        GrainLogic otherGrain = collision.gameObject.GetComponent<GrainLogic>();

        if (otherGrain != null)
        {
            // 상대방도 이미 파괴 중이면 무시 (중복 점수 방지)
            if (otherGrain.isDestroying) return;

            // 색상 비교
            if (IsColorMatch(myRenderer.color, otherGrain.GetComponent<SpriteRenderer>().color))
            {
                // 출신 비교
                if (this.batchId != otherGrain.batchId)
                {
                    // 둘 다 사라짐
                    DestroyGrain();
                    otherGrain.DestroyGrain();
                }
            }
        }
    }

    public void DestroyGrain(){
        if (isDestroying) return;   // 안전장치
        isDestroying = true;

        // 기본 점수 계산 (크기 * 기본 배율)
        float baseScore = transform.localScale.x * scoreMultiplier;

        // 성장 배율 계산 (1 + 진행도 * 성장률)
        float currentEra = BombLogic.globalBatchIdCounter;
        float growthFactor = 1.0f + (currentEra * scoreGrowthRate);

        // 최종 점수 계산
        int finalScore = Mathf.Max(1, Mathf.RoundToInt(baseScore * growthFactor));

        // 점수 추가
        if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(finalScore);
        
        // 사운드 재생
        if (popSound != null)
        {
            AudioSource.PlayClipAtPoint(popSound, transform.position, 1.0f);
        }

        // 진동
        // Handheld.Vibrate();
        
        // 파티클 생성 및 색상 적용
        if (explosionEffect != null)
        {
            ParticleSystem ps = Instantiate(explosionEffect, transform.position, Quaternion.identity);

            var main = ps.main;

            main.startColor = myRenderer.color;
            Destroy(ps.gameObject, main.startLifetime.constantMax + 0.1f);
        }

        // 플로팅 텍스트 생성
        if (floatingTextPrefab != null)
        {
            // 프리팹 생성
            GameObject go = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity);

            // 텍스트 내용 설정 ("+점수")
            FloatingText floatingText = go.GetComponent<FloatingText>();
            if (floatingText != null)
            {
                // 텍스트 설정
                floatingText.SetText("+" + finalScore, Color.yellow);

                // 공 색 동기화
                floatingText.SetText("+" + finalScore,  myRenderer.color);
            }
        }

        // 물리 비활성화 및 애니메이션 시작
        
        // 더 이상 물리 충돌하지 않도록 설정
        if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.simulated = false;   // 물리 연산 중단
            rb.linearVelocity = Vector2.zero;     // 제자리에 멈춤
        }

        // 바로 삭제하지 않고 코루틴 시작
        StartCoroutine(CoPopAnimation());
    }

    // 젤리 팝 애니메이션 코루틴
    IEnumerator CoPopAnimation()
    {
        float duration = 0.15f;     // 애니메이션 시간
        float timer = 0f;

        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.5f;     // 커짐 (텐션)

        // 커지기 (Tension)
        while (timer < duration * 0.5f)
        {
            timer += Time.deltaTime;
            float progress = timer / (duration * 0.5f);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            yield return null;
        }

        // 작아지기 (Pop)
        timer = 0f;
        while (timer < duration * 0.5f)
        {
            timer += Time.deltaTime;
            float progress = timer / (duration * 0.5f);
            transform.localScale = Vector3.Lerp(targetScale, Vector3.zero, progress);
            yield return null;
        }

        // 삭제
        Destroy(gameObject);
    }

    bool IsColorMatch(Color c1, Color c2)
    {
        return (Mathf.Abs(c1.r - c2.r) + Mathf.Abs(c1.g - c2.g) + Mathf.Abs(c1.b - c2.b)) < 0.1f;
    }
}