using UnityEngine;

public class BombLogic : MonoBehaviour
{
    [Header("Explosion Settings (폭발 설정)")]
    [Tooltip("폭발 시 흩뿌려질 알갱이 프리팹")]
    public GameObject grainPrefab;
    [Tooltip("생성할 알갱이 개수")]
    public int grainCount = 15;
    [Tooltip("알갱이가 퍼져나가는 물리적인 힘")]
    public float explosionForce = 3.0f;

    [Header("Size Settings")]
    public float minSize = 0.1f;        // 최소 크기
    public float maxSize = 0.6f;        // 최대 크기

    private bool hasExploded = false;   // 중복 폭발 방지용 플래그
    private SpriteRenderer myRenderer;
    private Sprite myElementSprite;
    private Color myEffectColor = Color.white;

    void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
    }

    // 풀링 최적화: 오브젝트 풀에서 꺼내져 재활용될 때마다 폭발 상태 초기화
    void OnEnable()
    {
        hasExploded = false;
    }

    /// <summary>
    /// 폭탄이 생성될 때 초기 속성 세팅
    /// </summary>
    /// <param name="elementSprite"></param>
    /// <param name="effectColor"></param>
    public void Initialize(Sprite elementSprite, Color effectColor)
    {
        myElementSprite = elementSprite;
        myEffectColor = effectColor;

        if (myRenderer != null)
        {
            myRenderer.sprite = myElementSprite;
            myRenderer.color = Color.white;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 이미 터진 폭탄이면 물리 충돌 무시
        if (hasExploded) return;

        // "Stack" 태그(바닥, 다른 공)에 닿으면 폭발
        if (collision.gameObject.CompareTag("Stack"))
        {
            Explode();
        }
    }

    /// <summary>
    /// 폭발 및 공 분열 로직
    /// </summary>
    void Explode()
    {
        hasExploded = true;     // 중복 폭발 방지

        // 공 스프라이트 결정
        Sprite spriteToPass = (myElementSprite != null) ? myElementSprite : myRenderer.sprite;

        // 고유 번호 발급
        int myBatchId = GameplayBatchSequence.Next();

        for (int i = 0; i < grainCount; i++)
        {
            // 랜덤 위치 생성
            Vector3 randomPos = transform.position + (Vector3)(Random.insideUnitCircle * 0.4f);

            // 풀링
            GameObject grainObj = PoolService.Spawn(grainPrefab, randomPos, Quaternion.identity);

            // 크기 랜덤 설정
            float randomScale = Random.Range(minSize, maxSize);
            grainObj.transform.localScale = new Vector3(randomScale, randomScale, 1f);

            // 이미지 설정
            if (grainObj.TryGetComponent(out SpriteRenderer grainRenderer))
            {
                grainRenderer.sprite = spriteToPass;
                grainRenderer.color = Color.white;
            }

            // 태그 설정
            grainObj.tag = "Stack";

            // ID 부여
            if (grainObj.TryGetComponent(out GrainLogic grainLogic))
            {
                grainLogic.SetupGrain(myBatchId, myEffectColor);
            }

            // 퍼지는 힘
            if (grainObj.TryGetComponent(out Rigidbody2D rb))
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                rb.AddForce(randomDir * explosionForce, ForceMode2D.Impulse);
            }
        }

        // 폭탄 반환
        PoolService.Despawn(gameObject);
    }
}