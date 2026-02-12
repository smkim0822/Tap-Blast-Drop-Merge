using UnityEngine;

public class CameraResolution : MonoBehaviour
{
    private void Awake()
    {
        Camera cam = GetComponent<Camera>();

        // 1. 목표 비율 (9:16)
        float targetAspect = 9.0f / 16.0f;

        // 2. 현재 기기 비율
        float windowAspect = (float)Screen.width / Screen.height;

        // 3. 비율에 따른 카메라 사이즈 보정
        // (기존 Size 5를 기준으로 계산)
        float initialSize = 5f;

        if (windowAspect < targetAspect) 
        {
            // 화면이 더 홀쭉함 (길쭉한 폰) -> 너비를 맞추기 위해 카메라를 뒤로 뺌(Size 증가)
            cam.orthographicSize = initialSize * (targetAspect / windowAspect);
        }
        else
        {
            // 화면이 더 넓음 (태블릿) -> 높이를 5로 고정하면 너비가 넓어짐 (커버가 보임)
            cam.orthographicSize = initialSize;
        }

        // 4. Viewport Rect는 항상 전체 화면 사용 (잔상 문제 해결)
        cam.rect = new Rect(0, 0, 1, 1);
    }
}