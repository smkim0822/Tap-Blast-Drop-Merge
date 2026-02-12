using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    RectTransform rectTransform;
    Rect safeArea;
    Vector2 minAnchor;
    Vector2 maxAnchor;

    // 변화 감지용 변수
    Rect lastSafeArea = new Rect(0, 0, 0, 0);
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

    void Refresh()
    {
        safeArea = Screen.safeArea;

        // 현재 상태 저장
        lastSafeArea = safeArea;

        minAnchor = safeArea.position;
        maxAnchor = minAnchor + safeArea.size;

        minAnchor.x /= Screen.width;
        minAnchor.y /= Screen.height;
        maxAnchor.x /= Screen.width;
        maxAnchor.y /= Screen.height;

        rectTransform.anchorMin = minAnchor;
        rectTransform.anchorMax = maxAnchor;

        // 오프셋 0으로 초기화
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
