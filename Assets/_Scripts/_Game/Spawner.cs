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

    [Header("UI References (UI 연결)")]
    [Tooltip("대기열 폭탄 색상을 표시할 UI 이미지 배열")]
    public Image[] nextBombUIs;
    
    [Header("Guide Line (표시선 설정)")]
    [Tooltip("라인 렌더러 컴포넌트")]
    public LineRenderer guideLine;

    [Tooltip("표시선이 닿을 바닥의 Y좌표")]
    public float groundY = -4.5f;

    [Tooltip("점선 간격 조절")]
    public float lineTextureScale = 1f;

    [Header("Animation Settings (애니메이션 곡선 및 시간)")]
    public AnimationCurve spawnCurve;
    public float spawnDuration = 0.3f;

    public AnimationCurve spawnerLoadCurve;
    public float spawnerLoadDuration = 0.3f;

    public AnimationCurve uiPunchCurve;
    public float uiAnimDuration = 0.2f;

    // 내부 상태 변수
    private Camera mainCam;
    private bool isDragging = false;
    private bool isReloading = false;

    // 컴포넌트 및 캐싱 변수
    private SpriteRenderer spawnerRenderer;
    private Vector3 originalScale;

    // 데이터 큐
    private ElementData currentElement;
    private List<ElementData> nextElements = new List<ElementData>();

    void Awake()
    {
        mainCam = Camera.main;
        spawnerRenderer = GetComponent<SpriteRenderer>();

        if (guideLine == null)
        {
            guideLine = GetComponent<LineRenderer>();
        }

        originalScale = transform.localScale;
        if (originalScale == Vector3.zero)
        {
            originalScale = Vector3.one;
        }
    }

    void Start()
    {
        InitializeGuideLine();
        InitializeQueue();
        PrepareNextBomb(false);
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
            DrawGuideLine();
        }
        else if (guideLine.enabled)
        {
            guideLine.enabled = false;
        }
    }

    /// <summary>
    /// 표시선 렌더러의 초기 속성 설정
    /// </summary>
    /// <returns></returns>
    private void InitializeGuideLine()
    {
        guideLine.positionCount = 2;
        guideLine.enabled = false;
        guideLine.textureMode = LineTextureMode.Tile;
        guideLine.sortingOrder = 5;
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
    /// 목표 지점까지의 표시선을 그리고 텍스처 배율을 조절
    /// </summary>
    void DrawGuideLine()
    {
        Vector3 currentPos = transform.position;
        Vector3 endPos = new Vector3(currentPos.x, groundY, 0f);

        // 시작점: 스포너의 현재 위치
        guideLine.SetPosition(0, currentPos);

        // 끝점: X는 스포너와 같고, Y는 바닥까지
        guideLine.SetPosition(1, endPos);

        float distance = Vector3.Distance(currentPos, endPos);
        
        guideLine.material.mainTextureScale = new Vector2(distance * lineTextureScale, 1f);

        Color guideColor = currentElement.color;
        guideLine.startColor = guideColor;
        guideColor.a = 0.3f;
        guideLine.endColor = guideColor;
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
            GameObject newBomb = PoolManager.Instance.Spawn(bombPrefab, transform.position, Quaternion.identity);

            // 초기 크기 지정 후 코루틴 애니메이션
            Vector3 realScale = newBomb.transform.localScale;
            newBomb.transform.localScale = Vector3.zero;
            StartCoroutine(AnimateScale(newBomb.transform, spawnCurve, spawnDuration, realScale));

            if (newBomb.TryGetComponent(out BombLogic bombLogic))
            {
                bombLogic.Initialize(currentElement.sprite, currentElement.color);
            }
        }

        // 재장전 시작
        isReloading = true;
        if (spawnerRenderer != null)
        {
            spawnerRenderer.enabled = false;  
        } 

        // 장전 대기시간
        yield return new WaitForSeconds(cooldownTime);

        // 재장전 완료
        PrepareNextBomb(true);

        if (spawnerRenderer != null)
        {
            spawnerRenderer.enabled = true;
        }
        isReloading = false;
    }

    /// <summary>
    /// 대기열을 지정된 UI 개수만큼 무작위 원소로 초기화
    /// </summary>
    private void InitializeQueue()
    {
        nextElements.Clear();
        for (int i = 0; i < nextBombUIs.Length; i++)
        {
            nextElements.Add(PickRandomElement());
        }
    }

    /// <summary>
    /// 대기열의 마지막 원소를 현재 원소로 가져오고, 새로운 원소를 대기열 앞에 추가
    /// </summary>
    void PrepareNextBomb(bool playAnim = true)
    {
        if (nextElements.Count == 0)
        {
            InitializeQueue();
        }

        int lastIndex = nextElements.Count - 1;
        currentElement = nextElements[lastIndex];

        nextElements.RemoveAt(lastIndex);
        nextElements.Insert(0, PickRandomElement());

        // 시각적 업데이트
        UpdateVisuals(playAnim);
    }

    private ElementData PickRandomElement()
    {
        if (elements != null && elements.Length > 0)
        {
            return elements[Random.Range(0, elements.Length)];
        }
        return new ElementData();
    }

    /// <summary>
    /// 스포너와 UI의 스프라이트 및 애니메이션 갱신
    /// </summary>
    void UpdateVisuals(bool playAnim)
    {
        // 스포너 이미지 변경
        if (spawnerRenderer != null)
        {
            spawnerRenderer.sprite = currentElement.sprite;
            spawnerRenderer.color = Color.white;

            if (playAnim)
            {
                StartCoroutine(AnimateScale(transform, spawnerLoadCurve, spawnerLoadDuration, originalScale));
            }
            else
            {
                transform.localScale = originalScale;
            }            
        }

        // UI 업데이트
        for (int i = 0; i < nextBombUIs.Length; i++)
        {
            if (i < nextElements.Count)
            {
                Image targetUI = nextBombUIs[i];
                targetUI.sprite = nextElements[i].sprite;

                bool isNextBomb = (i == nextElements.Count - 1);

                Color uiColor = Color.white;
                uiColor.a = isNextBomb ? 1.0f : 0.5f;
                targetUI.color = uiColor;

                float targetScale = 0.8f;

                if (playAnim)
                {
                    StartCoroutine(AnimateScale(targetUI.rectTransform, uiPunchCurve, uiAnimDuration, Vector3.one * targetScale));
                }
                else
                {
                    targetUI.rectTransform.localScale = Vector3.one * targetScale;
                }
            }
        }
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
        transform.localScale = originalScale;

        if (spawnerRenderer == null)
        {
            spawnerRenderer = GetComponent<SpriteRenderer>();
        }
        if (spawnerRenderer != null)
        {
            spawnerRenderer.enabled = true;
        }

        InitializeQueue();
        PrepareNextBomb(false);
    }

    /// <summary>
    /// Transform의 스케일을 지정된 Curve와 시간에 맞춰 애니메이션을 처리하는 통합 코루틴
    /// </summary>
    /// <param name="target"></param>
    /// <param name="targetScale"></param>
    /// <returns></returns>
    private IEnumerator AnimateScale(Transform targetTransform, AnimationCurve curve, float duration, Vector3 finalScale)
    {
        if (targetTransform == null) yield break;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            if (targetTransform != null)
            {
                targetTransform.localScale = finalScale * curve.Evaluate(progress);
            }
            yield return null;
        }

        if (targetTransform != null)
        {
            targetTransform.localScale = finalScale;
        }
    }
}