using UnityEngine;

public class GameOverZone : MonoBehaviour
{
    private float timer = 0f;
    private bool isDanger = false;  // 위험 상태 플래그

    void Update()
    {
        if (isDanger)
        {
            timer += Time.deltaTime;
            Debug.Log($"Game Over Timer: {timer}");

            if (timer > 0.5f)
            {
                Debug.Log("Game Over Triggerd!");
                GameManager.instance.TriggerGameOver();
                isDanger = false;
            }
        }
        else
        {
            timer = 0f;
        }
    }

    // 닿기 시작하면 위험 상태 ON
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Stack"))
        {
            isDanger = true;
        }
    }

    // 떨어지면 위험 상태 OFF
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Stack"))
        {
            isDanger = false;
        }
    }
}