using UnityEngine;
using System.Collections.Generic;

public class GameOverZone : MonoBehaviour
{
    public float limitTime = 0.5f;
    private float timer = 0f;

    private List<GameObject> stackList = new List<GameObject>();

    void Update()
    {
        stackList.RemoveAll(item => item == null);

        if (stackList.Count  > 0)
        {
            timer += Time.deltaTime;

            Debug.Log($"game Over Timer: {timer:F2}");

            if (timer > limitTime)
            {
                Debug.Log("Game Over Triggered!");
                GameManager.instance.TriggerGameOver();

                stackList.Clear();
                timer = 0f;
            }
        }
        else
        {
            timer = 0f;
        }
    }

    // 닿으면 명단에 추가
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Stack"))
        {
            if (!stackList.Contains(other.gameObject))
            {
                stackList.Add(other.gameObject);
            }
        }
    }

    // 나가면 명단에서 삭제
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Stack"))
        {
            stackList.Remove(other.gameObject);
        }
    }
}