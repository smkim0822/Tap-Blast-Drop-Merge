using UnityEngine;
using UnityEngine.EventSystems;

public class DragSpawner : MonoBehaviour
{
    public GameObject bombPrefab;
    public Color[] grainColors;
    public float xRange =  2.5f;

    private bool isDragging = false;

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // 터치 위치를 월드 좌표로 반환
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        mousePos.y = transform.position.y;

        // 범위 제한
        mousePos.x = Mathf.Clamp(mousePos.x, -xRange, xRange);

        // 드래그 시작
        if (Input.GetMouseButton(0))
        {
            isDragging = true;
            transform.position = mousePos;
        }

        // 드래그 중
        if (Input.GetMouseButton(0) && isDragging)
        {
            transform.position = mousePos;
        }

        // 드래그 끝
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            DropBomb();
        }
    }

    void DropBomb()
    {
        GameObject newBomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);

        if (grainColors.Length > 0)
        {
            SpriteRenderer sr = newBomb.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = grainColors[Random.Range(0, grainColors.Length)];
            }
        }
    }
}
