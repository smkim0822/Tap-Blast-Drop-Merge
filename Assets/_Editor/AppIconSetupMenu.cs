#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// image_gameicon.png 를 Default·Android 플랫폼 아이콘 슬롯에 일괄 적용 (#118).
/// </summary>
public static class AppIconSetupMenu
{
    const string IconPath = "Assets/_Materials/image_gameicon.png";

    [MenuItem("Dev/Apply Game App Icon (Android + Default)")]
    public static void ApplyGameAppIcon()
    {
        Texture2D source = LoadIconTexture();
        if (source == null)
        {
            Debug.LogError($"[AppIcon] 텍스처를 찾을 수 없습니다: {IconPath}");
            return;
        }

        PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { source }, IconKind.Application);

        ApplyAndroidIcons(source, IconKind.Application);
        ApplyAndroidIcons(source, IconKind.Round);
        ApplyAndroidIcons(source, IconKind.Adaptive);

        AssetDatabase.SaveAssets();
        Debug.Log("[AppIcon] Default + Android(Application/Round/Adaptive) 아이콘 적용 완료. Player Settings에서 확인하세요.");
    }

    static Texture2D LoadIconTexture()
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(IconPath);
        Sprite sprite = assets.OfType<Sprite>().FirstOrDefault(s => s.name == "image_gameicon_0")
            ?? assets.OfType<Sprite>().FirstOrDefault();
        if (sprite != null)
        {
            return sprite.texture;
        }

        return AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
    }

    static void ApplyAndroidIcons(Texture2D source, IconKind kind)
    {
        int[] sizes = PlayerSettings.GetIconSizes(NamedBuildTarget.Android, kind);
        if (sizes == null || sizes.Length == 0)
        {
            return;
        }

        Texture2D[] icons = new Texture2D[sizes.Length];
        for (int i = 0; i < icons.Length; i++)
        {
            icons[i] = source;
        }

        PlayerSettings.SetIcons(NamedBuildTarget.Android, icons, kind);
    }
}
#endif
