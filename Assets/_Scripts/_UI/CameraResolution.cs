using UnityEngine;

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

        if (scaleHeight < 1.0f)
        {
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        cam.rect = rect;
    }

    void OnPreCull()
    {
        GL.Clear(true, true, Color.black);
    }
}