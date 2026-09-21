using UnityEngine;
using TMPro;

// 플레이어의 보유 수류탄 개수를 화면에 표시
public class GrenadeUI : MonoBehaviour
{
    [Tooltip("플레이어 컨트롤러 참조")]
    [SerializeField] private PlayerController player;

    [Tooltip("수류탄 개수 표시 텍스트")]
    [SerializeField] private TextMeshProUGUI grenadeCountText;

    private void Update()
    {
        if (player == null || grenadeCountText == null) return;

        grenadeCountText.text = $" {player.GrenadeCount}";
    }
}