#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Unity가 프로젝트 폴더명 등으로 productName을 되돌리는 경우가 있어,
/// 예전/기본값일 때만 공식 타이틀로 맞춥니다.
/// </summary>
[InitializeOnLoad]
static class ProductNameEditorGuard
{
    const string Expected = "Tap Blast: Drop & Merge";

    static ProductNameEditorGuard()
    {
        EditorApplication.delayCall += EnsureProductName;
    }

    static void EnsureProductName()
    {
        var current = PlayerSettings.productName;
        if (current == Expected)
            return;

        if (current == "Project-Drop" || current == "POP POP Blast" || string.IsNullOrWhiteSpace(current))
        {
            PlayerSettings.productName = Expected;
        }
    }
}
#endif
