using UnityEngine;
using System.Collections;

public class DragSpawner : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("생성할 폭탄 프리팹")]
    public GameObject bombPrefab;

    [Tooltip("화면 좌우 이동 제한 범위 (X축)")]
    public float xLimit = 2.6f;

    [Tooltip("재장전 시간")]
    public float cooldownTime = 0.5f;

    [Header("Colors")]
    public Color[] bombColors;
    
    [Header("Guide Line")]
    [Tooltip("라인 렌더러 컴포넌트 연결")]
    public LineRenderer guideLine;

    [Tooltip("가이드 라인이 바닥에 닿을 바닥의 Y좌표")]
    public float groundY = -4.5f;

    [Tooltip("점선 간격 조절")]
    public float lineTextureScale = 1;

    private Camera mainCam;
    private bool isDragging = false;
    private bool isReloading = false;

    private SpriteRenderer spawnerRenderer;
    private Color nextColor;

    void Start()
    {
        mainCam = Camera.main;
        spawnerRenderer = GetComponent<SpriteRenderer>();

        // 라인 렌더러 초기화
        if (guideLine == null)
        {
            guideLine = GetComponent<LineRenderer>();
        }
        guideLine.positionCount = 2;
        guideLine.enabled = false;

        guideLine.textureMode = LineTextureMode.Tile;

        PickNewColor();
    }

    void Update()
    {
        if (isReloading) return;

        HandleInput();

        // 드래그 중일 때만 가이드 라인 표시
        if (isDragging)
        {
            DrawGuideLine();
        }
        else
        {
            guideLine.enabled = false;
        }
    }

    void HandleInput()
    {
        // 터치 시작
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            guideLine.enabled = true;
        }

        // 드래그 중
        if (Input.GetMouseButton(0) && isDragging)
        {
            MoveSpawner();
        }

        // 터치 끝
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            guideLine.enabled = false;
            StartCoroutine(DropBombRoutine());
        }
    }

    void MoveSpawner()
    {
        // 마우스 화면 좌표 -> 월드 좌표로 변환
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        // X축 이동 제한 (Clamp)
        float clampedX = Mathf.Clamp(mousePos.x, -xLimit, xLimit);

        // 이동
        transform.position = new Vector2(clampedX, transform.position.y);
    }

    void DrawGuideLine()
    {
        // 시작점: 스포너의 현재 위치
        guideLine.SetPosition(0, transform.position);

        // 끝점: X는 스포너와 같고, Y는 바닥까지
        Vector3 endPos = new Vector3(transform.position.x, groundY, 0);
        guideLine.SetPosition(1, endPos);

        float distance = Vector3.Distance(transform.position, endPos);
        guideLine.material.mainTextureScale = new Vector2(distance * lineTextureScale, 1);
    }

    IEnumerator DropBombRoutine()
    {
        // 폭탄 생성
        if (bombPrefab != null)
        {
            GameObject newBomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);

            SpriteRenderer bombRenderer = newBomb.GetComponent<SpriteRenderer>();
            if (bombRenderer != null)
            {
                bombRenderer.color = nextColor;
                Color fixedColor = nextColor;
                fixedColor.a = 1f;
                bombRenderer.color = fixedColor;
            }
        }

        // 재장전 시작
        isReloading = true;
        spawnerRenderer.enabled = false;

        // 장전 대기시간
        yield return new WaitForSeconds(cooldownTime);

        // 재장전 완료
        PickNewColor();
        spawnerRenderer.enabled = true;
        isReloading = false;
    }

    void PickNewColor()
    {
        if (bombColors.Length > 0)
        {
            int randomIndex = Random.Range(0, bombColors.Length);
            nextColor = bombColors[randomIndex];

            // 스포너 색상 변경
            if (spawnerRenderer != null)
            {
                spawnerRenderer.color = nextColor;
            }

            // 가이드 라인 색상 동기화
            if (guideLine != null)
            {
                guideLine.startColor = nextColor;

                Color fadedColor = nextColor;
                fadedColor.a = 0.3f;
                guideLine.endColor = fadedColor;
            }
        }
    }
}