using UnityEngine;

/// <summary>
/// 기기의 해상도 비율이 달라도 게임 화면이 찌그러지거나 UI가 화면 밖으로 나가지 않도록,
/// 카메라의 Viewport Rect를 9:16 비율로 고정하고 남는 공간을 레터박스로 채우는 스크립트.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraResolution : MonoBehaviour
{
    private void Awake()
    {
        Camera cam = GetComponent<Camera>();

        // 목표 비율 (9:16 = 0.5625)
        float targetAspect = 9.0f / 16.0f;

        // 현재 기기 비율
        float windowAspect = (float)Screen.width / (float)Screen.height;

        // 비율 계산 (현재 비율 / 목표 비율)
        float scaleHeight = windowAspect / targetAspect;

        Rect rect = cam.rect;

        // 기기 화면이 목표 비율보다 세로로 길 때 (상하에 검은 띠 생성)
        if (scaleHeight < 1.0f)
        {
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;   // 화면 중앙 정렬
        }
        // 기기 화면이 목표 비율보다 가로로 길 때 (좌우에 검은 띠 생성)
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;    // 화면 중앙 정렬
            rect.y = 0;
        }

        cam.rect = rect;
    }

    /// <summary>
    /// 카메라가 화면을 렌더링하기 직전에 호출.
    /// 카메라의 렌더링 영역 밖의 남는 빈 공간을 검은색으로 칠해 레터박스를 완성.
    /// </summary>
    void OnPreCull()
    {
        GL.Clear(true, true, Color.black);
    }
}