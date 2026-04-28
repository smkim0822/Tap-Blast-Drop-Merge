#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class DevMenu : MonoBehaviour
{
    [MenuItem("Dev/Clear Best Score")]
    public static void ClearBestScore()
    {
        BestScoreStore.Clear();
        Debug.Log("최고 기록 초기화.");
    }
}
#endif