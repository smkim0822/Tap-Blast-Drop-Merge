using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    public struct ElementData
    {
        public string name;
        public Sprite sprite;
        public Color color;
    }

    [Header("Settings (기본 설정)")]
    [Tooltip("생성할 폭탄 프리팹")]
    public GameObject bombPrefab;

    [Tooltip("재장전 시간")]
    public float cooldownTime = 0.5f;

    [Tooltip("스포너에서 생성되는 폭탄의 스케일 배율(플레이 영역에 맞게 조정)")]
    [Range(0.5f, 1.2f)]
    public float bombScaleMultiplier = 0.85f;

    [Header("Play area (X축 제한)")]
    [Tooltip("플레이 영역 좌측 벽의 Collider2D (비어 있으면 이름 \"Left Wall\"으로 찾음)")]
    [SerializeField] private Collider2D leftWall;

    [Tooltip("플레이 영역 우측 벽의 Collider2D (비어 있으면 이름 \"Right Wall\"으로 찾음)")]
    [SerializeField] private Collider2D rightWall;

    [Tooltip("좌우 벽을 찾지 못했을 때만 사용하는 X 반경 (절대값, -xLimit ~ xLimit)")]
    public float xLimit = 2.0f;

    [Header("Elements (폭탄 데이터)")]
    public ElementData[] elements;

    // 내부 상태 변수
    private Camera mainCam;
    private bool isDragging = false;
    private bool isReloading = false;

    // 컴포넌트 및 캐싱 변수
    private SpawnerQueue queue;
    private SpawnerVisuals visuals;

    void Awake()
    {
        mainCam = Camera.main;
        visuals = GetComponent<SpawnerVisuals>();
        if (visuals == null)
        {
            visuals = gameObject.AddComponent<SpawnerVisuals>();
        }
        queue = new SpawnerQueue(elements, visuals.PreviewCount);

        ResolveWallColliders();
    }

    /// <summary>
    /// 인스펙터에 벽이 없으면 씬의 Left Wall / Right Wall에서 Collider2D를 찾습니다.
    /// </summary>
    void ResolveWallColliders()
    {
        if (leftWall == null)
        {
            GameObject go = GameObject.Find("Left Wall");
            if (go != null) go.TryGetComponent(out leftWall);
        }
        if (rightWall == null)
        {
            GameObject go = GameObject.Find("Right Wall");
            if (go != null) go.TryGetComponent(out rightWall);
        }
    }

    /// <summary>
    /// 스포너 중심이 머물 수 있는 월드 X 구간 [minX, maxX].
    /// 좌우 벽 안쪽 면에서 다시 폭탄 반경만큼 안쪽으로 들여, 폭탄 끝이 벽에 닿도록 한다.
    /// </summary>
    void GetHorizontalClampBounds(out float minX, out float maxX)
    {
        float outerMin, outerMax;
        if (leftWall != null && rightWall != null)
        {
            float innerLeft = leftWall.bounds.max.x;
            float innerRight = rightWall.bounds.min.x;
            outerMin = Mathf.Min(innerLeft, innerRight);
            outerMax = Mathf.Max(innerLeft, innerRight);
        }
        else
        {
            outerMin = -xLimit;
            outerMax = xLimit;
        }

        float pad = GetBombHalfWidth();
        minX = outerMin + pad;
        maxX = outerMax - pad;

        // 폭탄이 영역보다 큰 비정상 케이스: 중앙으로 고정
        if (minX > maxX)
        {
            float mid = (outerMin + outerMax) * 0.5f;
            minX = maxX = mid;
        }
    }

    /// <summary>
    /// 스폰될 폭탄의 월드 기준 가로 반경.
    /// CircleCollider2D가 있으면 그 반경을, 없으면 다른 Collider2D의 X extents를 사용한다.
    /// </summary>
    float GetBombHalfWidth()
    {
        if (bombPrefab == null) return 0f;

        float localHalfWidth = 0f;
        if (bombPrefab.TryGetComponent(out CircleCollider2D circle))
        {
            localHalfWidth = circle.radius;
        }
        else if (bombPrefab.TryGetComponent(out Collider2D anyCol))
        {
            localHalfWidth = anyCol.bounds.extents.x;
        }

        Vector3 prefabScale = bombPrefab.transform.localScale;
        return localHalfWidth * Mathf.Abs(prefabScale.x) * Mathf.Abs(bombScaleMultiplier);
    }

    void Start()
    {
        queue.Reset();
        visuals.RenderQueue(queue.CurrentElement, queue.NextElements, false);
    }

    void Update()
    {
        if (Time.timeScale == 0f || isReloading || IsPointerOverUI())
        {
            return;
        }

        HandleInput();

        // 드래그 중일 때만 표시선 표시
        if (isDragging) 
        {
            visuals.DrawGuideLine(transform.position, queue.CurrentElement.color);
        }
        else if (visuals.IsGuideEnabled())
        {
            visuals.SetGuideEnabled(false);
        }
    }

    /// <summary>
    /// 현재 터치 이벤트가 UI 요소 위에서 발생했는지 판별
    /// </summary>
    /// <returns></returns>
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        // 클릭
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        // 터치
        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        return false;
    }

    /// <summary>
    /// 사용자 입력 처리
    /// </summary>
    void HandleInput()
    {
        // 터치 시작
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            visuals.SetGuideEnabled(true);
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
            visuals.SetGuideEnabled(false);
            StartCoroutine(DropBombRoutine());
        }
    }

    /// <summary>
    /// 스포너의 위치를 마우스 좌표에 맞춰 X축으로만 이동
    /// </summary>
    private void MoveSpawner()
    {
        // 마우스 화면 좌표 -> 월드 좌표로 변환
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        GetHorizontalClampBounds(out float minX, out float maxX);
        float clampedX = Mathf.Clamp(mousePos.x, minX, maxX);

        // 이동
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }

    /// <summary>
    /// 폭탄을 투하하고 재장전 대기 시간을 갖는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator DropBombRoutine()
    {
        // 폭탄 생성
        if (bombPrefab != null)
        {
            GameObject newBomb = PoolService.Spawn(bombPrefab, transform.position, Quaternion.identity);
            // 풀링 오브젝트는 이전 스케일이 남을 수 있으므로, 프리팹 기준 스케일로 재설정 후 배율 적용
            Vector3 baseScale = bombPrefab.transform.localScale;
            newBomb.transform.localScale = new Vector3(baseScale.x * bombScaleMultiplier, baseScale.y * bombScaleMultiplier, baseScale.z);
            visuals.PlaySpawnAnimation(newBomb.transform);

            if (newBomb.TryGetComponent(out BombLogic bombLogic))
            {
                bombLogic.Initialize(queue.CurrentElement.sprite, queue.CurrentElement.color);
            }
        }

        // 재장전 시작
        isReloading = true;
        visuals.SetSpawnerVisible(false);

        // 장전 대기시간
        yield return new WaitForSeconds(cooldownTime);

        // 재장전 완료
        queue.Advance();
        visuals.RenderQueue(queue.CurrentElement, queue.NextElements, true);
        visuals.SetSpawnerVisible(true);
        isReloading = false;
    }

    /// <summary>
    /// 스포너 상태 강제 초기화
    /// </summary>
    public void ResetSpawner()
    {
        StopAllCoroutines();

        isDragging = false;
        isReloading = false;

        GetHorizontalClampBounds(out float minX, out float maxX);
        float resetX = Mathf.Clamp(0f, minX, maxX);
        transform.position = new Vector3(resetX, transform.position.y, transform.position.z);

        visuals.ResetVisualState();
        queue.Reset();
        visuals.RenderQueue(queue.CurrentElement, queue.NextElements, false);
    }
}