using UnityEngine;

/// <summary>
/// 모바일 기기의 노치, 펀치홀, 홈 바 등으로 인해
/// UI가 가려지는 현상을 방지하기 위해, 패널을 안전 영역에 맞춰주는 스크립트.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    // --- 내부 캐싱 및 계산용 변수 ---
    /// <summary>크기를 조절할 대상 컴포넌트</summary>
    RectTransform rectTransform;
    /// <summary>현재 화면의 안전 영역 데이터를 담을 구조체</summary>
    Rect safeArea;
    /// <summary>좌측 하단 앵커 비율</summary>
    Vector2 minAnchor;
    /// <summary>우측 상단 앵커 비율</summary>
    Vector2 maxAnchor;

    /// <summary>
    /// 기기 변형으로 인한 화면 변화를 감지하기 위헤,
    /// 이전 프레임의 안전 영역 크기를 저장하는 변수.
    /// </summary>
    Rect lastSafeArea = new Rect(0, 0, 0, 0);

    /// <summary>
    /// RectTransform 컴포넌트 캐싱, 안전 영역 즉시 1회 적용.
    /// </summary>
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Refresh();
    }

    void Update()
    {
        // Safe Area 영역이 바뀌었다면
        if (lastSafeArea != Screen.safeArea)
        {
            Refresh();
        }
    }

    /// <summary>
    /// 실제 기기의 Safe Area를 가져와서 UI 앵커에 맞게 백분율로 변환한 뒤,
    /// RectTransform의 크기를 조절하는 함수.
    /// </summary>
    void Refresh()
    {
        // 현재 기기의 Safe Area 픽셀 값
        safeArea = Screen.safeArea;

        // 현재 상태 저장
        lastSafeArea = safeArea;

        // Safe Area의 절대 좌표
        minAnchor = safeArea.position;
        maxAnchor = minAnchor + safeArea.size;

        // 절대 픽셀 좌표 정규화
        minAnchor.x /= Screen.width;
        minAnchor.y /= Screen.height;
        maxAnchor.x /= Screen.width;
        maxAnchor.y /= Screen.height;

        // 계산된 비율을 RectTransform의 Anchor에 적용.
        rectTransform.anchorMin = minAnchor;
        rectTransform.anchorMax = maxAnchor;

        // 오프셋 0으로 초기화
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
