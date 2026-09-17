using UnityEngine;
using UnityEngine.UI;

// 화면 좌측 하단에 장전된 탄약 / 예비 탄약을 표시
public class AmmoUI : MonoBehaviour
{
    [Tooltip("플레이어 컨트롤러 참조")]
    [SerializeField] private PlayerController player;

    [Tooltip("탄약 표시 텍스트")]
    [SerializeField] private Text ammoText;

    [Tooltip("재장전 중 표시 텍스트")]
    [SerializeField] private Text reloadingText;

    private void Update()
    {
        if (player == null || ammoText == null) return;

        ammoText.text = $"{player.CurrentAmmo} / {player.ReserveAmmo}";

        if (reloadingText != null)
            reloadingText.gameObject.SetActive(player.IsReloading);
    }
}
