using UnityEngine;

public class CameraResolution : MonoBehaviour
{
    void Start() // Awake 대신 Start 사용 (화면 크기 인식 안정화)
    {
        Camera cam = GetComponent<Camera>();

        // 1. 목표 비율 (9:16)
        float targetAspect = 9.0f / 16.0f;

        // 2. 현재 기기 비율
        float windowAspect = (float)Screen.width / Screen.height;

        // 3. 비율 계산
        float scaleHeight = windowAspect / targetAspect;

        Rect rect = cam.rect;

        // 오차 범위(0.001)를 두어 거의 같으면 그냥 꽉 채움
        if (Mathf.Abs(scaleHeight - 1.0f) < 0.001f)
        {
            rect.width = 1.0f;
            rect.height = 1.0f;
            rect.x = 0;
            rect.y = 0;
        }
        else if (scaleHeight < 1.0f) // 세로로 더 긴 경우 (레터박스 위아래)
        {
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else // 가로로 더 넓은 경우 (레터박스 좌우 - 필러박스)
        {
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        cam.rect = rect;
    }
}