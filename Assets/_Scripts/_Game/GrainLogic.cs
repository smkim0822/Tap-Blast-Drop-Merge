using UnityEngine;
using System.Collections;

public class GrainLogic : MonoBehaviour
{
    // 출신 번호 (BombLogic에서 할당)
    public int batchId = -1;
    // 오디오 파일 변수
    public AudioClip popSound;
    // 파티클 프리팹 변수
    public GameObject explosionEffectPrefab;

    [Header("Score Settings")]
    [Tooltip("점수 계산 배율 (크기 * 배율 = 기본 점수)")]
    public float scoreMultiplier = 1f;

    [Tooltip("배치 ID당 점수 증가율 (0.1 = 10%씩 증가, 0.01 = 1%씩 증가)")]
    public float scoreGrowthRate = 0.05f;

    [Header("UI Effects")]
    public GameObject floatingTextPrefab;

    private SpriteRenderer myRenderer;
    private Collider2D myCollider;
    private Rigidbody2D myRb;

    private bool isDestroying = false;
    private Color effectColor = Color.white;
    private Vector3 originalScale;

    void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();
        myRb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
    }

    // 풀링 재활용 시 상태 초기화
    void OnEnable()
    {
        isDestroying = false;
        if (myCollider != null)
        {
            myCollider.enabled = true;
        }
        if (myRb != null)
        {
            myRb.simulated = true;
            myRb.linearVelocity = Vector2.zero;
        }
    }

    public void SetupGrain(int id, Color color)
    {
        batchId = id;
        effectColor = color;
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

            // 이미지 비교
            if (IsSpriteMatch(myRenderer, otherGrain.GetComponent<SpriteRenderer>()))
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
        if (isDestroying) return;
        isDestroying = true;

        // 기본 점수 계산 (크기 * 기본 배율)
        float baseScore = transform.localScale.x * scoreMultiplier;

        // 성장 배율 계산 (1 + 진행도 * 성장률)
        float currentEra = BombLogic.globalBatchIdCounter;
        float growthFactor = 1.0f + (currentEra * scoreGrowthRate);

        // 최종 점수 계산
        int finalScore = Mathf.Max(1, Mathf.RoundToInt(baseScore * growthFactor));

        // 이벤트 발송
        GameEvents.OnScoreAdded?.Invoke(finalScore);
        if (popSound != null)
        {
            GameEvents.OnPlaySFX?.Invoke(popSound, 1.0f);
        }

        // 파티클 풀링 생성
        if (explosionEffectPrefab != null)
        {
            GameObject psObj = PoolManager.Instance.Spawn(explosionEffectPrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = psObj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = effectColor;
                ps.Play();
                PoolManager.Instance.Despawn(psObj, main.startLifetime.constantMax + 0.1f);
            }
        }

        // 플로팅 텍스트 풀링 생성
        if (floatingTextPrefab != null)
        {
            GameObject go = PoolManager.Instance.Spawn(floatingTextPrefab, transform.position, Quaternion.identity);
            FloatingText floatingText = go.GetComponent<FloatingText>();
            if (floatingText != null)
            {
                floatingText.SetText("+" + finalScore, effectColor);
            }
        }

        // 물리 연산 즉시 중단
        if (myCollider != null)
        {
            myCollider.enabled = false;
        }
        if (myRb != null)
        {
            myRb.simulated = false;
            myRb.linearVelocity = Vector2.zero;
        }

        StartCoroutine(CoPopAnimation());
    }

    // 애니메이션 코루틴
    IEnumerator CoPopAnimation()
    {
        float duration = 0.15f;     // 애니메이션 시간
        float timer = 0f;

        Vector3 baseScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.5f;     // 커짐 (텐션)

        // 커지기 (Tension)
        while (timer < duration * 0.5f)
        {
            timer += Time.deltaTime;
            float progress = timer / (duration * 0.5f);
            transform.localScale = Vector3.Lerp(baseScale, targetScale, progress);
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

        // 풀에 반환
        PoolManager.Instance.Despawn(gameObject);
    }

    bool IsSpriteMatch(SpriteRenderer r1, SpriteRenderer r2)
    {
        if (r1 == null || r2 == null) return false;
        return r1.sprite == r2.sprite;
    }
}