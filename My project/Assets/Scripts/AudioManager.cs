using UnityEngine;

// 게임 전역 사운드 재생을 담당하는 싱글톤 매니저
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Tooltip("효과음(SFX) 재생용 AudioSource")]
    [SerializeField] private AudioSource sfxSource;

    [Tooltip("배경음악(BGM) 재생용 AudioSource")]
    [SerializeField] private AudioSource bgmSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 위치와 무관한 효과음 재생 (UI 클릭, 피격 등)
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    // 특정 월드 위치에서 재생 (총소리, 좀비 소리 등 3D 위치감 필요할 때)
    public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    // BGM 교체 (씬 전환 시)
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }
}