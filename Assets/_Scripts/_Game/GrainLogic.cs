using UnityEngine;
using System.Collections;

public class GrainLogic : MonoBehaviour
{
    [Header("Basic Info (기본 정보)")]
    [Tooltip("출신 폭탄 번호 (BombLogic에서 할당됨)")]
    public int batchId = -1;

    [Header("Effects (이펙트)")]
    [SerializeField, HideInInspector] public AudioClip popSound;
    [SerializeField, HideInInspector] public GameObject explosionEffectPrefab;
    [SerializeField, HideInInspector] public GameObject floatingTextPrefab;

    [Header("Score Settings (점수 설정)")]
    [Tooltip("점수 계산 배율 (크기 * 배율 = 기본 점수)")]
    public float scoreMultiplier = 1f;
    [Tooltip("배치 ID당 점수 증가율 (0.05 = 5%씩 증가)")]
    public float scoreGrowthRate = 0.05f;

    private SpriteRenderer myRenderer;
    private Collider2D myCollider;
    private Rigidbody2D myRb;

    private bool isDestroying = false;
    private Color effectColor = Color.white;
    private GrainBurstEffects burstEffects;

    void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();
        myRb = GetComponent<Rigidbody2D>();
        burstEffects = GetComponent<GrainBurstEffects>();
        if (burstEffects == null)
        {
            burstEffects = gameObject.AddComponent<GrainBurstEffects>();
        }

        burstEffects.ConfigureIfMissing(popSound, explosionEffectPrefab, floatingTextPrefab);
    }

    // 풀링 시스템: 오브젝트가 활성화될 때마다 모든 상태 초기화
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
            myRb.angularVelocity = 0f;
        }
    }

    /// <summary>
    /// 폭발 시 공 기본 정보 세팅
    /// </summary>
    /// <param name="id"></param>
    /// <param name="color"></param>
    public void SetupGrain(int id, Color color)
    {
        batchId = id;
        effectColor = color;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 이미 파괴 중이면 로직 무시
        if (isDestroying) return;

        // 태그로 1차 필터링
        if (!collision.gameObject.CompareTag("Stack")) return;

        // 상대방 공 로직 가져오기
        if (collision.gameObject.TryGetComponent(out GrainLogic otherGrain))
        {
            // 상대방도 이미 파괴 중이면 무시 (중복 점수 방지)
            if (otherGrain.isDestroying) return;

            // 이미지 비교
            if (IsSpriteMatch(myRenderer, otherGrain.myRenderer))
            {
                // 출신 비교
                if (this.batchId != otherGrain.batchId)
                {
                    // 둘 다 사라짐
                    this.DestroyGrain();
                    otherGrain.DestroyGrain();
                }
            }
        }
    }

    /// <summary>
    /// 공 파괴, 점수 계산, 이펙트 생성, 애니메이션 실행 담당
    /// </summary>
    public void DestroyGrain(){
        if (isDestroying) return;
        isDestroying = true;    // 다중 충돌로 인한 중복 파괴 방지

        int finalScore = GrainScorePolicy.Calculate(batchId, transform.localScale.x, scoreMultiplier, scoreGrowthRate);

        // 이벤트 발송
        ScoreEvents.OnScoreAdded?.Invoke(finalScore);
        if (burstEffects != null)
        {
            burstEffects.Play(transform.position, effectColor, finalScore);
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
            myRb.angularVelocity = 0f;
        }

        StartCoroutine(CoPopAnimation());
    }

    // 애니메이션 코루틴
    IEnumerator CoPopAnimation()
    {
        float duration = 0.15f;     // 애니메이션 시간
        float halfDuration = duration * 0.5f;
        float timer = 0f;

        Vector3 startScale = transform.localScale;
        Vector3 targetScale = startScale * 1.5f;     // 커짐 (텐션)

        // 커지기 (Tension)
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / halfDuration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }

        timer = 0f;

        // 작아지기 (Pop)
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / halfDuration;
            transform.localScale = Vector3.Lerp(targetScale, Vector3.zero, progress);
            yield return null;
        }

        // 풀에 반환
        PoolService.Despawn(gameObject);
    }

    // 스프라이트 이미지 동일 여부 확인 헬퍼 함수
    private bool IsSpriteMatch(SpriteRenderer r1, SpriteRenderer r2)
    {
        if (r1 == null || r2 == null) return false;
        return r1.sprite == r2.sprite;
    }
}