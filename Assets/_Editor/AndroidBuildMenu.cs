#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Android Release APK 빌드 메뉴 (#117).
/// </summary>
public static class AndroidBuildMenu
{
    const string ApkPath = "Builds/Android/TapBlast-DropMerge.apk";

    [MenuItem("Dev/Build/Android APK (Release)")]
    public static void BuildReleaseApk()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Android,
                BuildTarget.Android);
        }

        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.buildAppBundle = false;

        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("[Build] Editor Build Settings에 활성 씬이 없습니다.");
            return;
        }

        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), ApkPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        var options = BuildOptions.CompressWithLz4;
        BuildReport report = BuildPipeline.BuildPlayer(scenes, fullPath, BuildTarget.Android, options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[Build] 성공: {fullPath} ({report.summary.totalSize / (1024 * 1024)} MB)");
        }
        else
        {
            Debug.LogError($"[Build] 실패: {report.summary.result}");
        }
    }
}
#endif
