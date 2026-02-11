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

    [Header("Settings")]
    [Tooltip("생성할 폭탄 프리팹")]
    public GameObject bombPrefab;

    [Tooltip("화면 좌우 이동 제한 범위 (X축)")]
    public float xLimit = 1.6f;

    [Tooltip("재장전 시간")]
    public float cooldownTime = 0.5f;

    [Header("Elements")]
    public ElementData[] elements;

    [Header("UI References")]
    [Tooltip("다음 폭탄 색상 UI 이미지")]
    public Image[] nextBombUIs;
    
    [Header("Guide Line")]
    [Tooltip("라인 렌더러 컴포넌트 연결")]
    public LineRenderer guideLine;

    [Tooltip("표시선이 바닥에 닿을 바닥의 Y좌표")]
    public float groundY = -4.5f;

    [Tooltip("점선 간격 조절")]
    public float lineTextureScale = 1;

    private Camera mainCam;
    private bool isDragging = false;
    private bool isReloading = false;

    private SpriteRenderer spawnerRenderer;
    private ElementData currentElement;
    private List<ElementData> nextElements = new List<ElementData>();

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

        // 표시선이 테두리 위로 보이는 문제
        // 표시선을 강제로 맨 뒤로 보냄
        guideLine.sortingOrder = -10;

        // 게임 시작 시 초기화
        InitializeQueue();
        PrepareNextBomb();
    }

    void Update()
    {
        if (Time.timeScale == 0) return;

        if (IsPointerOverUI()) return;

        if (isReloading) return;

        HandleInput();

        // 드래그 중일 때만 표시선 표시
        if (isDragging) DrawGuideLine();
        else guideLine.enabled = false;
    }

    // 입력이 UI 위인지 판별하는 함수
    private bool IsPointerOverUI()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return true;

        if (Input.touchCount > 0)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return true;
        }

        return false;
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

        Color guideColor = currentElement.color;
        guideLine.startColor = guideColor;
        guideColor.a = 0.3f;
        guideLine.endColor = guideColor;
    }

    IEnumerator DropBombRoutine()
    {
        // 폭탄 생성
        if (bombPrefab != null)
        {
            GameObject newBomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);

            BombLogic bombLogic = newBomb.GetComponent<BombLogic>();
            
            if (bombLogic != null)
            {
                bombLogic.Initialize(currentElement.sprite, currentElement.color);
            }
        }

        // 재장전 시작
        isReloading = true;
        spawnerRenderer.enabled = false;

        // 장전 대기시간
        yield return new WaitForSeconds(cooldownTime);

        // 재장전 완료
        PrepareNextBomb();

        spawnerRenderer.enabled = true;
        isReloading = false;
    }

    // 대기열 3개 채우기
    void InitializeQueue()
    {
        nextElements.Clear();
        for (int i = 0; i < nextBombUIs.Length; i++)
        {
            nextElements.Add(PickRandomElement());
        }
    }

    // 다음 폭탄을 현재 폭탄으로 가져오고, 새로운 다음 폭탄을 뽑는 함수
    void PrepareNextBomb()
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
        UpdateVisuals();
    }

    ElementData PickRandomElement()
    {
        if (elements.Length > 0)
        {
            int randomIndex = Random.Range(0, elements.Length);
            return elements[randomIndex];
        }
        return new ElementData();
    }

    void UpdateVisuals()
    {
        // 스포너 이미지 변경
        if (spawnerRenderer != null)
        {
            spawnerRenderer.sprite = currentElement.sprite;
            spawnerRenderer.color = Color.white;
        }

        // UI 업데이트
        for (int i = 0; i < nextBombUIs.Length; i++)
        {
            if (i < nextElements.Count)
            {
                nextBombUIs[i].sprite = nextElements[i].sprite;

                Color uiColor = Color.white;

                bool isNextBomb = (i == nextElements.Count - 1);

                if (isNextBomb)
                {
                    uiColor.a = 1.0f;
                    nextBombUIs[i].transform.localScale = Vector3.one * 1.2f;
                }
                else
                {
                    uiColor.a = 0.5f;
                    nextBombUIs[i].transform.localScale = Vector3.one * 0.8f;
                }
                
                nextBombUIs[i].color = uiColor;
            }
        }
    }

    // 초기화
    public void ResetSpawner()
    {
        // 1. 위치 초기화
        transform.position = new Vector3(0, transform.position.y, 0);

        // 2. 상태 초기화
        StopAllCoroutines();
        isDragging = false;
        isReloading = false;
        spawnerRenderer.enabled = true;
        guideLine.enabled = false;

        // 3. 폭탄 초기화
        InitializeQueue();
        PrepareNextBomb();
    }
}