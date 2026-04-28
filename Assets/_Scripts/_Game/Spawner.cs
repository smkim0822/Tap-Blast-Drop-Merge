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

    [Tooltip("화면 좌우 이동 제한 범위 (X축)")]
    public float xLimit = 2.0f;

    [Tooltip("재장전 시간")]
    public float cooldownTime = 0.5f;

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

        // X축 이동 제한 (Clamp)
        float clampedX = Mathf.Clamp(mousePos.x, -xLimit, xLimit);

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

        transform.position = new Vector3(0f, transform.position.y, 0f);

        visuals.ResetVisualState();
        queue.Reset();
        visuals.RenderQueue(queue.CurrentElement, queue.NextElements, false);
    }
}