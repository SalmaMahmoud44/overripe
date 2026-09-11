using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip levelMusic;
    [SerializeField] private AudioClip bossMusic;

    [Header("Boss SFX")]
     public AudioClip mangoSmash;
     public AudioClip mangoDeath;

    [Header("Player SFX")]
     public AudioClip footstepClip;
     public AudioClip jumpClip;
     public AudioClip meleeClip;
   

    [Header("Interaction SFX")]
     public AudioClip juiceClip;
     public AudioClip juiceHit;
     public AudioClip artifactCollect;

    [Header("Music Settings")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;

    [Header("SFX Settings")]
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource.playOnAwake = false;

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    public void PlayMusic(AudioClip music)
    {
        if (music == null)
            return;

        if (musicSource.clip == music && musicSource.isPlaying)
            return;

        musicSource.clip = music;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
        musicSource.clip = null;
    }

    public void PlayMainMenuMusic()
    {
        PlayMusic(mainMenuMusic);
    }

    public void PlayLevelMusic()
    {
        PlayMusic(levelMusic);
    }

    public void PlayBossMusic()
    {
        PlayMusic(bossMusic);
    }



    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
            return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayFootstep()
    {
        sfxSource.PlayOneShot(footstepClip);
    }

    public void PlayJump()
    {
        sfxSource.PlayOneShot(jumpClip);
    }

    public void PlayMelee()
    {
        sfxSource.PlayOneShot(meleeClip);
    }
    public void PlayJuiceSound()
    {
        sfxSource.PlayOneShot(juiceClip);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

}
