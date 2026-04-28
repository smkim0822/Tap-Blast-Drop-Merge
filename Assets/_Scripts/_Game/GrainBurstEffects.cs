using UnityEngine;

/// <summary>
/// 알갱이 파괴 시 점수 이펙트와 사운드 재생을 담당합니다.
/// </summary>
[DisallowMultipleComponent]
public class GrainBurstEffects : MonoBehaviour
{
    [Header("Effects (이펙트)")]
    [SerializeField] AudioClip popSound;
    [SerializeField] GameObject explosionEffectPrefab;
    [SerializeField] GameObject floatingTextPrefab;

    public void ConfigureIfMissing(AudioClip sound, GameObject explosionPrefab, GameObject floatingPrefab)
    {
        if (popSound == null) popSound = sound;
        if (explosionEffectPrefab == null) explosionEffectPrefab = explosionPrefab;
        if (floatingTextPrefab == null) floatingTextPrefab = floatingPrefab;
    }

    public void Play(Vector3 position, Color effectColor, int finalScore)
    {
        if (popSound != null)
        {
            AudioEvents.OnPlaySfx?.Invoke(popSound, 1.0f);
        }

        if (explosionEffectPrefab != null)
        {
            GameObject psObj = PoolService.Spawn(explosionEffectPrefab, position, Quaternion.identity);
            if (psObj.TryGetComponent(out ParticleSystem ps))
            {
                var main = ps.main;
                main.startColor = effectColor;
                ps.Play();
                PoolService.Despawn(psObj, main.startLifetime.constantMax + 0.1f);
            }
        }

        if (floatingTextPrefab != null)
        {
            GameObject go = PoolService.Spawn(floatingTextPrefab, position, Quaternion.identity);
            if (go.TryGetComponent(out FloatingText floatingText))
            {
                floatingText.SetText("+" + finalScore, effectColor);
            }
        }
    }
}
