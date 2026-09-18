using UnityEngine;

// 피격 시 셰이더의 _FlashAmount를 블렌드하여 붉은 히트 플래시 연출
public class HitFlash : MonoBehaviour
{
    [Tooltip("플래시가 유지되는 시간 (초)")]
    [SerializeField] private float flashDuration = 0.15f;

    [Tooltip("최대 플래시 강도 (0~1)")]
    [SerializeField] private float maxFlashIntensity = 0.6f;

    [Tooltip("플래시 효과를 적용할 렌더러들 (비워두면 자동으로 자식에서 찾음)")]
    [SerializeField] private Renderer[] renderers;

    
    private MaterialPropertyBlock propBlock;
    private static readonly int FlashAmountID = Shader.PropertyToID("_FlashAmount");
    private Coroutine flashRoutine;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        propBlock = new MaterialPropertyBlock();
    }

    // 외부(피격 판정 지점)에서 호출
    public void Flash()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        float t = 0f;

        while (t < flashDuration)
        {
            t += Time.deltaTime;
            float amount = maxFlashIntensity * (1f - (t / flashDuration));
            SetFlashAmount(amount);
            yield return null;
        }

        SetFlashAmount(0f);
        flashRoutine = null;
    }

    private void SetFlashAmount(float amount)
    {
        foreach (Renderer r in renderers)
        {
            r.GetPropertyBlock(propBlock);
            propBlock.SetFloat(FlashAmountID, amount);
            r.SetPropertyBlock(propBlock);
        }
    }
}