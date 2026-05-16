using UnityEngine;

// 게임 전체의 배경음악(BGM)과 효과음(SFX)을 통합 관리하는 매니저
public class SoundManager : MonoBehaviour
{
    // 어디서든 SoundManager.Instance 로 접근할 수 있게 해주는 마법의 코드 (싱글톤)
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource; // 배경음악 전용 스피커
    public AudioSource sfxSource; // 효과음 전용 스피커

    void Awake()
    {
        // 씬(Scene)이 바뀌어도 파괴되지 않고 하나만 유지되도록 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 효과음 재생 함수 (동시에 여러 소리가 겹쳐서 날 수 있음)
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // 배경음악 재생 함수 (기존 음악을 끄고 새로운 음악을 반복 재생)
    public void PlayBGM(AudioClip clip)
    {
        if (clip != null && bgmSource != null)
        {
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }
}