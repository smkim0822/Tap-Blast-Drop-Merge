using UnityEngine;

public class CameraResolution : MonoBehaviour
{
    private void Awake()
    {
        Camera cam = GetComponent<Camera>();

        // 1. 목표 비율 (9:16 = 0.5625)
        float targetAspect = 9.0f / 16.0f;

        // 2. 현재 기기 비율
        float windowAspect = (float)Screen.width / (float)Screen.height;

        // 3. 비율 계산 (현재 비율 / 목표 비율)
        float scaleHeight = windowAspect / targetAspect;

        // 4. 뷰포트(Viewport) 조정으로 레터박스/필러박스 구현
        if (scaleHeight < 1.0f)
        {
            // 화면이 더 길쭉함 (아이폰X, 갤럭시 S시리즈 등) -> 위아래 레터박스(Letterbox)
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f; // 중앙 정렬
            cam.rect = rect;
        }
        else
        {
            // 화면이 더 넓음 (태블릿 등) -> 좌우 필러박스(Pillarbox)
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f; // 중앙 정렬
            rect.y = 0;
            cam.rect = rect;
        }
    }

    // 잔상(Ghosting) 제거를 위한 GL Clear
    // 뷰포트 밖의 영역(검은 여백)을 강제로 검은색으로 밀어버림
    void OnPreCull()
    {
        GL.Clear(true, true, Color.black);
    }
}