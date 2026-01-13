using UnityEngine;
using UnityEngine.EventSystems;

public class DragSpawner : MonoBehaviour
{
    public GameObject bombPrefab;
    public Color[] grainColors;
    public float xRange =  2.5f;

    // 입력 대기시간
    public float spawnCooldown = 0.8f;
    private float lastSpawnTime = 0f;

    private bool isDragging = false;

    // 시각 처리
    private SpriteRenderer myRenderer;
    private Color nextColor;

    void Start()
    {
        lastSpawnTime = -spawnCooldown;

        myRenderer = GetComponent<SpriteRenderer>();

        PrepareNextBomb();
        if (myRenderer != null) myRenderer.enabled = true;
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // 대기시간 처리
        if (Time.time >= lastSpawnTime + spawnCooldown && myRenderer.enabled == false)
        {
            PrepareNextBomb();
            myRenderer.enabled = true;
        }

        if (myRenderer.enabled == false) return;

        // 드래그 로직 (터치 위치를 월드 좌표로 반환)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        mousePos.y = transform.position.y;

        // 범위 제한
        mousePos.x = Mathf.Clamp(mousePos.x, -xRange, xRange);

        // 드래그 시작
        if (Input.GetMouseButton(0))
        {
            isDragging = true;
            transform.position = mousePos;
        }

        // 드래그 중
        if (Input.GetMouseButton(0) && isDragging)
        {
            transform.position = mousePos;
        }

        // 드래그 끝
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            DropBomb();
        }
    }

    void DropBomb()
    {
        // 발사 시간 기록
        lastSpawnTime = Time.time;

        GameObject newBomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);

        if (grainColors.Length > 0)
        {
            SpriteRenderer sr = newBomb.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = nextColor;
            }
        }

        if (myRenderer != null)
        {
            myRenderer.enabled = false;
        }
    }

    void PrepareNextBomb()
    {
        if (grainColors.Length > 0)
        {
            nextColor = grainColors[Random.Range(0, grainColors.Length)];

            if (myRenderer != null)
            {
                myRenderer.color = nextColor;
            }
        }
    }
}
