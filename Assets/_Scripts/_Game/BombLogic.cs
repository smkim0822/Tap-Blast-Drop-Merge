using UnityEngine;

public class BombLogic : MonoBehaviour
{
    [Header("Basic Settings")]
    public GameObject grainPrefab;  // 알갱이 프리팹
    public int grainCount = 15;     // 생성 개수
    public float explosionForce = 3.0f; // 퍼지는 힘

    [Header("Size Settings")]
    public float minSize = 0.1f;    // 최소 크기
    public float maxSize = 0.6f;    // 최대 크기

    // 폭탄 번호
    public static int globalBatchIdCounter = 0;

    private bool hasExploded = false;
    private SpriteRenderer myRenderer;
    private Sprite myElementSprite;
    private Color myEffectColor = Color.white;

    void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Sprite elementSprite, Color effectColor)
    {
        myElementSprite = elementSprite;
        myEffectColor = effectColor;

        if (myRenderer == null)
        {
            myRenderer = GetComponent<SpriteRenderer>();
        }

        if (myRenderer != null)
        {
            myRenderer.sprite = myElementSprite;
            myRenderer.color = Color.white;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;

        // "Stack" 태그(바닥, 다른 알갱이)에 닿으면 무조건 해체
        if (collision.gameObject.CompareTag("Stack"))
        {
            Explode();
        }
    }

    void Explode()
    {
        hasExploded = true;
        Sprite spriteToPass = (myElementSprite != null) ? myElementSprite : myRenderer.sprite;

        // 고유 번호 발급
        int myBatchId = globalBatchIdCounter++;

        for (int i = 0; i < grainCount; i++)
        {
            // 랜덤 위치 생성
            Vector3 randomPos = transform.position + (Vector3)(Random.insideUnitCircle * 0.4f);
            GameObject grain = Instantiate(grainPrefab, randomPos, Quaternion.identity);

            // 크기 랜덤 설정
            float randomScale = Random.Range(minSize, maxSize);
            grain.transform.localScale = new Vector3(randomScale, randomScale, 1f);

            // 이미지 설정
            SpriteRenderer grainRenderer = grain.GetComponent<SpriteRenderer>();
            if (grainRenderer != null)
            {
                grainRenderer.sprite = spriteToPass;
                grainRenderer.color = Color.white;
            }

            // 태그 설정
            grain.tag = "Stack";

            // ID 부여
            grain.GetComponent<GrainLogic>().SetupGrain(myBatchId, myEffectColor);

            // 퍼지는 힘
            Rigidbody2D rb = grain.GetComponent<Rigidbody2D>();
            if (rb != null){
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                rb.AddForce(randomDir * explosionForce, ForceMode2D.Impulse);
            }
        }

        // 폭탄 삭제
        Destroy(gameObject);
    }
}