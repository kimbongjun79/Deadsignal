using UnityEngine;
using UnityEngine.UI;

// 스태미나 게이지 UI 표시
public class StaminaUI : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [Tooltip("스태미나 게이지 UI Image (Fill Type: Filled)")]
    [SerializeField] private Image staminaFillImage;

    private void Update()
    {
        if (player == null || staminaFillImage == null) return;
        if (staminaFillImage != null)
            staminaFillImage.fillAmount = player.CurrentStamina / player.MaxStamina;   
    }
}